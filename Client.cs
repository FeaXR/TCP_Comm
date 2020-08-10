using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace TCP_Comm
{
    public class Client
    {
        public Client()
        {
            stringSenderWorker.DoWork += StringSenderWorker_DoWork;
            serializableMessageSender.DoWork += SerializableMessageSender_DoWork;
        }

        #region String Communicaction

        private readonly BackgroundWorker stringSenderWorker = new BackgroundWorker();

        /// <summary>
        /// Send TCP message to given IP, Port
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="portText"></param>
        /// <param name="message"></param>
        /// <exception cref="Exception"> Thrown when can not connect to next peer </exception>
        /// <exception cref="FormatException"> Thrown when IP is in wrong format, or message is empty</exception>
        /// <exception cref="InvalidDataException">Thrown when port number is invalid</exception>
        public void SendStringMessage(string message, string ip = "127.0.0.1", string portText = "9001")
        {
            //If message is empty or null, throw exception
            if (message?.Length == 0)
            {
                throw new FormatException("Message can't be empty!");
            }

            try
            {
                //REGEX for IP adress matching
                Regex pattern = new Regex(@"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");

                //Throw exception if prrovided IP is not a valid IP
                if (!pattern.IsMatch(ip))
                {
                    throw new FormatException("Invalid format for IP");
                }

                //Throw exception if prrovided Port is not a valid number
                if (!int.TryParse(portText, out int portInt))
                {
                    throw new InvalidDataException("Invalid Port Number!");
                }

                //Throw exception if prrovided Port is not a valid Port
                if (portInt > 65535)
                {
                    throw new InvalidDataException("Invalid Port Number!");
                }

                stringSenderWorker.RunWorkerAsync(argument: new MessageClass
                {
                    MessageText = message + "|" + DateTime.Now.ToString(),
                    Ip = ip,
                    Port = portInt
                });
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
        private void StringSenderWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            MessageClass message = (MessageClass)e.Argument;

            try
            {
                var client = new TcpClient(message.Ip, message.Port);

                using (var streamWriter = new StreamWriter(client.GetStream()))
                {
                    streamWriter.Write(message.MessageText);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                client.Dispose();
                return;
            }
            catch (SocketException)
            {
                throw new Exception("Can't connect to next peer");
            }
        }

        #endregion String Communicaction

        #region Serializable Communication

        private readonly BackgroundWorker serializableMessageSender = new BackgroundWorker();

        /// <summary>
        /// Send TCP Station Status message to given IP, Port
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="message"></param>
        /// <exception cref="Exception"> Thrown when can not connect to next peer </exception>
        /// <exception cref="FormatException"> Thrown when IP is in wrong format, or message is empty</exception>
        /// <exception cref="InvalidDataException">Thrown when port number is invalid</exception>
        public void SendSerializable(SerializableMessageData message, string ip = "127.0.0.1", string portText = "9001")
        {
            if (message == null)
            {
                throw new FormatException("Message can't be empty!");
            }

            try
            {
                //REGEX for IP adress matching
                Regex pattern = new Regex(@"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");

                //Throw exception if prrovided IP is not a valid IP
                if (!pattern.IsMatch(ip))
                {
                    throw new FormatException("Invalid format for IP");
                }

                //Throw exception if provided Port is not a valid number
                if (!int.TryParse(portText, out int portInt))
                {
                    throw new InvalidDataException("Invalid Port Number!");
                }

                //Throw exception if prrovided Port is not a valid Port
                if (portInt > 65535)
                {
                    throw new InvalidDataException("Invalid Port Number!");
                }

                serializableMessageSender.RunWorkerAsync(argument: new SerializableMessage
                {
                    Data = message,
                    Port = portInt,
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
            SerializableMessage message = (SerializableMessage)e.Argument;

            try
            {
                using (TcpClient client = new TcpClient(message.Ip, message.Port))
                {
                    NetworkStream clientStream = client.GetStream();
                    new BinaryFormatter().Serialize(clientStream, message.Data);
                    clientStream.Dispose();
                    client.Dispose();
                }
            }
            catch (SocketException)
            {
                throw new Exception("Can't connect to next peer");
            }
        }

        #endregion Serializable Communication
    }
}