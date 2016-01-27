﻿using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using GooglePubSub;
using Newtonsoft.Json;

namespace Bootstrap
{
    /// <summary>
    /// Main class for the Bootstrapper
    /// </summary>
    public static partial class Program
    {
        private static string Host;

        private static string Role
        {
            get
            {
                if (BasePath == null)
                {
                    return "...";
                }

                if (File.Exists(Path.Combine(BasePath, "role.txt")))
                {
                    using (var reader = new StreamReader(Path.Combine(BasePath, "role.txt")))
                    {
                        var role = reader.ReadToEnd().Trim();
                        if (role != "Client" && role != "Projector")
                        {
                            return "Client";
                        }
                        return role;
                    }
                }

                return "Client";
            }
            set
            {
                using (var writer = new StreamWriter(Path.Combine(BasePath, "role.txt")))
                {
                    writer.Write(value);
                }
            }
        }

        private static Package Client;

        private static Package Projector;

        private static Package Bootstrap;

        private static string BasePath;

        private static bool HasReceivedVersionInformation;

        private static string _oAuthEndpoint;

        private static PubSub PubSub;

        private static Package Active
        {
            get
            {
                if (Role == "Client")
                {
                    return Client;
                }
                else
                {
                    return Projector;
                }
            }
        }

        private static string GetLocalIPAddress()
        {
            IPHostEntry host;
            var localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        private static IPAddress[] GetAllKnownIPAddresses()
        {
            try
            {
                return Dns.GetHostAddresses("");
            }
            catch (Exception)
            {
                return new IPAddress[0];
            }
        }

        internal static void RealMain(string[] args)
        {
            int? processToKillOnSuccess = null;
            if (args.Length >= 1)
            {
                processToKillOnSuccess = int.Parse(args[0]);
            }

            Status = "Initializing";

            PlatformTraySetup();

            Status = "Reading Configuration";

            string project;
            var projectStream = typeof(Program).Assembly.GetManifestResourceStream("Bootstrap.project.txt");
            using (var reader = new StreamReader(projectStream))
            {
                project = reader.ReadToEnd().Trim();
            }
            var endpointStream = typeof(Program).Assembly.GetManifestResourceStream("Bootstrap.endpoint.txt");
            using (var reader = new StreamReader(endpointStream))
            {
                _oAuthEndpoint = reader.ReadToEnd().Trim();
            }

            if (string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(_oAuthEndpoint))
            {
                throw new InvalidOperationException("This bootstrap is not configured correctly.");
            }

            Status = "Setting Up Packages";

            // Create folder for storing client, projector and bootstrap software.
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JamCast");
            Directory.CreateDirectory(path);
            BasePath = path;

            Client = new Package(BasePath, "Client");
            Projector = new Package(BasePath, "Projector");
            Bootstrap = new Package(BasePath, "Bootstrap");

            // Check whether we're running from one of the blue-green folders
            // from the bootstrap package, and set the active mode appropriately
            // (so that when the bootstrap is updated, it gets written out to the
            // other directory).
            var directoryName = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
            if (directoryName != null)
            {
                if (directoryName.StartsWith(Bootstrap.BluePath))
                {
                    Bootstrap.ActiveMode = "Blue";
                }
                else if (directoryName.StartsWith(Bootstrap.GreenPath))
                {
                    Bootstrap.ActiveMode = "Green";
                }
            }

            // Preemptively calculate the current version?
            Bootstrap.Version = Bootstrap.CalculatePackageVersion();

            Status = "Querying Computer GUID";

            Guid guid;
            if (File.Exists(Path.Combine(BasePath, "guid.txt")))
            {
                using (var reader = new StreamReader(Path.Combine(BasePath, "guid.txt")))
                {
                    guid = Guid.Parse(reader.ReadToEnd().Trim());
                }
            }
            else
            {
                guid = Guid.NewGuid();
                using (var writer = new StreamWriter(Path.Combine(BasePath, "guid.txt")))
                {
                    writer.Write(guid.ToString());
                }
            }

            Status = "Querying Hostname";

            try
            {
                Host = Dns.GetHostEntry("").HostName;
            }
            catch
            {
                try
                {
                    Host = Environment.MachineName;
                }
                catch
                {
                    try
                    {
                        Host = GetLocalIPAddress();
                    }
                    catch
                    {
                        Host = "Unknown - " + guid;
                    }
                }
            }

            Status = "Killing Previous Process";

            try
            {
                if (processToKillOnSuccess != null)
                {
                    Process.GetProcessById(processToKillOnSuccess.Value).Kill();
                }
            }
            catch { }

            foreach (var process in Process.GetProcessesByName("Bootstrap"))
            {
                try
                {
                    if (process.Id != Process.GetCurrentProcess().Id)
                    {
                        process.Kill();
                    }
                }
                catch { }
            }

            Status = "Connecting to Pub/Sub";
            PubSub = new PubSub(project, GetOAuthToken);

            Status = "Creating Bootstrap Global Topic";
            PubSub.CreateTopic("game-jam-bootstrap");
            Status = "Creating Controller Global Topic";
            PubSub.CreateTopic("game-jam-controller");
            Status = "Creating Bootstrap Instance Topic";
            PubSub.CreateTopic("bootstrap-" + guid);
            Status = "Subscribing to Bootstrap Instance Topic";
            PubSub.Subscribe("bootstrap-" + guid, "bootstrap-" + guid);
            Status = "Subscribing to Bootstrap Global Topic";
            PubSub.Subscribe("game-jam-bootstrap", "bootstrap-" + guid);

            var pingTopic = "game-jam-controller";

            var thread = new Thread(() =>
            {
                while (true)
                {
                    Status = "Waiting for Messages";

                    try
                    {
                        var messages = PubSub.Poll(10, false);
                        foreach (var message in messages)
                        {
                            message.Acknowledge();

                            LastContact = DateTime.Now;

                            try
                            {
                                var m =
                                    JsonConvert.DeserializeObject<dynamic>(
                                        Encoding.ASCII.GetString(Convert.FromBase64String(message.Data)));
                                var target = (string) m.Target;

                                if (!string.IsNullOrEmpty(target))
                                {
                                    if (target != guid.ToString())
                                    {
                                        // not for this client.
                                        return;
                                    }
                                }

                                Debug.WriteLine("GOT MESSAGE: " + (string)m.Type);
                                switch ((string) m.Type)
                                {
                                    case "pong":
                                    {
                                        HasReceivedVersionInformation = true;
                                            
                                        Status = "Got Pong (Sending Ping)";
                                        SendPing(PubSub, pingTopic, guid);
                                        Status = "Sent Ping";

                                        Client.SetAvailableVersions(
                                            (string) m.AvailableClientVersion,
                                            (string) m.AvailableClientFile);
                                        Projector.SetAvailableVersions(
                                            (string)m.AvailableProjectorVersion,
                                            (string)m.AvailableProjectorFile);
                                        Bootstrap.SetAvailableVersions(
                                            (string)m.AvailableBootstrapVersion,
                                            (string)m.AvailableBootstrapFile);

                                        Status = "Updating Software";
                                        UpdateVersions(Role);
                                    }
                                        break;
                                    case "designate":
                                    {
                                        var oldRole = Role;
                                        Role = m.Role;

                                        Status = "Updating Software";
                                        UpdateVersions(oldRole);
                                            
                                        Status = "Sending Ping";
                                        SendPing(PubSub, pingTopic, guid);
                                        Status = "Sent Ping";
                                    }
                                        break;
                                    case "client-settings":
                                    {
                                        using (var writer = new StreamWriter(Client.SettingsPath))
                                        {
                                            writer.Write((string) m.Settings);
                                        }

                                        Status = "Restarting Client";
                                        Client.KillProcess();
                                        Client.StartProcess();
                                            
                                        Status = "Sending Ping";
                                        SendPing(PubSub, pingTopic, guid);
                                        Status = "Sent Ping";
                                    }
                                        break;
                                    case "projector-settings":
                                    {
                                        using (var writer = new StreamWriter(Projector.SettingsPath))
                                        {
                                            writer.Write((string) m.Settings);
                                        }

                                        Status = "Restarting Projector";
                                        Projector.KillProcess();
                                        Projector.StartProcess();

                                        Status = "Sending Ping";
                                        SendPing(PubSub, pingTopic, guid);
                                        Status = "Sent Ping";
                                    }
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
                        }

                        Thread.Sleep(1);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            });

            try
            {
                PingStatus = "Starting Messaging Thread";
                thread.IsBackground = true;
                thread.Start();

                PingStatus = "Starting " + Role;
                if (!Active.StartProcess())
                {
                    Active.KillUnmonitoredProcesses();
                    Active.ExtractPackageIfExists();
                    Active.StartProcess();
                }

                PingStatus = "Sending Ping";
                SendPing(PubSub, pingTopic, guid);
                PingStatus = "Sent Ping (Idle)";

                var timer = 0;

                while (true)
                {
                    Thread.Sleep(100);
                    timer += 100;

                    if (timer > 10000)
                    {
                        PingStatus = "Sending Ping";
                        SendPing(PubSub, pingTopic, guid);
                        PingStatus = "Sent Ping (Idle)";
                        timer = 0;
                    }
                }
            }
            finally
            {
                PingStatus = "Exiting";
                thread.Abort();

                PubSub.DeleteTopic("bootstrap-" + guid);
                PubSub.Unsubscribe("bootstrap-" + guid, "bootstrap-" + guid);
                PubSub.Unsubscribe("game-jam-bootstrap", "bootstrap-" + guid);
                // Don't delete the game-jam topic because it's the global one
            }
        }

        public static DateTime? LastContact { get; set; }

        public static string Status { get; set; }
        public static string PingStatus { get; set; }

        public static OAuthToken GetOAuthToken()
        {
            var client = new WebClient();
            var jsonResult = client.DownloadString(_oAuthEndpoint);
            var json = JsonConvert.DeserializeObject<dynamic>(jsonResult);
            if (!(bool)json.has_error)
            {
                var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds((double)(json.result.created + json.result.expires_in));
                return new OAuthToken
                {
                    ExpiryUtc = dtDateTime,
                    AccessToken = json.result.access_token
                };
            }

            throw new Exception("Error when retrieving access token: " + json.error);
        }

        private static void UpdateVersions(string oldRole)
        {
            Active.UpdateVersion(Role != oldRole, d =>
            {
                Status = "Updating " + Role + " (" + (d*100).ToString("F1") + "%)";
            }).Wait();

            Bootstrap.UpdateVersion(false, d =>
            {
                Status = "Updating Bootstrap (" + (d * 100).ToString("F1") + "%)";
            }).Wait();

            Bootstrap.RestartMainProcessIfOutOfDate();
        }
        
        private static void SendPing(PubSub pubsub, string pingTopic, Guid guid)
        {
            var ipaddresses = GetAllKnownIPAddresses().Select(x => x.ToString()).ToArray();
            
            var userPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JamCast",
                "user.txt");

            string fullName = null;
            string emailAddress = null;
            if (File.Exists(userPath))
            {
                using (var reader = new StreamReader(userPath))
                {
                    fullName = reader.ReadLine()?.Trim();
                    emailAddress = reader.ReadLine()?.Trim();
                }
            }

            pubsub.Publish(pingTopic, Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new
            {
                Source = guid.ToString(),
                Type = "ping",
                Hostname = Host,
#if PLATFORM_WINDOWS
                Platform = "Windows",
#elif PLATFORM_MACOS
                    Platform = "MacOS",
#else
#error Platform not supported
#endif
                Role = Role,
                HasReceivedVersionInformation = HasReceivedVersionInformation,
                IPAddresses = ipaddresses,
                CloudOperationsRequested = PubSub.OperationsRequested + 1,
                FullName = fullName,
                EmailAddress = emailAddress
            }))), null);
        }
    }
}
