using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Sockets.Plugin.Abstractions;

// ReSharper disable once CheckNamespace

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
        private readonly StreamSocket _backingStreamSocket;

        /// <summary>
        ///     Default constructor for <code>TcpSocketClient</code>.
        /// </summary>
        public TcpSocketClient()
        {
            _backingStreamSocket = new StreamSocket();
            ;
        }

        internal TcpSocketClient(StreamSocket nativeSocket)
        {
            _backingStreamSocket = nativeSocket;
        }

        /// <summary>
        ///     Establishes a TCP connection with the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="address">The address of the endpoint to connect to.</param>
        /// <param name="port">The port of the endpoint to connect to.</param>
        public async Task ConnectAsync(string address, int port)
        {
            var hn = new HostName(address);
            var sn = port.ToString();

            await _backingStreamSocket.ConnectAsync(hn, sn);
        }

        /// <summary>
        ///     Disconnects from an endpoint previously connected to using <code>ConnectAsync</code>.
        ///     Should not be called on a <code>TcpSocketClient</code> that is not already connected.
        /// </summary>
        public async Task DisconnectAsync()
        {
            await Task.Run(() => _backingStreamSocket.Dispose());
        }

        /// <summary>
        ///     A stream that can be used for receiving data from the remote endpoint.
        /// </summary>
        public Stream ReadStream
        {
            get { return _backingStreamSocket.InputStream.AsStreamForRead(); }
        }

        /// <summary>
        ///     A stream that can be used for sending data to the remote endpoint.
        /// </summary>
        public Stream WriteStream
        {
            get { return _backingStreamSocket.OutputStream.AsStreamForWrite(); }
        }

        /// <summary>
        ///     The address of the remote endpoint to which the <code>TcpSocketClient</code> is currently connected.
        /// </summary>
        public string RemoteAddress
        {
            get { return _backingStreamSocket.Information.RemoteAddress.CanonicalName; }
        }

        /// <summary>
        ///     The port of the remote endpoint to which the <code>TcpSocketClient</code> is currently connected.
        /// </summary>
        public int RemotePort
        {
            get { return Int32.Parse(_backingStreamSocket.Information.RemotePort); }
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
                if (_backingStreamSocket != null)
                    (_backingStreamSocket).Dispose();
            }
        }
    }
}