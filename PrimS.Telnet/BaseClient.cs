namespace PrimS.Telnet
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The base class for Clients.
    /// </summary>
    public abstract partial class BaseClient : IDisposable
    {
        /// <summary>
        /// The default time out ms.
        /// </summary>
        protected const int DefaultTimeoutMs = 100;

        /// <summary>
        /// The byte stream.
        /// </summary>
        private readonly IByteStream byteStream;

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get
            {
                return this.byteStream.Connected;
            }
        }



        /// <summary>
        /// Gets the byte stream.
        /// </summary>
        protected IByteStream ByteStream
        {
            get
            {
                return this.byteStream;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception)
            {
                // NOP
            }
        }

        /// <summary>
        /// Determines whether the specified terminator has been located.
        /// </summary>
        /// <param name="terminator">The terminator to search for.</param>
        /// <param name="s">The content to search for the <paramref name="terminator"/>.</param>
        /// <returns>True if the terminator is located, otherwise false.</returns>
        protected static bool IsTerminatorLocated(string terminator, string s)
        {
            return s.TrimEnd().EndsWith(terminator, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the specified Regex has been located.
        /// </summary>
        /// <param name="regex">The Regex to search for.</param>
        /// <param name="s">The content to search for the <paramref name="regex"/>.</param>
        /// <returns>True if the Regex is matched, otherwise false.</returns>
        protected static bool IsRegexLocated(Regex regex, string s)
        {
            return regex.IsMatch(s);
        }
    }
}