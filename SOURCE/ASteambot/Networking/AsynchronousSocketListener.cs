﻿using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace ASteambot.Networking
{
    public class AsynchronousSocketListener
    {
        public int Port { get; private set; }
        public event EventHandler<EventArgGameServer> MessageReceived;

        private string password;

        protected virtual void OnMessageReceived(EventArgGameServer e)
        {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }
        
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private bool running = false;

        private void HandleMessage(Socket handler, string content)
        {
            if (!content.StartsWith(password))
                return;

            content = content.Replace(password, "");

            string[] codeargs = content.Split(new char[] { '&' }, 2);

            string[] idmsgtype = codeargs[0].Split(new char[] { '|' }, 2);

            codeargs[1] = codeargs[1].Replace("\0", string.Empty);

            //Must be always true :
            if (codeargs[1].EndsWith("<EOF>"))
                codeargs[1] = codeargs[1].Substring(0, codeargs[1].Length - 5);

            EventArgGameServer arg = new EventArgGameServer(handler, idmsgtype[0], idmsgtype[1], codeargs[1]);
            OnMessageReceived(arg);
        }
        public void Stop()
        {
            running = false;
            allDone.Set();
        }

        public AsynchronousSocketListener(int port, string password)
        {
            Port = port;
            this.password = password;
        }

        public void StartListening()
        {
            byte[] bytes = new Byte[1024];

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), Port);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    allDone.Reset();
                    
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);
                    
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            try
            { 
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    content = state.sb.ToString().Replace("\0", String.Empty);

                    if (content.EndsWith("<EOF>"))
                    {
                        HandleMessage(handler, content);
                        state.sb.Clear();
                    }
               

                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
            catch(Exception e)
            {

            }
        }
    }
}