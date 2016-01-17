using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

            Status = "Initializing...";

            PlatformTraySetup();

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

            // Create folder for storing client, projector and bootstrap software.
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JamCast");
            Directory.CreateDirectory(path);
            BasePath = path;

            Client = new Package(BasePath, "Client");
            Projector = new Package(BasePath, "Projector");
            Bootstrap = new Package(BasePath, "Bootstrap");

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

            if (processToKillOnSuccess != null)
            {
                Process.GetProcessById(processToKillOnSuccess.Value).Kill();
            }

            var pubsub = new PubSub(project, GetOAuthToken);
            pubsub.CreateTopic("game-jam-bootstrap");
            pubsub.CreateTopic("game-jam-controller");
            pubsub.CreateTopic("bootstrap-" + guid);
            pubsub.Subscribe("bootstrap-" + guid, "bootstrap-" + guid);
            pubsub.Subscribe("game-jam-bootstrap", "bootstrap-" + guid);

            var pingTopic = "game-jam-controller";

            var thread = new Thread(() =>
            {
                while (true)
                {
                    Status = "Waiting for Messages";

                    try
                    {
                        var messages = pubsub.Poll(10, false);
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

                                        Status = "Got Pong";
                                        SendPing(pubsub, pingTopic, guid);

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

                                        SendPing(pubsub, pingTopic, guid);
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

                                        SendPing(pubsub, pingTopic, guid);
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

                                        SendPing(pubsub, pingTopic, guid);
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
                thread.IsBackground = true;
                thread.Start();

                Client.CalculatePackageVersion();
                Projector.CalculatePackageVersion();
                Bootstrap.CalculatePackageVersion();

                Status = "Starting " + Role;
                if (!Active.StartProcess())
                {
                    Active.KillUnmonitoredProcesses();
                    Active.ExtractPackageIfExists();
                    Active.StartProcess();
                }

                SendPing(pubsub, pingTopic, guid);

                var timer = 0;

                while (true)
                {
                    Thread.Sleep(100);
                    timer += 100;

                    if (timer > 10000)
                    {
                        SendPing(pubsub, pingTopic, guid);
                        timer = 0;
                    }
                }
            }
            finally
            {
                thread.Abort();

                pubsub.DeleteTopic("bootstrap-" + guid);
                pubsub.Unsubscribe("bootstrap-" + guid, "bootstrap-" + guid);
                pubsub.Unsubscribe("game-jam-bootstrap", "bootstrap-" + guid);
                // Don't delete the game-jam topic because it's the global one
            }
        }

        public static DateTime? LastContact { get; set; }

        public static string Status { get; set; }

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
        }
        
        private static void SendPing(PubSub pubsub, string pingTopic, Guid guid)
        {
            var ipaddresses = GetAllKnownIPAddresses().Select(x => x.ToString()).ToArray();

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
            }))), null);
        }
    }
}
