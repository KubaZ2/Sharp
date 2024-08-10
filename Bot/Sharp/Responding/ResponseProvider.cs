using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;

using Sharp.Compilation;
using Sharp.Decompilation;
using Sharp.Diagnostics;

namespace Sharp.Responding;

public class ResponseProvider(IOptions<Options> options, IDiagnosticsFormatter diagnosticsFormatter, ILanguageFormatProvider languageFormatProvider) : IResponseProvider
{
    public T CompilationResultResponse<T>(ulong operationId, CompilationResult result) where T : IMessageProperties, new()
    {
        return result switch
        {
            CompilationResult.Success => CompilationSuccessResponse<T>(),
            CompilationResult.CompilerNotFound compilerNotFound => CompilerNotFoundResponse<T>(compilerNotFound.Language),
            CompilationResult.Fail fail => CompilationFailResponse<T>(operationId, fail.Diagnostics),
            _ => throw new ArgumentOutOfRangeException(nameof(result)),
        };
    }

    private T CompilationSuccessResponse<T>() where T : IMessageProperties, new()
    {
        T message = new();

        var optionsValue = options.Value;

        message.AddEmbeds(new EmbedProperties().WithTitle($"{optionsValue.Emojis.Success} Compilation successful")
                                               .WithColor(new(optionsValue.PrimaryColor)));

        return message;
    }

    private T CompilerNotFoundResponse<T>(Language language) where T : IMessageProperties, new()
    {
        T message = new();

        var optionsValue = options.Value;

        message.AddEmbeds(new EmbedProperties().WithTitle($"{optionsValue.Emojis.Error} Compiler not found")
                                               .WithDescription($"The compiler for the language `{language}` was not found.")
                                               .WithColor(new(optionsValue.PrimaryColor)));

        return message;
    }

    private T CompilationFailResponse<T>(ulong operationId, IReadOnlyList<Diagnostic> diagnostics) where T : IMessageProperties, new()
    {
        var formatResult = diagnosticsFormatter.FormatDiagnostics(operationId, diagnostics, false);

        if (formatResult.Expired)
        {
            T message = new();

            var optionsValue = options.Value;

            message.AddEmbeds(new EmbedProperties().WithTitle($"{optionsValue.Emojis.Error} The diagnostics are no longer available")
                                                   .WithDescription("The diagnostics for the operation are no longer available.")
                                                   .WithColor(new(optionsValue.PrimaryColor)));

            return message;
        }

        return new()
        {
            Embeds = formatResult.Embeds,
            Components = formatResult.Components,
        };
    }

    public T DecompilationResultResponse<T>(ulong operationId, DecompilationResult result) where T : IMessageProperties, new()
    {
        return result switch
        {
            DecompilationResult.Success => DecompilationSuccessResponse<T>(),
            DecompilationResult.DecompilerNotFound decompilerNotFound => DecompilerNotFoundResponse<T>(decompilerNotFound.Language),
            DecompilationResult.Fail fail => DecompilationFailResponse<T>(fail.Language),
            _ => throw new ArgumentOutOfRangeException(nameof(result)),
        };
    }

    private T DecompilationSuccessResponse<T>() where T : IMessageProperties, new()
    {
        T message = new();

        var optionsValue = options.Value;

        message.AddEmbeds(new EmbedProperties().WithTitle($"{optionsValue.Emojis.Success} Decompilation successful")
                                               .WithColor(new(optionsValue.PrimaryColor)));

        return message;
    }

    private T DecompilerNotFoundResponse<T>(Language language) where T : IMessageProperties, new()
    {
        T message = new();

        var optionsValue = options.Value;

        message.AddEmbeds(new EmbedProperties().WithTitle($"{optionsValue.Emojis.Error} Decompiler not found")
                                               .WithDescription($"The decompiler for the language `{language}` was not found.")
                                               .WithColor(new(optionsValue.PrimaryColor)));

        return message;
    }

    private T DecompilationFailResponse<T>(Language language) where T : IMessageProperties, new()
    {
        T message = new();

        var optionsValue = options.Value;

        message.AddEmbeds(new EmbedProperties().WithTitle($"{optionsValue.Emojis.Error} Decompilation failed")
                                               .WithDescription($"The decompilation for the language `{language}` failed.")
                                               .WithColor(new(optionsValue.PrimaryColor)));

        return message;
    }

    public T LanguageNotFoundResponse<T>(ulong operationId, string? language) where T : IMessageProperties, new()
    {
        T message = new();

        var optionsValue = options.Value;

        message.AddEmbeds(new EmbedProperties().WithTitle($"{optionsValue.Emojis.Error} Language not found")
                                               .WithDescription($"The language `{language}` was not found.")
                                               .WithColor(new(optionsValue.PrimaryColor)));

        return message;
    }

    public T Error<T>(string reason) where T : IMessageProperties, new()
    {
        T message = new();

        var optionsValue = options.Value;

        message.AddEmbeds(new EmbedProperties().WithTitle($"{optionsValue.Emojis.Error} An error occurred")
                                               .WithDescription(reason)
                                               .WithColor(new(optionsValue.PrimaryColor)));

        if (message is InteractionMessageProperties interactionMessage)
            interactionMessage.WithFlags(MessageFlags.Ephemeral);

        return message;
    }

    public T DecompilationResponse<T>(ulong operationId, Language language, string decompiledCode, List<Diagnostic> diagnostics) where T : IMessageProperties, new()
    {
        T message = new();

        var formatResult = diagnosticsFormatter.FormatDiagnostics(operationId, diagnostics, true);

        message.Embeds = formatResult.Embeds;
        message.Components = formatResult.Components;

        message.AddAttachments(new AttachmentProperties($"source.{languageFormatProvider.GetFormat(language)}", new MemoryStream(Encoding.UTF8.GetBytes(decompiledCode))));

        return message;
    }

    public T RunResponse<T>(ulong operationId, Language language, string output, List<Diagnostic> diagnostics) where T : IMessageProperties, new()
    {
        T message = new();

        var formatResult = diagnosticsFormatter.FormatDiagnostics(operationId, diagnostics, true);

        message.Embeds = formatResult.Embeds;
        message.Components = formatResult.Components;

        message.AddAttachments(new AttachmentProperties($"output.txt", new MemoryStream(Encoding.UTF8.GetBytes(output))));

        return message;
    }

    public T RateLimitResponse<T>(ulong operationId) where T : IMessageProperties, new()
    {
        T message = new();

        var optionsValue = options.Value;

        message.AddEmbeds(new EmbedProperties().WithTitle($"{optionsValue.Emojis.Error} Rate limit exceeded")
                                               .WithDescription("You have exceeded the rate limit. Please try again later.")
                                               .WithColor(new(optionsValue.PrimaryColor)));

        return message;
    }
}
