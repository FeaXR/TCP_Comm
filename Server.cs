using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TCP
{
    public class Server
    {
        #region StringCommunication

        private TcpListener listener;
        private readonly BackgroundWorker tcpListenerBackgroundWorker = new BackgroundWorker();

        /// <summary>
        /// Triggers when a message is ready in public Queue messages
        /// </summary>
        public event EventHandler MessageArrived;

        /// <summary>
        /// FIFO storage for the messages. Dequeue to get the message when MessageArrived is triggered
        /// </summary>
        public Queue<string> messages = new Queue<string>();

        /// <summary>
        /// Start listening for string TCP messages on any IP  address, on given port (default = 9001)
        /// </summary>
        public void StartStringServer(ushort port = 9001)
        {
            listener = new TcpListener(IPAddress.Any, port);
            tcpListenerBackgroundWorker.DoWork += TcpListenerBackgroundWorker_DoWork;
            tcpListenerBackgroundWorker.RunWorkerAsync();
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

            while (true)
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
        }

        #endregion StringCommunication
}
