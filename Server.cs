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
        /// Backgroungworker that sends the string messages asyncronously
        /// </summary>
        private readonly BackgroundWorker tcpListenerBackgroundWorker = new BackgroundWorker();

        /// <summary>
        /// Triggers when a message is ready in public Queue messages
        /// </summary>
        public event EventHandler StringMessageArrived;

        /// <summary>
        ///The listener that will get the string communication requests
        /// </summary>
        private TcpListener stringListener;

        /// <summary>
        /// FIFO storage for the messages. Dequeue to get the message when MessageArrived is triggered
        /// </summary>
        public Queue<string> stringMessages = new Queue<string>();

        /// <summary>
        /// Start listening for string TCP messages on any IP  address, on given port (default = 9001)
        /// </summary>
        public void StartStringServer(ushort port = 9001, bool keepOn = true)
        {
            stringListener = new TcpListener(IPAddress.Any, port);
            tcpListenerBackgroundWorker.DoWork += StringListenerBackgroundWorker_DoWork;
            tcpListenerBackgroundWorker.RunWorkerAsync(argument: keepOn);
        }

        /// <summary>
        /// TCP string server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        private void StringListenerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            stringListener.Start();
            bool keepOn = (bool)e.Argument;

            do
            {
                TcpClient tcpClient = stringListener.AcceptTcpClient();

                using (NetworkStream stream = tcpClient.GetStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    string message = reader.ReadToEnd();

                    stringMessages.Enqueue(message);

                    StringMessageArrived?.Invoke(this, EventArgs.Empty);
                    reader.Dispose();
                }
                tcpClient.Close();
            }
            while (keepOn);
        }

        #endregion String Communication

        #region Serializable Communication

        /// <summary>
        /// Backgroungworker that sends the string messages asyncronously
        /// </summary>
        private readonly BackgroundWorker stationListenerBackGroundWorker = new BackgroundWorker();

        /// <summary>
        /// Triggers when a StationStatus is ready in public Queue messages
        /// </summary>
        public event EventHandler SerializableMessageArrived;

        /// <summary>
        ///The listener that will get the serializable communication requests
        /// </summary>
        private TcpListener serializableListener;

        /// <summary>
        /// FIFO storage for the serializable messages. Dequeue to get the message when SerializableMessageArrived is triggered
        /// </summary>
        public Queue<SerializableMessageText> SerializableMessages { get; set; } = new Queue<SerializableMessageText>();

        /// <summary>
        /// Start listening for StationStatus TCP messages on any IP  address, on given port (default = 9001)
        /// </summary>
        /// <param name="port"></param>
        public void StartSerializableMessageServer(ushort port = 9001, bool keepOn = true)
        {
            serializableListener = new TcpListener(IPAddress.Any, port);
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
            serializableListener.Start();

            bool keepOn = (bool)e.Argument;

            do
            {
                TcpClient tcpClient = serializableListener.AcceptTcpClient();

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