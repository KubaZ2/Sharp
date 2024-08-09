﻿using Microsoft.Extensions.DependencyInjection;
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
using Sharp.Backend;
using Sharp.Compilation;
using Sharp.Decompilation;
using Sharp.Diagnostics;
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
    .AddSingleton<ILanguageFormatProvider, LanguageFormatProvider>()
    .AddSingleton<IDiagnosticsFormatter, DiagnosticsFormatter>()
    .AddSingleton<IResponseProvider, ResponseProvider>()
    .AddSingleton<IBackendUriProvider, BackendUriProvider>()
    .AddSingleton<IBackendProvider, BackendProvider>()
    .AddSingleton<ICompiler, CSharpCompiler>()
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
    .AddCommands<CommandContext>(o => o.ResultHandler = new EnhancedCommandServiceResultHandler<CommandContext>())
    .AddApplicationCommands<SlashCommandInteraction, SlashCommandContext>(o => o.ResultHandler = new EnhancedApplicationCommandServiceResultHandler<SlashCommandContext>())
    .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>(o => o.ResultHandler = new EnhancedComponentInteractionServiceResultHandler<ButtonInteractionContext>())
    .AddDiscordGateway(o =>
    {
        o.Configuration = new()
        {
            Intents = GatewayIntents.GuildMessages | GatewayIntents.MessageContent,
        };
    });

var host = builder.Build();

host.AddModules(typeof(Program).Assembly)
    .AddDecompileCommands()
    .UseGatewayEventHandlers();

await host.RunAsync();
