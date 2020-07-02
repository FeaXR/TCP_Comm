using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace TCP
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

                senderWorker.RunWorkerAsync(argument: new MessageClass { MessageText = message + "|" + DateTime.Now.ToString(), MessageIP = iP, MessagePort = portInt });
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
                var client = new TcpClient(mc.MessageIP, mc.MessagePort);
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
}
