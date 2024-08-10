using System.Runtime.InteropServices;

using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Commands;

using Sharp.Backend;
using Sharp.Compilation;
using Sharp.Responding;

namespace Sharp.Modules;

public class RunCommand(ILanguageMatcher languageMatcher, ICompilationProvider compilationProvider, IResponseProvider responseProvider, IBackendProvider backendProvider, IOptions<Options> options) : CommandModule<CommandContext>
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
        return RunAsync(codeBlock.Formatter, codeBlock.Code, options.Value.DefaultArchitecture);
    }

    [Command("run", Priority = 0)]
    public Task<ReplyMessageProperties> RunAsync([CommandParameter(Remainder = true)] string code)
    {
        return RunAsync(null, code, options.Value.DefaultArchitecture);
    }

    private async Task<ReplyMessageProperties> RunAsync(string? languageInput, string code, BackendArchitecture architecture)
    {
        var typingTask = Context.Client.Rest.EnterTypingStateAsync(Context.Message.ChannelId);

        try
        {
            return await RunAsyncCore(languageInput, code, architecture);
        }
        finally
        {
            (await typingTask).Dispose();
        }
    }

    private async Task<ReplyMessageProperties> RunAsyncCore(string? languageInput, string code, BackendArchitecture architecture)
    {
        var language = languageMatcher.Match(languageInput);

        if (!language.HasValue)
            return responseProvider.LanguageNotFoundResponse<ReplyMessageProperties>(Context.Message.Id, languageInput);

        var compilationResult = await compilationProvider.CompileAsync(language.GetValueOrDefault(), code, CompilationOutput.Executable);

        if (compilationResult is not CompilationResult.Success { Assembly: var assembly, Diagnostics: var diagnostics })
            return responseProvider.CompilationResultResponse<ReplyMessageProperties>(Context.Message.Id, compilationResult);

        var output = await backendProvider.RunAsync((Architecture)architecture, assembly);

        return responseProvider.RunResponse<ReplyMessageProperties>(Context.Message.Id, language.GetValueOrDefault(), output, diagnostics);
    }
}
