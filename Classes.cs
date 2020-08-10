using System;
using System.Net;
using System.Reflection;

namespace TCP_Comm
{
    /// <summary>
    /// Used for TCP_Client Message sending
    /// </summary>
    public class MessageClass
    {
        internal string Ip { get; set; }
        internal string MessageText { get; set; }
        internal int Port { get; set; }
    }

    public class SerializableMessage
    {
        internal SerializableMessageData Data { get; set; }
        internal string Ip { get; set; }
        internal int Port { get; set; }
    }

    /// <summary>
    /// Send several values at once
    /// Customize fields of message if needed
    /// </summary>
    [Serializable]
    public class SerializableMessageData //TODO: Implement required fields and adjust constructor and ToString methods accordingly
    {
        public SerializableMessageData(string message = "")
        {
            MessageText = message;
        }

        public string MessageText { get; set; }

        public override string ToString()
        {
            return MessageText;
        }
    }

    public class ServerBackgroundworkerArguments
    {
        public bool keepOn;
        public IPAddress IP;
        public int port;

        public ServerBackgroundworkerArguments(bool _keepOn, string _IP, int _port)
        {
            keepOn = _keepOn;
            IP = IPAddress.Parse(_IP);
            port = _port;
        }

        public ServerBackgroundworkerArguments(bool _keepOn, IPAddress _IP, int _port)
        {
            keepOn = _keepOn;
            IP = _IP;
            port = _port;
        }
    }
}