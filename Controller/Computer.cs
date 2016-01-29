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

        public DateTime LastTimeControllerSentMessageToBootstrap;
        public DateTime LastTimeControllerRecievedMessageFromBootstrap;
        public DateTime? LastTimeBootstrapSentAMessage;
        public DateTime? LastTimeBootstrapRecievedAMessageFromControllerAndAckedIt;

        [NonSerialized]
        public bool HasWaitingTask;

        public bool SentVersionInformation { get; internal set; }
    }
}