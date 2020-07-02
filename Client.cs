using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace TCP_Comm
{
    public static class Client
    {
        private static readonly BackgroundWorker senderWorker = new BackgroundWorker();

        /// <summary>
        /// Send TCP message to given IP, Port
        /// </summary>
        /// <param name="iP"></param>
        /// <param name="port"></param>
        /// <param name="message"></param>
        /// <exception cref="Exception"> Thrown when can not connect to next peer </exception>
        /// <exception cref="FormatException"> Thrown when IP is in wrong format, or message is empty</exception>
        /// <exception cref="InvalidDataException">Thrown when port number is invalid</exception>
        public static void SendMessage(string iP, string port, string message)
        {
            senderWorker.DoWork += SenderWorker_DoWork;

            if (message?.Length == 0)
            {
                throw new FormatException("Message can't be empty!");
            }

            try
            {
                Regex pattern = new Regex(@"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");

                if (!pattern.IsMatch(iP))
                {
                    throw new FormatException("Invalid format for IP");
                }

                if (!int.TryParse(port, out int portInt))
                {
                    throw new InvalidDataException("Invalid Port Number!");
                }

                senderWorker.RunWorkerAsync(argument: new MessageClass { MessageText = message + "|" + DateTime.Now.ToString(), Ip = iP, Port = portInt });
            }
            catch (SocketException)
            {
                throw new Exception("Can't connect to next peer");
            }
        }

        /// <summary>
        ///Sending a string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception">Thrown when can not connect to next peer</exception>
        private static void SenderWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            senderWorker.DoWork -= SenderWorker_DoWork;
            var mc = (MessageClass)e.Argument;

            try
            {
                var client = new TcpClient(mc.Ip, mc.Port);
                var clientStream = client.GetStream();

                using (var streamWriter = new StreamWriter(clientStream))
                {
                    streamWriter.Write(mc.MessageText);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                client.Dispose();
                clientStream.Dispose();
                return;
            }
            catch (SocketException)
            {
                throw new Exception("Can't connect to next peer");
            }
        }

        /// <summary>
        /// Send TCP Station Status message to given IP, Port
        /// </summary>
        /// <param name="iP"></param>
        /// <param name="port"></param>
        /// <param name="message"></param>
        public static void SendStatus(string iP, string port, SerializableMessage message)
        {
            BackgroundWorker serializableMessageSender = new BackgroundWorker();

            serializableMessageSender.DoWork += SerializableMessageSender_DoWork;

            if (message == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                Regex pattern = new Regex(@"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");

                if (!pattern.IsMatch(iP))
                {
                    throw new FormatException("Invalid format for IP");
                }

                if (!int.TryParse(port, out int portInt))
                {
                    throw new InvalidDataException("Invalid Port Number!");
                }

                serializableMessageSender.RunWorkerAsync(argument: new StatusMessage
                {
                    status = message,
                    Port = port,
                    Ip = iP
                });
            }
            catch (SocketException)
            {
                throw new Exception("Can't connect to next peer");
            }
        }

        private static void SerializableMessageSender_DoWork(object sender, DoWorkEventArgs e)
        {
            var msg = (StatusMessage)e.Argument;
            string ip = msg.Ip;
            string port = msg.Port;
            SerializableMessage status = msg.status;

            IFormatter formatter = new BinaryFormatter();
            var client = new TcpClient(ip, int.Parse(port));
            var clientStream = client.GetStream();
            formatter.Serialize(clientStream, status);
            clientStream.Dispose();
            client.Dispose();
            System.Threading.Thread.Sleep(500);
        }

        /// <summary>
        /// Used for TCP_Client Message sending
        /// </summary>
        private class MessageClass
        {
            internal string Ip { get; set; }
            internal int Port { get; set; }
            internal string MessageText { get; set; }
        }

        private class StatusMessage
        {
            internal string Ip;
            internal string Port;
            internal SerializableMessage status;
        }

        //TODO: Implement required fields and adjust constructor accordingly

        /// <summary>
        /// Send several values at once
        /// Insert required fields into message
        /// </summary>
        [Serializable]
        public class SerializableMessage
        {
            private readonly string MessageText;

            public SerializableMessage(string message = "")
            {
                MessageText = message;
            }

            public override string ToString()
            {
                return MessageText;
            }
        }
    }
}
