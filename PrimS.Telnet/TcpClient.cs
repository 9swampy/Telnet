namespace PrimS.Telnet
{
    using System;

    /// <summary>
    /// A TcpClient to connect to the specified socket.
    /// </summary>
    public class TcpClient : ISocket, IDisposable
    {
        private readonly System.Net.Sockets.TcpClient client;

        /// <summary>
        /// Initialises a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="hostName">The host name.</param>
        /// <param name="port">The port.</param>
        public TcpClient(string hostName, int port)
        {
            this.client = new System.Net.Sockets.TcpClient();
            this.client.ConnectAsync(hostName, port).GetAwaiter().GetResult();
        }



        /// <summary>
        /// Gets or sets the receive timeout.
        /// </summary>
        /// <value>
        /// The receive timeout.
        /// </value>
        public int ReceiveTimeout
        {
            get
            {
                return this.client.ReceiveTimeout;
            }

            set
            {
                this.client.ReceiveTimeout = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ISocket" /> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        public bool Connected
        {
            get
            {
                return this.client.Connected;
            }
        }

        /// <summary>
        /// Gets the available bytes to be read.
        /// </summary>
        /// <value>
        /// The available bytes to be read.
        /// </value>
        public int Available
        {
            get
            {
                return this.client.Available;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            this.client.Dispose();
        }

        /// <summary>
        /// Gets the stream.
        /// </summary>
        /// <returns>
        /// Network stream socket connected to.
        /// </returns>
        public INetworkStream GetStream()
        {
            return new NetworkStream(this.client.GetStream());
        }

        private void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this.client.Client.Dispose();
                this.client.Dispose();

            }
        }
    }
}