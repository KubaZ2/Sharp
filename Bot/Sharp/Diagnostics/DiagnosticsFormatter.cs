using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;

using Sharp.Compilation;

namespace Sharp.Diagnostics;

public class DiagnosticsFormatter(IOptions<Options> options, IMemoryCache cache) : IDiagnosticsFormatter
{
    private record DiagnosticsCacheEntry(IReadOnlyList<CompilationDiagnostic> Diagnostics);

    public DiagnosticsFormatResult FormatDiagnostics(ulong operationId, bool success, int page, int embedContentLength)
    {
        var entry = cache.Get<DiagnosticsCacheEntry>(operationId);

        if (entry is null)
            return new DiagnosticsFormatResult.Expired();

        var fields = CreateDiagnosticsFields(entry.Diagnostics, page, embedContentLength, out var more);

        var actionRow = CreateActionRow(success, page, page is 1, !more);

        return new DiagnosticsFormatResult.Success(fields, [actionRow]);
    }

    public DiagnosticsFormatResult.Success FormatDiagnostics(ulong operationId, bool success, IReadOnlyList<CompilationDiagnostic> diagnostics, int embedContentLength)
    {
        IReadOnlyList<CompilationDiagnostic> visibleDiagnostics = [.. diagnostics.Where(d => d.Severity is not DiagnosticSeverity.Hidden)];

        var fields = CreateDiagnosticsFields(visibleDiagnostics, 1, embedContentLength, out var more);

        IEnumerable<ComponentProperties>? components;

        if (more)
        {
            cache.Set(operationId, new DiagnosticsCacheEntry(visibleDiagnostics), new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30)));

            components = [CreateActionRow(success, 1, true, false)];
        }
        else
            components = null;

        return new DiagnosticsFormatResult.Success(fields, components);
    }

    private static ActionRowProperties CreateActionRow(bool success, int page, bool previousDisabled, bool nextDisabled)
    {
        return
        [
            new ButtonProperties($"diagnostics:{success}:{page - 1}", new EmojiProperties("⬅️"), ButtonStyle.Secondary) { Disabled = previousDisabled },
            new ButtonProperties($"diagnostics:{success}:{page + 1}", new EmojiProperties("➡️"), ButtonStyle.Secondary) { Disabled = nextDisabled }
        ];
    }

    private string GetSeverityEmoji(DiagnosticSeverity severity)
    {
        return severity switch
        {
            DiagnosticSeverity.Info => options.Value.Emojis.Diagnostics.Info,
            DiagnosticSeverity.Warning => options.Value.Emojis.Diagnostics.Warning,
            DiagnosticSeverity.Error => options.Value.Emojis.Diagnostics.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(severity)),
        };
    }

    private List<EmbedFieldProperties> CreateDiagnosticsFields(IReadOnlyList<CompilationDiagnostic> diagnostics, int page, int length, out bool more)
    {
        int diagnosticCount = diagnostics.Count;
        var pageStartIndex = GetPageStartIndex(diagnostics, page, length);

        if (!pageStartIndex.HasValue)
        {
            more = false;
            return [];
        }

        int diagnosticIndex = pageStartIndex.GetValueOrDefault();

        List<EmbedFieldProperties> fields = new(Math.Min(25, diagnosticCount - diagnosticIndex));

        for (int pageDiagnosticIndex = 0; diagnosticIndex < diagnosticCount; diagnosticIndex++)
        {
            if (pageDiagnosticIndex++ is 25)
            {
                more = true;
                return fields;
            }

            var diagnostic = diagnostics[diagnosticIndex];

            var name = LimitLength(FormatName(diagnostic), 256);
            var value = LimitLength(FormatValue(diagnostic), 1024);

            length += name.Length + value.Length;
            if (length > 6000)
            {
                more = true;
                return fields;
            }

            fields.Add(new EmbedFieldProperties().WithName(name).WithValue(value));
        }

        more = false;
        return fields;
    }

    private int? GetPageStartIndex(IReadOnlyList<CompilationDiagnostic> diagnostics, int page, int length)
    {
        int diagnosticIndex = 0;
        int diagnosticCount = diagnostics.Count;

        int i = 0;
        int currentPage = 1;
        if (currentPage != page)
        {
            int simulatedLength = length;
            while (true)
            {
                if (diagnosticIndex == diagnosticCount)
                    return null;

                var diagnostic = diagnostics[diagnosticIndex];

                var nameLength = GetLength(FormatName(diagnostic), 256);
                var valueLength = GetLength(FormatValue(diagnostic), 1024);

                simulatedLength += nameLength + valueLength;
                if (simulatedLength > 6000)
                {
                    if (++currentPage == page)
                        break;

                    simulatedLength = length + nameLength + valueLength;
                    i = 1;
                    diagnosticIndex++;
                    continue;
                }

                if (++i is 25)
                {
                    if (++currentPage == page)
                    {
                        diagnosticIndex++;
                        break;
                    }

                    simulatedLength = length;
                    i = 0;
                }

                diagnosticIndex++;
            }
        }

        return diagnosticIndex;
    }

    private string FormatName(CompilationDiagnostic diagnostic)
    {
        var location = diagnostic.Location;
        return $"{GetSeverityEmoji(diagnostic.Severity)} {diagnostic.Id} ({location.Line + 1},{location.Character + 1})";
    }

    private static string FormatValue(CompilationDiagnostic diagnostic) => diagnostic.Message;

    private static string LimitLength(string value, int maxLength)
    {
        if (value.Length <= maxLength)
            return value;

        return maxLength > 3 ? $"{value.AsSpan(0, maxLength - 3)}..." : string.Empty;
    }

    private static int GetLength(string value, int maxLength)
    {
        if (value.Length <= maxLength)
            return value.Length;

        return maxLength > 3 ? maxLength : 0;
    }
}
