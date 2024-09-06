using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace Sharp.Backend.Manager;

public class JailSandboxProvider : ISandboxProvider
{
    private const int MaxOutputSize = 1024 * 1024; // 1MiB
    private const int CopyBufferSize = 81920;

    public async Task ExecuteAsync(ContainerFunction function, Stream assembly, Stream output)
    {
        using TcpClient client = new();

        (string host, int port) = function switch
        {
            ContainerFunction.Run => ("sharp-backend-runner", 5000),
            ContainerFunction.Assembly => ("sharp-backend-asm", 6000),
            _ => ThrowOutOfRange(),
        };

        var connectTask = client.ConnectAsync(host, port);

        MemoryStream ms = new(assembly.CanSeek ? (int)assembly.Length : 0);
        await assembly.CopyToAsync(ms);
        int length = (int)ms.Length;

        var pool = ArrayPool<byte>.Shared;

        var buffer = pool.Rent(Math.Max(Base64.GetMaxEncodedToUtf8Length(length) + 1, CopyBufferSize));
        try
        {
            Base64.EncodeToUtf8(ms.GetBuffer().AsSpan(0, length), buffer, out _, out int bytesWritten);
            buffer[bytesWritten] = (byte)'\n';

            await connectTask;

            var stream = client.GetStream();

            await stream.WriteAsync(buffer.AsMemory(0, bytesWritten + 1));
            await stream.FlushAsync();

            await ReadOutputAsync(stream, output, buffer);
        }
        finally
        {
            pool.Return(buffer);
        }

        [DoesNotReturn]
        static (string, int) ThrowOutOfRange() => throw new ArgumentOutOfRangeException(nameof(function));
    }

    private static async Task ReadOutputAsync(NetworkStream input, Stream output, byte[] buffer)
    {
        int bufferLength = buffer.Length;
        int read;
        int remaining = MaxOutputSize;
        while (true)
        {
            read = await input.ReadAsync(buffer.AsMemory(0, Math.Min(bufferLength, remaining)));

            if (read is 0)
                break;

            await output.WriteAsync(buffer.AsMemory(0, read));

            if ((remaining -= read) is 0)
                break;
        }

        await output.FlushAsync();
    }
}
