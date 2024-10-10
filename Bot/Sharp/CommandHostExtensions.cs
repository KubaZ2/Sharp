using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetCord;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.Commands;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.Commands;

using Sharp.Attachments;
using Sharp.Compilation;
using Sharp.Decompilation;
using Sharp.RateLimits;
using Sharp.Responding;

namespace Sharp;

public static class CommandHostExtensions
{
    public static IHost AddDecompileCommands(this IHost host)
    {
        foreach (var (language, aliases) in typeof(Language).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                                            .Select(f => ((Language)f.GetRawConstantValue()!, f.GetCustomAttribute<LanguageAliasesAttribute>()!.Aliases)))
        {
            host.AddCommand<CommandContext>(aliases, (IServiceProvider services, CommandContext context, [CommandParameter(Remainder = true)] CodeBlock codeBlock) =>
            {
                return HandleAsync(codeBlock.Formatter, codeBlock.Code, context, services, language);
            }, priority: 2);

            host.AddCommand<CommandContext>(aliases, (IServiceProvider services, CommandContext context, [CommandParameter(Remainder = true)] string code) =>
            {
                return HandleAsync(null, code, context, services, language);
            }, priority: 1);

            host.AddCommand<CommandContext>(aliases, async (IServiceProvider services, IAttachmentCodeProvider attachmentCodeProvider, CommandContext context) =>
            {
                var result = await attachmentCodeProvider.GetCodeAsync(context.Message.Attachments);

                if (result is not AttachmentCodeResult.Success { Language: var sourceLanguage, Code: var code })
                    return services.GetRequiredService<IResponseProvider>().AttachmentCodeResultResponse<ReplyMessageProperties>(context.Message.Id, result);

                return await HandleAsync(sourceLanguage, code, context, services, language);
            }, priority: 0);
        }

        return host;
    }

    private static async Task<ReplyMessageProperties> HandleAsync(string? sourceLanguageInput, string code, CommandContext context, IServiceProvider services, Language targetLanguage)
    {
        var responseProvider = services.GetRequiredService<IResponseProvider>();

        var sourceLanguage = services.GetRequiredService<ILanguageMatcher>().Match(sourceLanguageInput);

        if (!sourceLanguage.HasValue)
            return responseProvider.LanguageNotFoundResponse<ReplyMessageProperties>(context.Message.Id, sourceLanguageInput);

        var rateLimitProvider = services.GetRequiredService<IRateLimiter>();

        if (!await rateLimitProvider.TryAcquireAsync(context.User.Id))
            return responseProvider.RateLimitResponse<ReplyMessageProperties>(context.Message.Id);

        var typingTask = context.Client.Rest.EnterTypingStateAsync(context.Message.ChannelId);

        try
        {
            return await HandleAsyncCore(sourceLanguage, code, context, services, responseProvider, targetLanguage);
        }
        finally
        {
            (await typingTask).Dispose();
        }
    }

    private static async Task<ReplyMessageProperties> HandleAsyncCore(Language? sourceLanguage, string code, CommandContext context, IServiceProvider services, IResponseProvider responseProvider, Language targetLanguage)
    {
        var operationId = context.Message.Id;
        var compilationResult = await services.GetRequiredService<ICompilationProvider>().CompileAsync(operationId, sourceLanguage.GetValueOrDefault(), code, null);

        if (compilationResult is not CompilationResult.Success { Assembly: var assembly, Diagnostics: var diagnostics })
            return responseProvider.CompilationResultResponse<ReplyMessageProperties>(operationId, compilationResult);

        var decompilationResult = await services.GetRequiredService<IDecompilationProvider>().DecompileAsync(operationId, assembly, targetLanguage);

        if (decompilationResult is not DecompilationResult.Success { Code: var decompiledCode })
            return responseProvider.DecompilationResultResponse<ReplyMessageProperties>(operationId, decompilationResult);

        return responseProvider.DecompilationResponse<ReplyMessageProperties>(operationId, targetLanguage, decompiledCode, diagnostics);
    }

    public static IHost AddHelpCommands(this IHost host)
    {
        host.AddCommand<CommandContext>(["help"], (IResponseProvider responseProvider, CommandContext context) =>
        {
            return responseProvider.HelpResponseAsync<ReplyMessageProperties>(context.Message.Id).AsTask();
        });

        host.AddSlashCommand<SlashCommandContext>("help", "Shows how to use the bot", (IResponseProvider responseProvider, SlashCommandContext context) =>
        {
            return responseProvider.HelpResponseAsync<InteractionMessageProperties>(context.Interaction.Id).AsTask();
        });

        return host;
    }
}
