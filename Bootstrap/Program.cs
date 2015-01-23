using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using SlackRTM;

namespace Bootstrap
{
    public static class Program
    {
        private static string Host;

        private static string Role = "Client";

        private static string GetLocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
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
                                        SendPing(slack, jamcastControllerChannel, guid);
                                    }
                                    break;
                            }
                        }
                    }

                }
            };
            slack.Connect();

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
                }))));
        }
    }
}
