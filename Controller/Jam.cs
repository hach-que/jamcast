using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        public void ReceivedClientMessage(string source, dynamic data)
        {
            switch ((string) data.Type)
            {
                case "ping":
                    var guid = Guid.Parse(source);

                    var ipaddresses = new List<IPAddress>();
                    if (data.IPAddresses != null && data.IPAddresses is JArray)
                    {
                        foreach (var ip in data.IPAddresses)
                        {
                            ipaddresses.Add(IPAddress.Parse((string)ip));
                        }
                    }

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
                            CloudOperationsRequested = 0,
                        };
                        Computers.Add(computer);
                        _node.NewComputerRegistered(computer);
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
                        computer.CloudOperationsRequested = ((int?)data.CloudOperationsRequested ?? 0);
                        foreach (var node in _node.Nodes.OfType<ComputerTreeNode>())
                        {
                            node.Update();
                        }
                    }

                    break;
            }
        }
    }
}