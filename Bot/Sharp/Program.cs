using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.Commands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.Commands;
using NetCord.Services.ComponentInteractions;

using Sharp;
using Sharp.Attachments;
using Sharp.Names;
using Sharp.Backend;
using Sharp.Compilation;
using Sharp.CompilationResponse;
using Sharp.Decompilation;
using Sharp.Diagnostics;
using Sharp.RateLimits;
using Sharp.Responding;
using Sharp.ResultHandlers;

var builder = Host.CreateApplicationBuilder(args);

var services = builder.Services;

services
    .AddOptions<Options>()
    .BindConfiguration(nameof(Options))
    .ValidateDataAnnotations();

services
    .AddHttpClient()
    .AddMemoryCache()
    .AddSingleton<INameFormatter, NameFormatter>()
    .AddSingleton<IAttachmentCodeProvider, AttachmentCodeProvider>()
    .AddSingleton<ILanguageFormatProvider, LanguageFormatProvider>()
    .AddSingleton<IDiagnosticsFormatter, DiagnosticsFormatter>()
    .AddSingleton<ICompilationFormatter, CompilationFormatter>()
    .AddSingleton<IResponseProvider, ResponseProvider>()
    .AddSingleton<IBackendUriProvider, BackendUriProvider>()
    .AddSingleton<IBackendProvider, BackendProvider>()
    .AddSingleton<ICompiler, CSharpCompiler>()
    .AddSingleton<ICompiler, VisualBasicCompiler>()
    .AddSingleton<ICompiler, FSharpCompiler>()
    .AddSingleton<ICompiler, ILCompiler>()
    .AddSingleton<ICompilerProvider, CompilerProvider>()
    .AddSingleton<IDecompiler, CSharpDecompiler>()
    .AddSingleton<IDecompiler, ILDecompiler>()
    .AddSingleton<IDecompiler, X64Decompiler>()
    .AddSingleton<IDecompiler, X86Decompiler>()
    .AddSingleton<IDecompiler, Arm64Decompiler>()
    .AddSingleton<IDecompiler, Arm32Decompiler>()
    .AddSingleton<IDecompilerProvider, DecompilerProvider>()
    .AddSingleton<ILanguageMatcher, LanguageMatcher>()
    .AddSingleton<ICompilationProvider, CompilationProvider>()
    .AddSingleton<IDecompilationProvider, DecompilationProvider>()
    .AddSingleton<Sharp.RateLimits.IRateLimiter, RateLimiter>()
    .AddCommands<CommandContext>(o => o.ResultHandler = new EnhancedCommandServiceResultHandler<CommandContext>())
    .AddApplicationCommands<SlashCommandInteraction, SlashCommandContext>(o => o.ResultHandler = new EnhancedApplicationCommandServiceResultHandler<SlashCommandContext>())
    .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>(o => o.ResultHandler = new EnhancedComponentInteractionServiceResultHandler<ButtonInteractionContext>())
    .AddDiscordGateway(o =>
    {
        o.Intents = GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.MessageContent;
    });

var host = builder.Build();

host.AddModules(typeof(Program).Assembly)
    .AddDecompileCommands()
    .AddHelpCommands()
    .UseGatewayEventHandlers();

await host.RunAsync();
