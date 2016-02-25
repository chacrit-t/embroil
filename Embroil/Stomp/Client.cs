using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vrc.Embroil.Connection;
using Vrc.Embroil.Stomp;

namespace Vrc.Transport.Stomp
{
    public sealed class Client
    {
        private IConnection connection;
        public Client(IConnection connection)
        {
            this.connection = connection;
        }

        /// <summary>
        /// Request Message
        /// CONNECT
        /// [REQ] accept-version:{version=1.2}, host:{host-name}
        /// [OPT] login, passcode, heart-beat
        /// 
        /// Response Message
        /// CONNECTED
        /// [REQ] version:{version=1.2}
        /// [OPT] session, server, heart-beat
        /// 
        /// </summary>
        public void Connect()
        {
            connection.OnOpen += OnOpen;
            connection.OnMessage += OnMessage;
            connection.OnClose += OnClose;

            connection.Connect();
        }

        private void OnClose()
        {
            throw new NotImplementedException();
        }

    
        private void OnOpen()
        {
            throw new NotImplementedException();
        }

        private void OnMessage(string message)
        {
            var msgObj = MessageSerializer.Deserailize(message);
        }

        /// <summary>
        /// Request Message
        /// SEND
        /// [REQ] destination
        /// [OPT] transaction
        /// Body
        ///  
        /// </summary>
        void Send()
        {
            
        }

        void Subscribe()
        {
            
        }

        void Unsubscribe()
        {
            
        }

        void Ack()
        {
            
        }

        void Nack()
        {
            
        }

        void Begin()
        {
            
        }

        void Commit()
        { }

        void Abort()
        {
            
        }

        void Disconnect()
        {
            
        }
    }
}
