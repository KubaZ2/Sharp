using System.Runtime.InteropServices;

using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Commands;

using Sharp.Backend;
using Sharp.Compilation;
using Sharp.RateLimits;
using Sharp.Responding;

namespace Sharp.Modules;

public class RunCommand(ILanguageMatcher languageMatcher, ICompilationProvider compilationProvider, IResponseProvider responseProvider, IBackendProvider backendProvider, IOptions<Options> options, IRateLimiter rateLimitProvider) : CommandModule<CommandContext>
{
    [Command("run", Priority = 3)]
    public Task<ReplyMessageProperties> RunAsync(BackendArchitecture architecture, [CommandParameter(Remainder = true)] CodeBlock codeBlock)
    {
        return RunAsync(codeBlock.Formatter, codeBlock.Code, architecture);
    }

    [Command("run", Priority = 2)]
    public Task<ReplyMessageProperties> RunAsync(BackendArchitecture architecture, [CommandParameter(Remainder = true)] string code)
    {
        return RunAsync(null, code, architecture);
    }

    [Command("run", Priority = 1)]
    public Task<ReplyMessageProperties> RunAsync([CommandParameter(Remainder = true)] CodeBlock codeBlock)
    {
        return RunAsync(codeBlock.Formatter, codeBlock.Code, options.Value.Backend.DefaultArchitecture);
    }

    [Command("run", Priority = 0)]
    public Task<ReplyMessageProperties> RunAsync([CommandParameter(Remainder = true)] string code)
    {
        return RunAsync(null, code, options.Value.Backend.DefaultArchitecture);
    }

    private async Task<ReplyMessageProperties> RunAsync(string? sourceLanguageInput, string code, BackendArchitecture architecture)
    {
        var sourceLanguage = languageMatcher.Match(sourceLanguageInput);

        if (!sourceLanguage.HasValue)
            return responseProvider.LanguageNotFoundResponse<ReplyMessageProperties>(Context.Message.Id, sourceLanguageInput);

        if (!await rateLimitProvider.TryAcquireAsync(Context.User.Id))
            return responseProvider.RateLimitResponse<ReplyMessageProperties>(Context.Message.Id);

        var typingTask = Context.Client.Rest.EnterTypingStateAsync(Context.Message.ChannelId);

        try
        {
            return await RunAsyncCore(sourceLanguage, code, architecture);
        }
        finally
        {
            (await typingTask).Dispose();
        }
    }

    private async Task<ReplyMessageProperties> RunAsyncCore(Language? language, string code, BackendArchitecture architecture)
    {
        var compilationResult = await compilationProvider.CompileAsync(language.GetValueOrDefault(), code, CompilationOutput.Executable);

        if (compilationResult is not CompilationResult.Success { Assembly: var assembly, Diagnostics: var diagnostics })
            return responseProvider.CompilationResultResponse<ReplyMessageProperties>(Context.Message.Id, compilationResult);

        var output = await backendProvider.RunAsync((Architecture)architecture, assembly);

        return responseProvider.RunResponse<ReplyMessageProperties>(Context.Message.Id, language.GetValueOrDefault(), output, diagnostics);
    }
}
