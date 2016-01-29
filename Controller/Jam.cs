using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Controller.TreeNode;
using GooglePubSub;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Controller
{
    public class Jam
    {
        [NonSerialized]
        private PubSub _pubSub;

        [NonSerialized] private JamTreeNode _node;

        public Jam(Guid guid, string name)
        {
            this.Guid = guid;
            this.Name = name;
        }

        public string GoogleCloudProjectID;

        public string GoogleCloudOAuthEndpointURL;

        public string GoogleCloudStorageSecret;

        public string ProjectorSlackAPIToken;

        public string ProjectorSlackChannels;

        public Guid Guid;

        public string Name;

        [NonSerialized]
        public List<Computer> Computers = new List<Computer>();

        public Dictionary<string, string> CachedProjectorSettings = new Dictionary<string, string>();

        public Dictionary<string, string> CachedClientSettings = new Dictionary<string, string>();

        public string AvailableClientVersion;

        public string AvailableProjectorVersion;

        public string AvailableBootstrapVersion;

        public string AvailableClientFile;

        public string AvailableProjectorFile;

        public string AvailableBootstrapFile;

        public string MacAddressReportingEndpointURL;

        public void SetTreeNode(JamTreeNode node)
        {
            _node = node;
        }

        public void Save()
        {
            using (var writer = new StreamWriter(this.Guid + ".jam", false))
            {
                writer.Write(JsonConvert.SerializeObject(this));
            }
        }

        public static Jam Load(string path)
        {
            using (var reader = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<Jam>(reader.ReadToEnd());
            }
        }

        public void ReceivedClientMessage(PubSubController controller, string source, dynamic data)
        {
            switch ((string) data.Type)
            {
                case "ping":
                    var guid = Guid.Parse(source);

                    var lastRecievedTime = (DateTime?)data.SendTime;

                    if (lastRecievedTime != null && lastRecievedTime < DateTime.UtcNow.AddMinutes(-10))
                    {
                        System.Diagnostics.Debug.WriteLine("Ignored message from " + (string)data.Hostname + " because it's too old.");
                        return;
                    }

                    var ipaddresses = new List<IPAddress>();
                    if (data.IPAddresses != null && data.IPAddresses is JArray)
                    {
                        foreach (var ip in data.IPAddresses)
                        {
                            ipaddresses.Add(IPAddress.Parse((string)ip));
                        }
                    }

                    var macaddresses = new List<string>();
                    if (data.HWAddresses != null && data.HWAddresses is JArray)
                    {
                        foreach (var hw in data.HWAddresses)
                        {
                            var mac = (string) hw;
                            if (!mac.Contains(":") && mac.Length == 12)
                            {
                                mac =
                                    mac.Substring(0, 2).ToLowerInvariant() + ":" +
                                    mac.Substring(2, 2).ToLowerInvariant() + ":" +
                                    mac.Substring(4, 2).ToLowerInvariant() + ":" +
                                    mac.Substring(6, 2).ToLowerInvariant() + ":" +
                                    mac.Substring(8, 2).ToLowerInvariant() + ":" +
                                    mac.Substring(10, 2).ToLowerInvariant();
                            }
                            macaddresses.Add(mac);
                        }
                    }

                    Computer computerToSubmit = null;

                    if (Computers.All(x => x.Guid != guid))
                    {
                        var computer = new Computer
                        {
                            Guid = guid,
                            Hostname = (string) data.Hostname,
                            Platform = (Platform)Enum.Parse(typeof(Platform), (string)data.Platform),
                            Role = (Role)Enum.Parse(typeof(Role), (string)data.Role),
                            HasReceivedVersionInformation = (bool)(data.HasReceivedVersionInformation ?? false),
                            WaitingForPing = false,
                            LastContact = DateTime.UtcNow,
                            IPAddresses = ipaddresses.ToArray(),
                            MACAddresses = macaddresses.ToArray(),
                            CloudOperationsRequested = 0,
                            FullName = (string)data.FullName,
                            EmailAddress = (string)data.EmailAddress,
                        };
                        computerToSubmit = computer;
                    }
                    else
                    {
                        var computer = Computers.First(x => x.Guid == guid);
                        computer.Hostname = (string) data.Hostname;
                        computer.Role = (Role)Enum.Parse(typeof(Role), (string)data.Role);
                        computer.HasReceivedVersionInformation = (bool) (data.HasReceivedVersionInformation ?? false);
                        computer.WaitingForPing = false;
                        computer.LastContact = DateTime.UtcNow;
                        computer.IPAddresses = ipaddresses.ToArray();
                        computer.MACAddresses = macaddresses.ToArray();
                        computer.CloudOperationsRequested = ((int?)data.CloudOperationsRequested ?? 0);
                        computer.FullName = (string)data.FullName;
                        computer.EmailAddress = (string)data.EmailAddress;
                        computerToSubmit = computer;
                    }

                    if (!computerToSubmit.HasReceivedVersionInformation)
                    {
                        computerToSubmit.SentVersionInformation = true;
                        controller.SendPong(guid, computerToSubmit, this);
                    }

                    if (Computers.All(x => x.Guid != guid))
                    {
                        Computers.Add(computerToSubmit);
                        _node.NewComputerRegistered(computerToSubmit);
                    }
                    else
                    {
                        foreach (var node in _node.Nodes.OfType<ComputerTreeNode>().Where(x => x.Computer == computerToSubmit))
                        {
                            node.Update();
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(MacAddressReportingEndpointURL))
                    {
                        if (computerToSubmit.EmailAddress != null && computerToSubmit.MACAddresses.Length > 0)
                        {
                            foreach (var mac in computerToSubmit.MACAddresses)
                            {
                                var client = new WebClient();
                                Task.Run(async () => await client.UploadValuesTaskAsync(MacAddressReportingEndpointURL, "POST",
                                    new NameValueCollection
                                    {
                                        {"email", computerToSubmit.EmailAddress},
                                        {"ip_addresses", string.Join(" ", computerToSubmit.IPAddresses.Select(x => x.ToString()))},
                                        {"hostname", computerToSubmit.Hostname},
                                        {"mac_address", mac},
                                    }));
                            }
                        }
                    }

                    break;
            }
        }
    }
}