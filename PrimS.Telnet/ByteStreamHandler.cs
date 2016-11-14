namespace PrimS.Telnet
{
    using System;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides core functionality for interacting with the ByteStream.
    /// </summary>
    public partial class ByteStreamHandler : IByteStreamHandler
    {
        private readonly IByteStream byteStream;

        private bool IsResponsePending
        {
            get
            {
                return this.byteStream.Available > 0;
            }
        }

        private static DateTime ExtendRollingTimeout(TimeSpan timeout)
        {
            return DateTime.Now.Add(TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / 100));
        }

        private static async Task<bool> IsWaitForIncrementalResponse(DateTime rollingTimeout)

        {
            bool result = DateTime.Now < rollingTimeout;
            await Task.Delay(1);

            return result;
        }

        private static bool IsWaitForInitialResponse(DateTime endInitialTimeout, bool isInitialResponseReceived)
        {
            return !isInitialResponseReceived && DateTime.Now < endInitialTimeout;
        }

        private static bool IsRollingTimeoutExpired(DateTime rollingTimeout)
        {
            return DateTime.Now >= rollingTimeout;
        }

        private static bool IsInitialResponseReceived(StringBuilder sb)
        {
            return sb.Length > 0;
        }

        private bool RetrieveAndParseResponse(StringBuilder sb)
        {
            if (this.IsResponsePending)
            {
                int input = this.byteStream.ReadByte();
                switch (input)
                {
                    case -1:
                        break;
                    case (int)Commands.InterpretAsCommand:
                        // interpret as command
                        int inputVerb = this.byteStream.ReadByte();
                        if (inputVerb == -1)
                        {
                            break;
                        }

                        switch (inputVerb)
                        {
                            case (int)Commands.InterpretAsCommand:
                                // literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputVerb);
                                break;
                            case (int)Commands.Do:
                            case (int)Commands.Dont:
                            case (int)Commands.Will:
                            case (int)Commands.Wont:
                                this.ReplyToCommand(inputVerb);
                                break;
                            default:
                                break;
                        }

                        break;
                    default:
                        sb.Append((char)input);
                        break;
                }

                return true;
            }

            return false;
        }

        private void ReplyToCommand(int inputVerb)
        {
            // reply to all commands with "WONT", unless it is SGA (suppress go ahead)
            int inputOption = this.byteStream.ReadByte();
            if (inputOption != -1)
            {
                this.byteStream.WriteByte((byte)Commands.InterpretAsCommand);
                if (inputOption == (int)Options.SuppressGoAhead)
                {
                    this.byteStream.WriteByte(inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do);
                }
                else
                {
                    this.byteStream.WriteByte(inputVerb == (int)Commands.Do ? (byte)Commands.Wont : (byte)Commands.Dont);
                }

                this.byteStream.WriteByte((byte)inputOption);
            }
        }

        private async Task<bool> IsResponseAnticipated(bool isInitialResponseReceived, DateTime endInitialTimeout, DateTime rollingTimeout)

        {
            return this.IsResponsePending || IsWaitForInitialResponse(endInitialTimeout, isInitialResponseReceived) || await IsWaitForIncrementalResponse(rollingTimeout);
        }
    }
}