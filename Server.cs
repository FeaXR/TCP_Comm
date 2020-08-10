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
        public Server()
        {
            serializableListenerBackGroundWorker.DoWork += SerializableMessageListenerBackGroundWorker_DoWork;
            tcpListenerBackgroundWorker.DoWork += StringListenerBackgroundWorker_DoWork;
        }

        #region String Communication

        /// <summary>
        /// FIFO storage for the messages. Dequeue to get the message when MessageArrived is triggered
        /// </summary>
        public Queue<string> stringMessages = new Queue<string>();

        /// <summary>
        /// Backgroungworker that sends the string messages asyncronously
        /// </summary>
        private readonly BackgroundWorker tcpListenerBackgroundWorker = new BackgroundWorker();

        /// <summary>
        /// Triggers when a message is ready in public Queue messages
        /// </summary>
        public event EventHandler StringMessageArrived;

        /// <summary>
        /// Start listening for string TCP messages on any IP  address, on given port (default = 9001)
        /// </summary>
        /// <param name="port"></param>
        /// <param name="keepOn">If true, the server will loop forever and receive all messages sent to it's port</param>
        public void StartStringServer(ushort port = 9001, bool keepOn = true)
        {
            tcpListenerBackgroundWorker.RunWorkerAsync(argument: new ServerBackgroundworkerArguments(keepOn, IPAddress.Any, port));
        }

        /// <summary>
        /// TCP string server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        private void StringListenerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TcpListener stringListener = new TcpListener(((ServerBackgroundworkerArguments)e.Argument).IP, ((ServerBackgroundworkerArguments)e.Argument).port);

            stringListener.Start();

            //if true, receciver will loop forever
            bool keepOn = ((ServerBackgroundworkerArguments)e.Argument).keepOn;

            do
            {
                TcpClient tcpClient = stringListener.AcceptTcpClient();

                using (NetworkStream stream = tcpClient.GetStream())
                {
                    StreamReader reader = new StreamReader(stream);

                    stringMessages.Enqueue(reader.ReadToEnd());

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
        private readonly BackgroundWorker serializableListenerBackGroundWorker = new BackgroundWorker();

        /// <summary>
        /// Triggers when a StationStatus is ready in public Queue messages
        /// </summary>
        public event EventHandler SerializableMessageArrived;

        /// <summary>
        /// FIFO storage for the serializable messages. Dequeue to get the message when SerializableMessageArrived is triggered
        /// </summary>
        public Queue<SerializableMessageData> SerializableMessages { get; set; } = new Queue<SerializableMessageData>();

        /// <summary>
        /// Start listening for StationStatus TCP messages on any IP  address, on given port (default = 9001)
        /// </summary>
        /// <param name="port"></param>
        /// <param name="keepOn">If true, the server will loop forever and receive all messages sent to it's port</param>
        public void StartSerializableMessageServer(ushort port = 9001, bool keepOn = true)
        {
            serializableListenerBackGroundWorker.RunWorkerAsync(argument: new ServerBackgroundworkerArguments(keepOn, IPAddress.Any, port));
        }

        /// <summary>
        /// TCP string server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        private void SerializableMessageListenerBackGroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TcpListener serializableListener = new TcpListener(((ServerBackgroundworkerArguments)e.Argument).IP, ((ServerBackgroundworkerArguments)e.Argument).port);

            serializableListener.Start();

            //if true, receciver will loop forever
            bool keepOn = ((ServerBackgroundworkerArguments)e.Argument).keepOn;

            do
            {
                TcpClient tcpClient = serializableListener.AcceptTcpClient();

                using (NetworkStream stream = tcpClient.GetStream())
                {
                    IFormatter formatter = new BinaryFormatter();
                    SerializableMessages.Enqueue((SerializableMessageData)formatter.Deserialize(stream));
                    stream.Dispose();
                    SerializableMessageArrived?.Invoke(this, EventArgs.Empty);
                }
                tcpClient.Close();
            }
            while (keepOn);
        }
    }

    #endregion Serializable Communication
}