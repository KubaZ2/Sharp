using System.Buffers;
using System.Buffers.Text;
using System.Reflection;

namespace Sharp.Backend.Sandbox.Shared;

public static class Utils
{
    public static Assembly LoadAssembly()
    {
        ArrayBufferWriter<byte> writer = new(4096);

        var stdin = Console.OpenStandardInput();
        int read;
        Span<byte> buffer = stackalloc byte[4096];
        while ((read = stdin.Read(buffer)) > 0)
        {
            var end = buffer[read - 1] is (byte)'\n';

            if (end)
                read--;

            int max = Base64.GetMaxDecodedFromUtf8Length(read);
            Base64.DecodeFromUtf8(buffer[..read], writer.GetSpan(max), out _, out int bytesWritten, end);
            writer.Advance(bytesWritten);

            if (end)
                break;
        }

        return Assembly.Load(writer.WrittenSpan.ToArray());
    }
}
