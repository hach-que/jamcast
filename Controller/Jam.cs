﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using Controller.TreeNode;
using Newtonsoft.Json;
using SlackRTM;

namespace Controller
{
    public class Jam
    {
        [NonSerialized]
        private Slack _slack;

        [NonSerialized] private JamTreeNode _node;

        public Jam(Guid guid, string name)
        {
            this.Guid = guid;
            this.Name = name;
        }

        public string ControllerSlackToken;

        public string ControllerSlackStorageToken;

        public string ProjectorSlackChannels;

        public string ClientSlackToken;

        public Guid Guid;

        public string Name;

        [NonSerialized]
        public List<Computer> Computers = new List<Computer>();

        public string AvailableClientVersion;

        public string AvailableProjectorVersion;

        public string AvailableClientFile;

        public string AvailableProjectorFile;

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