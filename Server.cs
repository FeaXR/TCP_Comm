using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TCP_Comm
{
    public class Server
    {
        #region String Communication

        /// <summary>
        /// FIFO storage for the messages. Dequeue to get the message when MessageArrived is triggered
        /// </summary>
        public Queue<string> messages = new Queue<string>();

        private readonly BackgroundWorker tcpListenerBackgroundWorker = new BackgroundWorker();
        private TcpListener listener;

        /// <summary>
        /// Triggers when a message is ready in public Queue messages
        /// </summary>
        public event EventHandler MessageArrived;

        /// <summary>
        /// Start listening for string TCP messages on any IP  address, on given port (default = 9001)
        /// </summary>
        public void StartStringServer(ushort port = 9001, bool keepOn = true)
        {
            listener = new TcpListener(IPAddress.Any, port);
            tcpListenerBackgroundWorker.DoWork += TcpListenerBackgroundWorker_DoWork;
            tcpListenerBackgroundWorker.RunWorkerAsync(argument: keepOn);
        }

        /// <summary>
        /// TCP string server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        private void TcpListenerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            listener.Start();

            bool keepOn = (bool)e.Argument;

            do
            {
                TcpClient tcpClient = listener.AcceptTcpClient();

                using (NetworkStream stream = tcpClient.GetStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    string message = reader.ReadToEnd();

                    messages.Enqueue(message);

                    MessageArrived?.Invoke(this, EventArgs.Empty);
                    reader.Dispose();
                }
                tcpClient.Close();
            }
            while (keepOn);
        }

        #endregion String Communication

        #region Serializable Communication

        private readonly BackgroundWorker stationListenerBackGroundWorker = new BackgroundWorker();

        private Queue<SerializableMessageText> serializablesMessages = new Queue<SerializableMessageText>();

        /// <summary>
        /// Triggers when a StationStatus is ready in public Queue messages
        /// </summary>
        public event EventHandler SerializableMessageArrived;

        public Queue<SerializableMessageText> SerializableMessages { get => serializablesMessages; set => serializablesMessages = value; }

        /// <summary>
        /// Start listening for StationStatus TCP messages on any IP  address, on given port (default = 9001)
        /// </summary>
        /// <param name="port"></param>
        public void StartSerializableMessageServer(ushort port = 9001, bool keepOn = true)
        {
            listener = new TcpListener(IPAddress.Any, port);
            stationListenerBackGroundWorker.DoWork += SerializableMessageListenerBackGroundWorker_DoWork;
            stationListenerBackGroundWorker.RunWorkerAsync(argument: keepOn);
        }

        /// <summary>
        /// TCP string server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        private void SerializableMessageListenerBackGroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            listener.Start();

            bool keepOn = (bool)e.Argument;

            do
            {
                TcpClient tcpClient = listener.AcceptTcpClient();

                using (NetworkStream stream = tcpClient.GetStream())
                {
                    IFormatter formatter = new BinaryFormatter();
                    SerializableMessageText status = (SerializableMessageText)formatter.Deserialize(stream);
                    stream.Dispose();
                    System.Threading.Thread.Sleep(500);
                    SerializableMessages.Enqueue(status);
                    SerializableMessageArrived?.Invoke(this, EventArgs.Empty);
                }
                tcpClient.Close();
            }
            while (keepOn);
        }
    }

    #endregion Serializable Communication
}