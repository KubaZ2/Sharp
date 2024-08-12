using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;

using Sharp.Compilation;
using Sharp.CompilationResponse;
using Sharp.Decompilation;

namespace Sharp.Responding;

public class ResponseProvider(IOptions<Options> options, ICompilationFormatter compilationFormatter, ILanguageFormatProvider languageFormatProvider) : IResponseProvider
{
    public T CompilationResultResponse<T>(ulong operationId, CompilationResult result) where T : IMessageProperties, new()
    {
        return result switch
        {
            CompilationResult.Success => CompilationSuccessResponse<T>(),
            CompilationResult.CompilerNotFound { Language: var language } => CompilerNotFoundResponse<T>(language),
            CompilationResult.Fail { Diagnostics: var diagnostics } => CompilationFailResponse<T>(operationId, diagnostics),
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
        return Error<T>("Compiler not found", $"The compiler for the language `{language}` was not found.");
    }

    private T CompilationFailResponse<T>(ulong operationId, IReadOnlyList<Diagnostic> diagnostics) where T : IMessageProperties, new()
    {
        var compilationFormatResult = compilationFormatter.CompilationResponse(operationId, diagnostics, false);

        return new()
        {
            Embeds = [compilationFormatResult.Embed],
            Components = compilationFormatResult.Components,
        };
    }

    public T DecompilationResultResponse<T>(ulong operationId, DecompilationResult result) where T : IMessageProperties, new()
    {
        return result switch
        {
            DecompilationResult.Success => DecompilationSuccessResponse<T>(),
            DecompilationResult.DecompilerNotFound { Language: var language } => DecompilerNotFoundResponse<T>(language),
            DecompilationResult.Fail { Language: var language } => DecompilationFailResponse<T>(language),
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
        return Error<T>("Decompiler not found", $"The decompiler for the language `{language}` was not found.");
    }

    private T DecompilationFailResponse<T>(Language language) where T : IMessageProperties, new()
    {
        return Error<T>("Decompilation failed", $"The decompilation for the language `{language}` failed.");
    }

    public T LanguageNotFoundResponse<T>(ulong operationId, string? language) where T : IMessageProperties, new()
    {
        return Error<T>("Language not found", $"The language `{language}` was not found.");
    }

    public T UnknownError<T>(string reason) where T : IMessageProperties, new()
    {
        return Error<T>("An error occurred", reason);
    }

    public T DecompilationResponse<T>(ulong operationId, Language language, string decompiledCode, List<Diagnostic> diagnostics) where T : IMessageProperties, new()
    {
        T message = new();

        var compilationFormatResult = compilationFormatter.CompilationResponse(operationId, diagnostics, true);

        message.Embeds = [compilationFormatResult.Embed];
        message.Components = compilationFormatResult.Components;

        message.AddAttachments(new AttachmentProperties($"source.{languageFormatProvider.GetFormat(language)}", new MemoryStream(Encoding.UTF8.GetBytes(decompiledCode))));

        return message;
    }

    public T RunResponse<T>(ulong operationId, Language language, string output, List<Diagnostic> diagnostics) where T : IMessageProperties, new()
    {
        T message = new();

        var compilationFormatResult = compilationFormatter.CompilationResponse(operationId, diagnostics, true);

        message.Embeds = [compilationFormatResult.Embed];
        message.Components = compilationFormatResult.Components;

        message.AddAttachments(new AttachmentProperties($"output.txt", new MemoryStream(Encoding.UTF8.GetBytes(output))));

        return message;
    }

    public T RateLimitResponse<T>(ulong operationId) where T : IMessageProperties, new()
    {
        return Error<T>("Rate limit exceeded", "You have exceeded the rate limit. Please try again later.");
    }

    public T HelpResponse<T>(ulong operationId) where T : IMessageProperties, new()
    {
        T message = new();

        var optionsValue = options.Value;

        message.AddEmbeds(new EmbedProperties().WithDescription(
                                                $"""
                                                # Help

                                                ## Commands
                                                - `#run <architecture?> <code>` - runs the provided code, uses {optionsValue.Backend.DefaultArchitecture} architecture by default
                                                - `#<language> <code>` - decompiles the provided code to the specified language
                                                - `#<architecture> <code>` - shows the architecture-specific JIT disassembly of the provided code
                                                ## Support
                                                ### Compilation
                                                - C#
                                                - Visual Basic
                                                - IL
                                                ### Decompilation
                                                - C#
                                                - IL
                                                ### Architectures
                                                - Arm64
                                                ## Examples
                                                #run
                                                \```c#
                                                Console.Write("Hello, World!");
                                                \```

                                                #c#
                                                \```c#
                                                Console.Write("Hello, World!");
                                                \```

                                                #il
                                                \```c#
                                                Console.Write("Hello, World!");
                                                \```

                                                #arm64
                                                \```c#
                                                Console.Write("Hello, World!");
                                                \```
                                                """)
                                               .WithColor(new(optionsValue.PrimaryColor))
                                               .WithTimestamp(Snowflake.CreatedAt(operationId)));

        return message;
    }

    private T Error<T>(string title, string reason) where T : IMessageProperties, new()
    {
        T message = new();

        var optionsValue = options.Value;

        message.AddEmbeds(new EmbedProperties().WithTitle($"{optionsValue.Emojis.Error} {title}")
                                               .WithDescription(reason)
                                               .WithColor(new(optionsValue.PrimaryColor)));

        if (message is InteractionMessageProperties interactionMessage)
            interactionMessage.WithFlags(MessageFlags.Ephemeral);

        return message;
    }
}
