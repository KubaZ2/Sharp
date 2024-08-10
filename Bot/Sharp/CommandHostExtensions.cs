using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Hosting;

using NetCord;

using NetCord.Hosting.Services.Commands;
using NetCord.Rest;
using NetCord.Services.Commands;

using Sharp.Compilation;

using Sharp.Decompilation;
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
            }, priority: 1);

            host.AddCommand<CommandContext>(aliases, (IServiceProvider services, CommandContext context, [CommandParameter(Remainder = true)] string code) =>
            {
                return HandleAsync(null, code, context, services, language);
            }, priority: 0);
        }

        return host;
    }

    private static async Task<ReplyMessageProperties> HandleAsync(string? languageInput, string code, CommandContext context, IServiceProvider services, Language targetLanguage)
    {
        var typingTask = context.Client.Rest.EnterTypingStateAsync(context.Message.ChannelId);

        try
        {
            return await HandleAsyncCore(languageInput, code, context, services, targetLanguage);
        }
        finally
        {
            (await typingTask).Dispose();
        }
    }

    private static async Task<ReplyMessageProperties> HandleAsyncCore(string? languageInput, string code, CommandContext context, IServiceProvider services, Language targetLanguage)
    {
        var sourceLanguage = services.GetRequiredService<ILanguageMatcher>().Match(languageInput);

        var responseProvider = services.GetRequiredService<IResponseProvider>();

        if (!sourceLanguage.HasValue)
            return responseProvider.LanguageNotFoundResponse<ReplyMessageProperties>(context.Message.Id, languageInput);

        var compilationResult = await services.GetRequiredService<ICompilationProvider>().CompileAsync(sourceLanguage.GetValueOrDefault(), code, null);

        if (compilationResult is not CompilationResult.Success { Assembly: var assembly, Diagnostics: var diagnostics })
            return responseProvider.CompilationResultResponse<ReplyMessageProperties>(context.Message.Id, compilationResult);

        var decompilationResult = await services.GetRequiredService<IDecompilationProvider>().DecompileAsync(assembly, targetLanguage);

        if (decompilationResult is not DecompilationResult.Success { Code: var decompiledCode })
            return responseProvider.DecompilationResultResponse<ReplyMessageProperties>(context.Message.Id, decompilationResult);

        return responseProvider.DecompilationResponse<ReplyMessageProperties>(context.Message.Id, targetLanguage, decompiledCode, diagnostics);
    }
}
