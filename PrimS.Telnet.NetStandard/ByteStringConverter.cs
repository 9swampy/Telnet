
namespace PrimS.Telnet
{
  using System.Text;

  internal static class ByteStringConverter
  {
    public static byte[] ConvertStringToByteArray(string value)
    {
      var buffer = ASCIIEncoding.ASCII.GetBytes(value.Replace("\0xFF", "\0xFF\0xFF"
#if NET6_0_OR_GREATER
        , StringComparison.InvariantCulture
#endif
        ));
      return buffer;
    }

    public static string ToString(byte[] bytes)
    {
      return ToString(bytes, 0, bytes.Length);
    }

    internal static string ToString(byte[] bytes, int offset, int count)
    {
      return Encoding.ASCII.GetString(bytes, offset, count).Trim((char)255);
    }
  }
}
