namespace PrimS.Telnet
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    /// <summary>
    /// Provides core functionality for interacting with the ByteStream.
    /// </summary>
    public partial class ByteStreamHandler
    {
        private readonly CancellationTokenSource internalCancellation;

        /// <summary>
        /// Initialises a new instance of the <see cref="ByteStreamHandler"/> class.
        /// </summary>
        /// <param name="byteStream">The byteStream to handle.</param>
        /// <param name="internalCancellation">A cancellation token.</param>
        public ByteStreamHandler(IByteStream byteStream, CancellationTokenSource internalCancellation)
        {
            this.byteStream = byteStream;
            this.internalCancellation = internalCancellation;
        }

        private bool IsCancellationRequested
        {
            get
            {
                return this.internalCancellation.Token.IsCancellationRequested;
            }
        }

        /// <summary>
        /// Reads asynchronously from the stream.
        /// </summary>
        /// <param name="timeout">The rolling timeout to wait for no further response from stream.</param>
        /// <returns>Any text read from the stream.</returns>
        public async Task<string> ReadAsync(TimeSpan timeout)

        {
            if (!this.byteStream.Connected || this.internalCancellation.Token.IsCancellationRequested)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            this.byteStream.ReceiveTimeout = (int)timeout.TotalMilliseconds;
            DateTime endInitialTimeout = DateTime.Now.Add(timeout);
            DateTime rollingTimeout = ExtendRollingTimeout(timeout);
            do
            {
                if (this.RetrieveAndParseResponse(sb))
                {
                    rollingTimeout = ExtendRollingTimeout(timeout);
                }
            }
            while (!this.IsCancellationRequested && await this.IsResponseAnticipated(IsInitialResponseReceived(sb), endInitialTimeout, rollingTimeout));
            if (IsRollingTimeoutExpired(rollingTimeout))
            {
                System.Console.WriteLine($"RollingTimeout exceeded {DateTime.Now.ToString("ss:fff")}");
            }

            return sb.ToString();
        }
    }
}
