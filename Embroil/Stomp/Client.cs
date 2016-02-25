using System;
using Vrc.Embroil.Connection;

namespace Vrc.Embroil.Stomp
{
    public sealed class Client
    {
        private readonly IConnection connection;

        public Client(IConnection connection)
        {
            this.connection = connection;
        }

        /// <summary>
        ///     Request Message
        ///     CONNECT
        ///     [REQ] accept-version:{version=1.2}, host:{host-name}
        ///     [OPT] login, passcode, heart-beat
        ///     Response Message
        ///     CONNECTED
        ///     [REQ] version:{version=1.2}
        ///     [OPT] session, server, heart-beat
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
        ///     Request Message
        ///     SEND
        ///     [REQ] destination
        ///     [OPT] transaction
        ///     Body
        /// </summary>
        private void Send()
        {
        }

        private void Subscribe()
        {
        }

        private void Unsubscribe()
        {
        }

        private void Ack()
        {
        }

        private void Nack()
        {
        }

        private void Begin()
        {
        }

        private void Commit()
        {
        }

        private void Abort()
        {
        }

        private void Disconnect()
        {
        }
    }
}