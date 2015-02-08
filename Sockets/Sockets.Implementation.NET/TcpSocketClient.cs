﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Sockets.Plugin
{
    /// <summary>
    ///     Sends and receives data over a TCP socket. Establish a connection with a listening TCP socket using
    ///     <code>ConnectAsync</code>.
    ///     Use the <code>WriteStream</code> and <code>ReadStream</code> properties for sending and receiving data
    ///     respectively.
    /// </summary>
    public class TcpSocketClient : ITcpSocketClient
    {
        private readonly TcpClient _backingTcpClient;
        private SslStream _secureStream;

        /// <summary>
        ///     Default constructor for <code>TcpSocketClient</code>.
        /// </summary>
        public TcpSocketClient()
        {
            _backingTcpClient = new TcpClient();
        }

        internal TcpSocketClient(TcpClient backingClient)
        {
            _backingTcpClient = backingClient;
        }

        /// <summary>
        ///     Establishes a TCP connection with the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="address">The address of the endpoint to connect to.</param>
        /// <param name="port">The port of the endpoint to connect to.</param>
        /// <param name="secure">Is this connection secure?</param>
        public async Task ConnectAsync(string address, int port, bool secure = false)
        {
            await _backingTcpClient.ConnectAsync(address, port);
            if (secure)
            {
                var secureStream = new SslStream(_backingTcpClient.GetStream(), true, (sender, cert, chain, sslPolicy) => ServerValidationCallback(sender,cert,chain,sslPolicy));
                _secureStream.AuthenticateAsClient(address, null, System.Security.Authentication.SslProtocols.Tls, false);
                _secureStream = secureStream;
            }            
        }

        #region Secure Sockets Details
        
        private bool ServerValidationCallback (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            switch (sslPolicyErrors)
            {
                case SslPolicyErrors.RemoteCertificateNameMismatch:
                    Console.WriteLine("Server name mismatch. End communication ...\n");
                    return false;
                case SslPolicyErrors.RemoteCertificateNotAvailable:
                    Console.WriteLine("Server's certificate not available. End communication ...\n");
                    return false;
                case SslPolicyErrors.RemoteCertificateChainErrors:
                    Console.WriteLine("Server's certificate validation failed. End communication ...\n");
                    return false;
            }
            //TODO: Perform others checks using the "certificate" and "chain" objects ...

            Console.WriteLine("Server's authentication succeeded ...\n");
            return true;
        }

        #endregion

        /// <summary>
        ///     Disconnects from an endpoint previously connected to using <code>ConnectAsync</code>.
        ///     Should not be called on a <code>TcpSocketClient</code> that is not already connected.
        /// </summary>
        public async Task DisconnectAsync()
        {
            await Task.Run(() => {
                _backingTcpClient.Close();
                _secureStream = null;
            });
        }

        /// <summary>
        ///     A stream that can be used for receiving data from the remote endpoint.
        /// </summary>
        public Stream ReadStream
        {
            get
            {
                if (_secureStream != null)
                {
                    return _secureStream;
                }
                return _backingTcpClient.GetStream();
            }
        }

        /// <summary>
        ///     A stream that can be used for sending data to the remote endpoint.
        /// </summary>
        public Stream WriteStream
        {
            get
            {
                if (_secureStream != null)
                {
                    return _secureStream;
                }
                return _backingTcpClient.GetStream();
            }
        }

        private IPEndPoint RemoteEndpoint
        {
            get { return _backingTcpClient.Client.RemoteEndPoint as IPEndPoint; }
        }

        /// <summary>
        ///     The address of the remote endpoint to which the <code>TcpSocketClient</code> is currently connected.
        /// </summary>
        public string RemoteAddress
        {
            get { return RemoteEndpoint.Address.ToString(); }
        }

        /// <summary>
        ///     The port of the remote endpoint to which the <code>TcpSocketClient</code> is currently connected.
        /// </summary>
        public int RemotePort
        {
            get { return RemoteEndpoint.Port; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~TcpSocketClient()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_backingTcpClient != null)
                    ((IDisposable)_backingTcpClient).Dispose();
            }
        }
        
    }
}