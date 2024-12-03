using Microsoft.CodeAnalysis;
using FSharp.Compiler.CodeAnalysis;
using Microsoft.FSharp.Control;
using FSharp.Compiler.IO;
using Microsoft.FSharp.Core;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Concurrent;
using System.Text;
using Basic.Reference.Assemblies;
using System.Runtime.InteropServices;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Sharp.Compilation;

public class FSharpCompiler : ICompiler
{
    private static readonly VirtualFileSystem _fileSystem;

    private static readonly string[] _baseArguments;

    public Language Language => Language.FSharp;

    static FSharpCompiler()
    {
        FileSystemAutoOpens.FileSystem = _fileSystem = new VirtualFileSystem();

        var references = Net90.References.All.Select(r => r.FilePath!).Where(p => p is not "System.Runtime.dll").Select(p => $"-r:{p}");
        _baseArguments = [string.Empty, "--targetprofile:netcore", "--noframework", "--nowin32manifest" , "--checknulls", .. references, $"-r:{typeof(JitGenericAttribute).Assembly.Location}"];
    }

    private const string SourceName = "_.fs";
    private const string OutputName = "_.dll";

    private static string[] GetArguments(string projectPath)
    {
        return [.. _baseArguments, $"-o:{projectPath}/{OutputName}", $"{projectPath}/{SourceName}"];
    }

    public async ValueTask<bool> CompileAsync(ulong operationId, string code, ICollection<CompilationDiagnostic> diagnostics, Stream assembly, CompilationOutput? output)
    {
        var fileSystem = _fileSystem;
        var checker = FSharpChecker.Create(null, null, null, null, null, null, null, null, null, null, null, null, null, null);

        var projectVirtualPath = $"/{operationId}".AsMemory();

        var encoding = Encoding.UTF8;
        var pool = ArrayPool<byte>.Shared;

        int maxLength = encoding.GetMaxByteCount(code.Length);
        var bytes = pool.Rent(maxLength);
        int length = encoding.GetBytes(code, bytes);

        VirtualEntry.File sourceFile = new(new MemoryStream(bytes, 0, length));
        VirtualEntry.File outputFile = new(new NonDisposableStream(assembly));

        VirtualEntry.Directory projectDirectory = new([new(SourceName.AsMemory(), sourceFile), new(OutputName.AsMemory(), outputFile)]);

        var projectFullPath = fileSystem.AddVirtualEntry(projectVirtualPath, projectDirectory);

        try
        {
            var arguments = GetArguments(projectFullPath);

            var (resultDiagnostics, resultCode) = await FSharpAsync.StartAsTask(checker.Compile(arguments, null), null, null);

            int resultDiagnosticsLength = resultDiagnostics.Length;
            for (int i = 0; i < resultDiagnosticsLength; i++)
            {
                var resultDiagnostic = resultDiagnostics[i];

                diagnostics.Add(new((DiagnosticSeverity)resultDiagnostic.Severity.Tag,
                                    $"FS{resultDiagnostic.ErrorNumber:D4}",
                                    new(resultDiagnostic.StartLine - 1, resultDiagnostic.StartColumn),
                                    resultDiagnostic.Message));
            }

            return resultCode is 0;
        }
        finally
        {
            fileSystem.TryDeleteVirtualEntry(projectVirtualPath);
            pool.Return(bytes);
        }
    }

    private class NonDisposableStream(Stream stream) : Stream
    {
        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => stream.CanWrite;

        public override long Length => stream.Length;

        public override long Position { get => stream.Position; set => stream.Position = value; }

        public override void Flush()
        {
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            stream.Write(buffer);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return stream.WriteAsync(buffer, cancellationToken);
        }

        public override int Read(Span<byte> buffer)
        {
            return stream.Read(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return stream.ReadAsync(buffer, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return stream.FlushAsync(cancellationToken);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            return stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            return stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override bool CanTimeout => stream.CanTimeout;

        public override void CopyTo(Stream destination, int bufferSize)
        {
            stream.CopyTo(destination, bufferSize);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return stream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            stream.EndWrite(asyncResult);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte()
        {
            return stream.ReadByte();
        }

        public override int ReadTimeout { get => stream.ReadTimeout; set => stream.ReadTimeout = value; }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            stream.WriteByte(value);
        }

        public override int WriteTimeout { get => stream.WriteTimeout; set => stream.WriteTimeout = value; }

        public override void Close()
        {
        }

        protected override void Dispose(bool disposing)
        {
        }

        public override ValueTask DisposeAsync()
        {
            return default;
        }
    }

    private abstract class VirtualEntry
    {
        internal class File(Stream stream) : VirtualEntry
        {
            public Stream Stream => stream;
        }

        internal class Directory(IEnumerable<KeyValuePair<ReadOnlyMemory<char>, VirtualEntry>> entries) : VirtualEntry
        {
            public ConcurrentDictionary<ReadOnlyMemory<char>, VirtualEntry> Entries { get; } = new(entries, OrdinalReadOnlyMemoryCharComparer.Instance);

            private class OrdinalReadOnlyMemoryCharComparer : IEqualityComparer<ReadOnlyMemory<char>>
            {
                public static OrdinalReadOnlyMemoryCharComparer Instance { get; } = new();

                private OrdinalReadOnlyMemoryCharComparer()
                {
                }

                private readonly CompareInfo _compareInfo = CultureInfo.InvariantCulture.CompareInfo;

                public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
                {
                    return _compareInfo.Compare(x.Span, y.Span, CompareOptions.Ordinal) == 0;
                }

                public int GetHashCode([DisallowNull] ReadOnlyMemory<char> obj)
                {
                    return _compareInfo.GetHashCode(obj.Span, CompareOptions.Ordinal);
                }
            }
        }
    }

    private class VirtualFileSystem : DefaultFileSystem
    {
        private class VirtualFileStorage
        {
            private readonly VirtualEntry.Directory _root = new([]);

            public bool CreateEntry(ReadOnlyMemory<char> path, VirtualEntry entry)
            {
                var current = _root;

                foreach (var segmentRange in Segment(path))
                {
                    var segment = path[segmentRange];

                    if (current.Entries.TryGetValue(segment, out var nextEntry))
                    {
                        if (nextEntry is VirtualEntry.Directory nextDirectory)
                            current = nextDirectory;
                        else
                            return false;
                    }
                    else
                    {
                        if (IsLast(segment.Span, path.Span))
                        {
                            current.Entries[segment] = entry;
                            return true;
                        }
                        else
                            return false;
                    }
                }

                return false;
            }

            public bool DeleteEntry(ReadOnlyMemory<char> path)
            {
                var current = _root;
                VirtualEntry.Directory? parentDirectory = null;
                ReadOnlyMemory<char> lastSegment = default;

                foreach (var segmentRange in Segment(path))
                {
                    var segment = path[segmentRange];

                    if (!current.Entries.TryGetValue(segment, out var nextEntry))
                        return false;

                    if (IsLast(segment.Span, path.Span))
                    {
                        lastSegment = segment;
                        parentDirectory = current;
                        break;
                    }

                    if (nextEntry is VirtualEntry.Directory nextDirectory)
                        current = nextDirectory;
                    else
                        return false;
                }

                if (parentDirectory is not null && !lastSegment.Span.IsEmpty)
                    return parentDirectory.Entries.TryRemove(lastSegment, out _);

                return false;
            }

            public VirtualEntry? GetEntry(ReadOnlyMemory<char> path)
            {
                var current = _root;

                foreach (var segmentRange in Segment(path))
                {
                    var segment = path[segmentRange];

                    if (!current.Entries.TryGetValue(segment, out var nextEntry))
                        return null;

                    if (IsLast(segment.Span, path.Span))
                        return nextEntry;

                    if (nextEntry is VirtualEntry.Directory nextDirectory)
                        current = nextDirectory;
                    else
                        return null;
                }

                return null;
            }

            private static bool IsLast(ReadOnlySpan<char> segment, ReadOnlySpan<char> fullPath)
            {
                return Unsafe.AreSame(ref Unsafe.Add(ref MemoryMarshal.GetReference(fullPath), fullPath.Length), ref Unsafe.Add(ref MemoryMarshal.GetReference(segment), segment.Length));
            }

            private static IEnumerable<Range> Segment(ReadOnlyMemory<char> path)
            {
                if (path.IsEmpty)
                    yield break;

                int startIndex = path.Span[0] is '/' ? 1 : 0;
                int endIndex;
                while ((endIndex = path.Span[startIndex..].IndexOf('/')) >= 0)
                {
                    yield return startIndex..(endIndex + startIndex);
                    startIndex += endIndex + 1;
                }

                yield return startIndex..path.Length;
            }
        }

        private const string VirtualDirectory = "/$";

        private readonly VirtualFileStorage _virtualStorage = new();

        private static bool TryGetVirtualPath(ReadOnlyMemory<char> path, out ReadOnlyMemory<char> virtualPath)
        {
            var pathSpan = path.Span;
            if (pathSpan.StartsWith(VirtualDirectory) && (pathSpan.Length == VirtualDirectory.Length || pathSpan[VirtualDirectory.Length] is '/'))
            {
                virtualPath = path[VirtualDirectory.Length..];
                return true;
            }

            virtualPath = default;
            return false;
        }

        public string AddVirtualEntry(ReadOnlyMemory<char> virtualFullPath, VirtualEntry entry)
        {
            if (_virtualStorage.CreateEntry(virtualFullPath, entry))
                return $"{VirtualDirectory}{virtualFullPath}";

            throw new InvalidOperationException("Failed to create virtual entry.");
        }

        public bool TryDeleteVirtualEntry(ReadOnlyMemory<char> virtualFullPath)
        {
            return _virtualStorage.DeleteEntry(virtualFullPath);
        }

        public override string ChangeExtensionShim(string path, string extension)
        {
            throw new NotSupportedException();
        }

        public override void CopyShim(string src, string dest, bool overwrite)
        {
            throw new NotSupportedException();
        }

        public override string DirectoryCreateShim(string path)
        {
            throw new NotSupportedException();
        }

        public override void DirectoryDeleteShim(string path)
        {
            throw new NotSupportedException();
        }

        public override bool DirectoryExistsShim(string path)
        {
            if (TryGetVirtualPath(path.AsMemory(), out var virtualPath))
                return _virtualStorage.GetEntry(virtualPath) is VirtualEntry.Directory;

            return Directory.Exists(path);
        }

        public override IEnumerable<string> EnumerateDirectoriesShim(string path)
        {
            throw new NotSupportedException();
        }

        public override IEnumerable<string> EnumerateFilesShim(string path, string pattern)
        {
            throw new NotSupportedException();
        }

        public override void FileDeleteShim(string fileName)
        {
            throw new NotSupportedException();
        }

        public override bool FileExistsShim(string fileName)
        {
            if (TryGetVirtualPath(fileName.AsMemory(), out var virtualPath))
                return _virtualStorage.GetEntry(virtualPath) is VirtualEntry.File;

            return File.Exists(fileName);
        }

        public override DateTime GetCreationTimeShim(string path)
        {
            return default;
        }

        public override string GetDirectoryNameShim(string path)
        {
            return base.GetDirectoryNameShim(path);
        }

        public override string GetFullFilePathInDirectoryShim(string dir, string fileName)
        {
            return base.GetFullFilePathInDirectoryShim(dir, fileName);
        }

        public override string GetFullPathShim(string fileName)
        {
            return base.GetFullPathShim(fileName);
        }

        public override DateTime GetLastWriteTimeShim(string fileName)
        {
            return default;
        }

        public override string GetTempPathShim()
        {
            throw new NotSupportedException();
        }

        public override bool IsInvalidPathShim(string path)
        {
            return base.IsInvalidPathShim(path);
        }

        public override bool IsPathRootedShim(string path)
        {
            return base.IsPathRootedShim(path);
        }

        public override bool IsStableFileHeuristic(string fileName)
        {
            return base.IsStableFileHeuristic(fileName);
        }

        public override string NormalizePathShim(string path)
        {
            return base.NormalizePathShim(path);
        }

        public override Stream OpenFileForReadShim(string filePath, [OptionalArgument] FSharpOption<bool>? useMemoryMappedFile, [OptionalArgument] FSharpOption<bool>? shouldShadowCopy)
        {
            if (TryGetVirtualPath(filePath.AsMemory(), out var virtualPath))
            {
                if (_virtualStorage.GetEntry(virtualPath) is VirtualEntry.File file)
                    return file.Stream;

                throw new FileNotFoundException();
            }

            return base.OpenFileForReadShim(filePath, useMemoryMappedFile, shouldShadowCopy);
        }

        public override Stream OpenFileForWriteShim(string filePath, FSharpOption<FileMode>? fileMode, FSharpOption<FileAccess>? fileAccess, FSharpOption<FileShare>? fileShare)
        {
            if (TryGetVirtualPath(filePath.AsMemory(), out var virtualPath))
            {
                if (_virtualStorage.GetEntry(virtualPath) is VirtualEntry.File file)
                    return file.Stream;

                throw new FileNotFoundException();
            }

            throw new NotSupportedException();
        }
    }
}
