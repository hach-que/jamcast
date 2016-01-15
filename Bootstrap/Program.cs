using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using GooglePubSub;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace Bootstrap
{
    /// <summary>
    /// Main class for the Bootstrapper
    /// </summary>
    public static class Program
    {
        private static string Host;

        private static string Role
        {
            get
            {
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

        private static string ClientVersion;

        private static string ProjectorVersion;

        private static string AvailableClientVersion;

        private static string AvailableProjectorVersion;

        private static string AvailableClientFile;

        private static string AvailableProjectorFile;

        private static string ClientPath;

        private static string ProjectorPath;

        private static string ClientPackagePath;

        private static string ProjectorPackagePath;

        private static string BasePath;

        private static Process MonitoredProcess;

        private static bool ProcessShouldBeRunning;

        private static bool HasReceivedVersionInformation;

        private static string ClientSettingsPath;

        private static string ProjectorSettingsPath;
        private static string _oAuthEndpoint;

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

            // Create folder for storing client / projector software.
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JamCast");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, "Client"));
            Directory.CreateDirectory(Path.Combine(path, "Projector"));
            BasePath = path;
            ClientPath = Path.Combine(path, "Client");
            ProjectorPath = Path.Combine(path, "Projector");
            ClientPackagePath = Path.Combine(path, "Client.zip");
            ProjectorPackagePath = Path.Combine(path, "Projector.zip");
            ClientSettingsPath = Path.Combine(BasePath, "client-settings.json");
            ProjectorSettingsPath = Path.Combine(BasePath, "projector-settings.json");

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
                    try
                    {
                        var message = pubsub.Poll(1, false).FirstOrDefault();
                        if (message != null)
                        {
                            message.Acknowledge();

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

                            switch ((string) m.Type)
                            {
                                case "pong":
                                {
                                    HasReceivedVersionInformation = true;

                                    SendPing(pubsub, pingTopic, guid);

                                    AvailableClientVersion = m.AvailableClientVersion;
                                    AvailableProjectorVersion = m.AvailableProjectorVersion;
                                    AvailableClientFile = m.AvailableClientFile;
                                    AvailableProjectorFile = m.AvailableProjectorFile;

                                    UpdateVersions();
                                }
                                    break;
                                case "designate":
                                {
                                    Role = m.Role;

                                    UpdateVersions();

                                    SendPing(pubsub, pingTopic, guid);
                                }
                                    break;
                                case "client-settings":
                                {
                                    using (var writer = new StreamWriter(ClientSettingsPath))
                                    {
                                        writer.Write((string) m.Settings);
                                    }

                                    KillProcess();
                                    StartProcess();

                                    SendPing(pubsub, pingTopic, guid);
                                }
                                    break;
                                case "projector-settings":
                                {
                                    using (var writer = new StreamWriter(ProjectorSettingsPath))
                                    {
                                        writer.Write((string) m.Settings);
                                    }

                                    KillProcess();
                                    StartProcess();

                                    SendPing(pubsub, pingTopic, guid);
                                }
                                    break;
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

                CalculateVersions();

                if (!StartProcess())
                {
                    switch (Role)
                    {
                        case "Client":
                            if (File.Exists(ClientPackagePath))
                            {
                                ExtractPackage(ClientPackagePath, ClientPath);
                            }
                            break;
                        case "Projector":
                            if (File.Exists(ProjectorPackagePath))
                            {
                                ExtractPackage(ProjectorPackagePath, ProjectorPath);
                            }
                            break;
                    }

                    StartProcess();
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

        private static void UpdateVersions()
        {
            switch (Role)
            {
                case "Client":
                    if (!string.IsNullOrWhiteSpace(AvailableClientVersion))
                    {
                        if (ClientVersion != AvailableClientVersion)
                        {
                            var package = new WebClient();
                            package.DownloadFile(new Uri(AvailableClientFile), ClientPackagePath);
                            KillProcess();
                            ExtractPackage(ClientPackagePath, ClientPath);
                            CalculateVersions();
                            StartProcess();
                        }
                    }
                    break;
                case "Projector":
                    if (!string.IsNullOrWhiteSpace(AvailableProjectorVersion))
                    {
                        if (ProjectorVersion != AvailableProjectorVersion)
                        {
                            var package = new WebClient();
                            package.DownloadFile(new Uri(AvailableProjectorFile), ProjectorPackagePath);
                            KillProcess();
                            ExtractPackage(ProjectorPackagePath, ProjectorPath);
                            CalculateVersions();
                            StartProcess();
                        }
                    }
                    break;
            }
        }

        private static void ExtractPackage(string clientPackagePath, string clientPath)
        {
            ZipFile zf = null;
            try
            {
                var fs = File.OpenRead(clientPackagePath);
                zf = new ZipFile(fs);
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;
                    }

                    var entryFileName = zipEntry.Name;

                    var buffer = new byte[4096];
                    var zipStream = zf.GetInputStream(zipEntry);

                    var fullZipToPath = Path.Combine(clientPath, entryFileName);
                    var directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    using (var streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        private static bool StartProcess()
        {
            ProcessShouldBeRunning = true;

            var startInfo = new ProcessStartInfo();

            switch (Role)
            {
                case "Client":
                    var clientPath = Path.Combine(ClientPath, "Client.exe");
                    if (ClientVersion != null && File.Exists(clientPath))
                    {
                        startInfo.FileName = clientPath;
                        startInfo.WorkingDirectory = ClientPath;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case "Projector":
                    var projectorPath = Path.Combine(ProjectorPath, "Projector.exe");
                    if (ProjectorVersion != null && File.Exists(projectorPath))
                    {
                        startInfo.FileName = projectorPath;
                        startInfo.WorkingDirectory = ProjectorPath;
                    }
                    else
                    {
                        return false;
                    }
                    break;
            }

            startInfo.UseShellExecute = false;

            if (startInfo.FileName == "")
            {
                return false;
            }

            MonitoredProcess = new Process();
            MonitoredProcess.StartInfo = startInfo;
            MonitoredProcess.EnableRaisingEvents = true;
            MonitoredProcess.Exited += (sender, args) =>
            {
                if (ProcessShouldBeRunning)
                {
                    StartProcess();
                }
            };
            MonitoredProcess.Start();

            return true;
        }

        private static void KillProcess()
        {
            ProcessShouldBeRunning = false;

            if (MonitoredProcess == null)
            {
                return;
            }

            if (!MonitoredProcess.HasExited)
            {
                MonitoredProcess.Kill();
                Thread.Sleep(1000);
            }
        }

        private static void CalculateVersions()
        {
            ClientVersion = CalculateVersion(ClientPackagePath);
            ProjectorVersion = CalculateVersion(ProjectorPackagePath);
        }

        private static string CalculateVersion(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var md5 = MD5.Create();
            var contentBytes = File.ReadAllBytes(path);
            md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);

            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
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
