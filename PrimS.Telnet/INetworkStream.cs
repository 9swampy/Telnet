namespace PrimS.Telnet
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A network stream that can be read and written to.
    /// </summary>
    public interface INetworkStream
    {
        /// <summary>
        /// Reads the next byte.
        /// </summary>
        /// <returns>The next byte read.</returns>
        int ReadByte();

        /// <summary>
        /// Writes the byte.
        /// </summary>
        /// <param name="value">The value to write.</param>
        void WriteByte(byte value);

        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>An awaitable task.</returns>
        Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

    }
}