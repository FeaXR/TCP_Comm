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
        #region String Communicaction

        private static readonly BackgroundWorker stringSenderWorker = new BackgroundWorker();

        /// <summary>
        /// Send TCP message to given IP, Port
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="message"></param>
        /// <exception cref="Exception"> Thrown when can not connect to next peer </exception>
        /// <exception cref="FormatException"> Thrown when IP is in wrong format, or message is empty</exception>
        /// <exception cref="InvalidDataException">Thrown when port number is invalid</exception>
        public static void SendStringMessage(string message, string ip = "127.0.0.1", string port = "9001")
        {
            if (message?.Length == 0)
            {
                throw new FormatException("Message can't be empty!");
            }

            try
            {
                Regex pattern = new Regex(@"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");

                if (!pattern.IsMatch(ip))
                {
                    throw new FormatException("Invalid format for IP");
                }

                if (!int.TryParse(port, out int portInt))
                {
                    throw new InvalidDataException("Invalid Port Number!");
                }
                stringSenderWorker.DoWork += StringSenderWorker_DoWork;
                stringSenderWorker.RunWorkerAsync(argument: new MessageClass { MessageText = message + "|" + DateTime.Now.ToString(), Ip = ip, Port = portInt });
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
        private static void StringSenderWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            stringSenderWorker.DoWork -= StringSenderWorker_DoWork;
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

        #endregion String Communicaction

        #region Serializable Communication

        private static readonly BackgroundWorker serializableMessageSender = new BackgroundWorker();

        /// <summary>
        /// Send TCP Station Status message to given IP, Port
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="message"></param>
        /// <exception cref="Exception"> Thrown when can not connect to next peer </exception>
        /// <exception cref="FormatException"> Thrown when IP is in wrong format, or message is empty</exception>
        /// <exception cref="InvalidDataException">Thrown when port number is invalid</exception>
        public static void SendSerializable(SerializableMessageText message, string ip = "127.0.0.1", string port = "9001")
        {
            if (message == null)
            {
                throw new FormatException("Message can't be empty!");
            }

            try
            {
                Regex pattern = new Regex(@"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");

                if (!pattern.IsMatch(ip))
                {
                    throw new FormatException("Invalid format for IP");
                }

                if (!int.TryParse(port, out int portInt))
                {
                    throw new InvalidDataException("Invalid Port Number!");
                }
                serializableMessageSender.DoWork += SerializableMessageSender_DoWork;
                serializableMessageSender.RunWorkerAsync(argument: new SerializableMessage
                {
                    Text = message,
                    Port = port,
                    Ip = ip
                });
            }
            catch (SocketException)
            {
                throw new Exception("Can't connect to next peer");
            }
        }

        private static void SerializableMessageSender_DoWork(object sender, DoWorkEventArgs e)
        {
            serializableMessageSender.DoWork -= SerializableMessageSender_DoWork;

            var msg = (SerializableMessage)e.Argument;
            string ip = msg.Ip;
            string port = msg.Port;
            SerializableMessageText status = msg.Text;

            IFormatter formatter = new BinaryFormatter();
            var client = new TcpClient(ip, int.Parse(port));
            var clientStream = client.GetStream();
            formatter.Serialize(clientStream, status);
            clientStream.Dispose();
            client.Dispose();
            System.Threading.Thread.Sleep(500);
        }

        #endregion Serializable Communication

        #region Classes

        /// <summary>
        /// Used for TCP_Client Message sending
        /// </summary>
        private class MessageClass
        {
            internal string Ip { get; set; }
            internal string MessageText { get; set; }
            internal int Port { get; set; }
        }

        #endregion Classes
    }
}