using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using SlackRTM;

namespace Bootstrap
{
    public static class Program
    {
        private static string Host;

        private static string Role = "Client";

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

        private static Process MonitoredProcess;

        private static bool ProcessShouldBeRunning;

        private static bool HasReceivedVersionInformation;

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

        public static void Main(string[] args)
        {
            string token;
            var tokenStream = typeof(Program).Assembly.GetManifestResourceStream("Bootstrap.token.txt");
            using (var reader = new StreamReader(tokenStream))
            {
                token = reader.ReadToEnd().Trim();
            }

            var guid = Guid.NewGuid();

            try
            {
                Host = System.Net.Dns.GetHostEntry("").HostName;
            }
            catch
            {
                try
                {
                    Host = System.Environment.MachineName;
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

            string jamcastControllerChannel = null;

            var slack = new Slack();
            slack.Init(token);
            slack.OnEvent += (s, e) =>
            {
                if (e.Data.Type == "message")
                {
                    var message = e.Data as SlackRTM.Events.Message;
                    if (message == null)
                    {
                        return;
                    }

                    if (message.Hidden)
                    {
                        return;
                    }

                    if (message.Channel[0] == 'D') // DMs.
                    {
                        var userId = message.User;
                        var user = slack.GetUser(userId).Name;

                        if (user == "jamcast-controller")
                        {
                            var m =
                                JsonConvert.DeserializeObject<dynamic>(
                                    Encoding.ASCII.GetString(Convert.FromBase64String(message.Text)));
                            var target = (string)m.Target;

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
                                    if (jamcastControllerChannel != null)
                                    {
                                        HasReceivedVersionInformation = true;

                                        SendPing(slack, jamcastControllerChannel, guid);

                                        AvailableClientVersion = m.AvailableClientVersion;
                                        AvailableProjectorVersion = m.AvailableProjectorVersion;
                                        AvailableClientFile = m.AvailableClientFile;
                                        AvailableProjectorFile = m.AvailableProjectorFile;

                                        UpdateVersions();
                                    }
                                    break;
                            }
                        }
                    }

                }
            };
            slack.Connect();

            // Create folder for storing client / projector software.
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JamCast");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, "Client"));
            Directory.CreateDirectory(Path.Combine(path, "Projector"));
            ClientPath = Path.Combine(path, "Client");
            ProjectorPath = Path.Combine(path, "Projector");
            ClientPackagePath = Path.Combine(path, "Client.zip");
            ProjectorPackagePath = Path.Combine(path, "Projector.zip");
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

            jamcastControllerChannel =
                slack.Ims.Where(x => x.User == slack.GetUser("jamcast-controller").Id).Select(x => x.Id).FirstOrDefault();
            if (jamcastControllerChannel == null)
            {
                var client = new WebClient();
                var result = client.DownloadString("https://slack.com/api/im.open?token=" + token + "&user=" +
                                                   slack.GetUser("jamcast-controller").Id);
                var response = JsonConvert.DeserializeObject<dynamic>(result);

                jamcastControllerChannel = (string)response.channel.id;
            }

            SendPing(slack, jamcastControllerChannel, guid);

            var timer = 0;

            while (true)
            {
                while (slack.Connected)
                {
                    Thread.Sleep(100);
                    timer += 100;

                    if (timer > 600000)
                    {
                        SendPing(slack, jamcastControllerChannel, guid);
                        timer = 0;
                    }
                }

                slack.Init(token);
                slack.Connect();
            }
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

        private static void SendPing(Slack slack, string channelId, Guid guid)
        {
            slack.SendMessage(channelId,
                Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new
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
                    HasReceivedVersionInformation = HasReceivedVersionInformation
                }))));
        }
    }
}
