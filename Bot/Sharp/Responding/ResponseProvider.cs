using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;

using Sharp.Attachments;
using Sharp.Names;
using Sharp.Backend;
using Sharp.Compilation;
using Sharp.CompilationResponse;
using Sharp.Decompilation;
using NetCord.Hosting.Services.Commands;
using NetCord.Services.Commands;

namespace Sharp.Responding;

public class ResponseProvider(IOptions<Options> options, IOptions<CommandServiceOptions<CommandContext>> discordOptions, ICompilationFormatter compilationFormatter, ILanguageFormatProvider languageFormatProvider, IBackendUriProvider backendUriProvider, INameFormatter nameFormatter, ICompilerProvider compilerProvider, IDecompilerProvider decompilerProvider) : IResponseProvider
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
        return Error<T>("Compiler not found", $"The compiler for the language {nameFormatter.Format(language)} was not found.");
    }

    private T CompilationFailResponse<T>(ulong operationId, IReadOnlyList<Diagnostic> diagnostics) where T : IMessageProperties, new()
    {
        var compilationFormatResult = compilationFormatter.CompilationResponse(operationId, false, diagnostics);

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
        return Error<T>("Decompiler not found", $"The decompiler for the language {nameFormatter.Format(language)} was not found.");
    }

    private T DecompilationFailResponse<T>(Language language) where T : IMessageProperties, new()
    {
        return Error<T>("Decompilation failed", $"The decompilation for the language {nameFormatter.Format(language)} failed.");
    }

    public T LanguageNotFoundResponse<T>(ulong operationId, string? language) where T : IMessageProperties, new()
    {
        return Error<T>("Language not found", $"The language {language} was not found.");
    }

    public T UnknownError<T>(string reason) where T : IMessageProperties, new()
    {
        return Error<T>("An error occurred", reason);
    }

    public T DecompilationResponse<T>(ulong operationId, Language language, string decompiledCode, List<Diagnostic> diagnostics) where T : IMessageProperties, new()
    {
        T message = new();

        var compilationFormatResult = compilationFormatter.CompilationResponse(operationId, true, diagnostics);

        message.Embeds = [compilationFormatResult.Embed];
        message.Components = compilationFormatResult.Components;

        message.AddAttachments(new AttachmentProperties($"source.{languageFormatProvider.GetFormat(language)}", new MemoryStream(Encoding.UTF8.GetBytes(decompiledCode))));

        return message;
    }

    public T RunResponse<T>(ulong operationId, Language language, string output, List<Diagnostic> diagnostics) where T : IMessageProperties, new()
    {
        T message = new();

        var compilationFormatResult = compilationFormatter.CompilationResponse(operationId, true, diagnostics);

        message.Embeds = [compilationFormatResult.Embed];
        message.Components = compilationFormatResult.Components;

        message.AddAttachments(new AttachmentProperties($"output.txt", new MemoryStream(Encoding.UTF8.GetBytes(output))));

        return message;
    }

    public T RateLimitResponse<T>(ulong operationId) where T : IMessageProperties, new()
    {
        return Error<T>("Rate limit exceeded", "You have exceeded the rate limit. Please try again later.");
    }

    public T AttachmentCodeResultResponse<T>(ulong operationId, AttachmentCodeResult result) where T : IMessageProperties, new()
    {
        return result switch
        {
            AttachmentCodeResult.Success => AttachmentCodeSuccessResponse<T>(),
            AttachmentCodeResult.CodeNotFound => AttachmentCodeNotFoundResponse<T>(),
            _ => throw new ArgumentOutOfRangeException(nameof(result)),
        };
    }

    private T AttachmentCodeSuccessResponse<T>() where T : IMessageProperties, new()
    {
        T message = new();

        var optionsValue = options.Value;

        message.AddEmbeds(new EmbedProperties().WithTitle($"{optionsValue.Emojis.Success} Code found successfully")
                                               .WithColor(new(optionsValue.PrimaryColor)));

        return message;
    }

    private T AttachmentCodeNotFoundResponse<T>() where T : IMessageProperties, new()
    {
        return Error<T>("Code not found", "No code was provided.");
    }

    public async ValueTask<T> HelpResponseAsync<T>(ulong operationId) where T : IMessageProperties, new()
    {
        T message = new();

        var optionsValue = options.Value;
        var emojis = optionsValue.Emojis;
        var information = optionsValue.Information;

        var defaultArchitectureFormatted = nameFormatter.Format(optionsValue.Backend.DefaultArchitecture);

        var architectures = await backendUriProvider.GetPlatformsAsync();

        var discordOptionsValue = discordOptions.Value;

        var prefix = discordOptionsValue.Prefixes is { Count: > 0 } prefixes ? prefixes[0] : discordOptionsValue.Prefix;

        message.AddEmbeds(new EmbedProperties().WithDescription(
                                                $"""
                                                # {emojis.Help} Help
                                                {information.Description}
                                                ## {emojis.Link} Links
                                                - [Invitation Link]({information.InvitationLink})
                                                - [GitHub Repository]({information.GitHubRepository})
                                                - [Support Discord]({information.SupportDiscord})
                                                - [Terms of Service]({information.TermsOfService})
                                                - [Privacy Policy]({information.PrivacyPolicy})
                                                ## {emojis.Command} Commands
                                                - `{prefix}run <architecture?> <code>` — Runs the provided code, using {defaultArchitectureFormatted} architecture by default. 
                                                  - **Example**:  
                                                    {prefix}run  
                                                    \```c#  
                                                    Console.Write("Hello, World!");  
                                                    \```
                                                  - **Output**:
                                                    ```
                                                    Hello, World!
                                                    ```
                                                - `{prefix}<language> <code>` — Decompiles the provided code to the specified language.
                                                  - **Example**:  
                                                    {prefix}c#  
                                                    \```f#  
                                                    printf "Hello, World!"  
                                                    \```
                                                - `{prefix}<architecture> <code>` — Shows the architecture-specific JIT disassembly of the provided code.
                                                  - **Example**:  
                                                    {prefix}{defaultArchitectureFormatted.ToLowerInvariant()}  
                                                    \```c#  
                                                    Console.Write("Hello, World!");  
                                                    \```
                                                The code can be provided as is, as a code block or as an attachment.
                                                ## {emojis.Support} Support
                                                - **Compilation**: {string.Join(", ", compilerProvider.SupportedLanguages.Select(l => $"**{nameFormatter.Format(l)}**"))}
                                                - **Decompilation**: {string.Join(", ", decompilerProvider.SupportedLanguages.Where(l => l <= Language.IL).Select(l => $"**{nameFormatter.Format(l)}**"))}
                                                - **Architectures**: {string.Join(", ", architectures.Select(a => $"**{nameFormatter.Format((BackendArchitecture)a)}**"))}
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
