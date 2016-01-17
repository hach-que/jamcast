using System;
using System.Net;

namespace Controller
{
    public class Computer
    {
        public string Hostname;
        public Platform Platform;
        public Role Role;
        public Guid Guid;
        public bool HasReceivedVersionInformation;
        public bool WaitingForPing;
        public IPAddress[] IPAddresses;
        public DateTime LastContact;
        public int CloudOperationsRequested;

        [NonSerialized]
        public bool HasWaitingTask;
    }
}