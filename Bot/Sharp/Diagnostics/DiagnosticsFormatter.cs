using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;

namespace Sharp.Diagnostics;

public class DiagnosticsFormatter(IOptions<Options> options, IMemoryCache cache) : IDiagnosticsFormatter
{
    private record DiagnosticsCacheEntry(IReadOnlyList<Diagnostic> Diagnostics, bool CompilationSucceeded);

    public DiagnosticsFormatResult FormatDiagnostics(ulong operationId, int page)
    {
        var entry = cache.Get<DiagnosticsCacheEntry>(operationId);

        if (entry is null)
        {
            var embed = new EmbedProperties().WithTitle("The diagnostics are no longer available")
                                             .WithDescription("The diagnostics for the operation are no longer available.")
                                             .WithColor(new(options.Value.PrimaryColor));

            return new([embed], null, true);
        }

        return FormatDiagnostics(operationId, entry.Diagnostics, entry.CompilationSucceeded, page);
    }

    public DiagnosticsFormatResult FormatDiagnostics(ulong operationId, IReadOnlyList<Diagnostic> diagnostics, bool compilationSucceeded)
    {
        IReadOnlyList<Diagnostic> visibleDiagnostics = [.. diagnostics.Where(d => d.Severity is not DiagnosticSeverity.Hidden)];
        var embed = CreateCompilationResultEmbed(operationId, visibleDiagnostics, compilationSucceeded, 1, out var more);

        IEnumerable<MessageComponentProperties>? components;

        if (more)
        {
            cache.Set(operationId, new DiagnosticsCacheEntry(visibleDiagnostics, compilationSucceeded), new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30)));

            components = [CreateActionRow(1, true, false)];
        }
        else
            components = null;

        return new([embed], components);
    }

    private DiagnosticsFormatResult FormatDiagnostics(ulong operationId, IReadOnlyList<Diagnostic> diagnostics, bool compilationSucceeded, int page)
    {
        var embed = CreateCompilationResultEmbed(operationId, diagnostics, compilationSucceeded, page, out var more);

        var actionRow = CreateActionRow(page, page is 1, !more);

        return new([embed], [actionRow]);
    }

    private static ActionRowProperties CreateActionRow(int page, bool previousDisabled, bool nextDisabled)
    {
        return new(
        [
            new ButtonProperties($"diagnostics:{page - 1}", new EmojiProperties("⬅️"), ButtonStyle.Secondary) { Disabled = previousDisabled },
            new ButtonProperties($"diagnostics:{page + 1}", new EmojiProperties("➡️"), ButtonStyle.Secondary) { Disabled = nextDisabled }
        ]);
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

    private EmbedProperties CreateCompilationResultEmbed(ulong operationId, IReadOnlyList<Diagnostic> diagnostics, bool compilationSucceeded, int page, out bool more)
    {
        var optionsValue = options.Value;

        var title = $"{(compilationSucceeded ? optionsValue.Emojis.Success : optionsValue.Emojis.Error)} Compilation {(compilationSucceeded ? "succeeded" : "failed")}";
        var description = $"The compilation {(compilationSucceeded ? "succeeded" : "failed")}.";
        int length = title.Length + description.Length;

        EmbedProperties embed = new()
        {
            Title = title,
            Description = description,
            Color = new(optionsValue.PrimaryColor),
            Fields = CreateDiagnosticsFields(diagnostics, page, length, out more),
            Timestamp = Snowflake.CreatedAt(operationId),
        };

        if (diagnostics.Count is not 0)
            embed.Footer = new EmbedFooterProperties().WithText($"Page {page}");

        return embed;
    }

    private List<EmbedFieldProperties> CreateDiagnosticsFields(IReadOnlyList<Diagnostic> diagnostics, int page, int length, out bool more)
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

    private int? GetPageStartIndex(IReadOnlyList<Diagnostic> diagnostics, int page, int length)
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

    private string FormatName(Diagnostic diagnostic)
    {
        var position = diagnostic.Location.GetMappedLineSpan().Span.Start;
        return $"{GetSeverityEmoji(diagnostic.Severity)} {diagnostic.Id} ({position.Line + 1},{position.Character + 1})";
    }

    private static string FormatValue(Diagnostic diagnostic)
    {
        return diagnostic.GetMessage();
    }

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
