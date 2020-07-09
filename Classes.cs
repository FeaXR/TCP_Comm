using System;

namespace TCP_Comm
{
    public class SerializableMessage
    {
        internal string Ip { get; set; }
        internal string Port { get; set; }
        internal SerializableMessageText Text { get; set; }
    }

    //TODO: Implement required fields and adjust constructor and ToString methods accordingly

    /// <summary>
    /// Send several values at once
    /// Insert required fields into message
    /// </summary>
    [Serializable]
    public class SerializableMessageText
    {
        public SerializableMessageText(string message = "")
        {
            MessageText = message;
        }

        public string MessageText { get; set; }

        public override string ToString()
        {
            return MessageText;
        }
    }
}