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
        public string[] MACAddresses;
        public DateTime LastContact;
        public int CloudOperationsRequested;
        public string EmailAddress;
        public string FullName;

        [NonSerialized]
        public bool HasWaitingTask;

        public bool SentVersionInformation { get; internal set; }
    }
}