﻿using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

using Microsoft.Extensions.Options;

using InnerDecompiler = ICSharpCode.Decompiler.CSharp.CSharpDecompiler;

namespace Sharp.Decompilation;

public class CSharpDecompiler : IDecompiler
{
    public CSharpDecompiler(IOptions<Options> options)
    {
        var formattingOptions = FormattingOptionsFactory.CreateAllman();
        formattingOptions.IndentationString = options.Value.Formatting.Indentation;
        _formattingOptions = formattingOptions;
    }

    private readonly CSharpFormattingOptions _formattingOptions;

    public Language Language => Language.CSharp;

    private static readonly DecompilerSettings _settings = new(LanguageVersion.CSharp1)
    {
        ArrayInitializers = false,
        AutomaticEvents = false,
        DecimalConstants = false,
        DoWhileStatement = false,
        FixedBuffers = false,
        ForEachStatement = false,
        ForStatement = false,
        FunctionPointers = true,
        LockStatement = false,
        SparseIntegerSwitch = false,
        StringConcat = false,
        SwitchStatementOnString = false,
        UseRefLocalsForAccurateOrderOfEvaluation = true,
        UsingStatement = false,
        FileScopedNamespaces = true,
    };

    public ValueTask<bool> DecompileAsync(ulong operationId, Stream assembly, TextWriter writer)
    {
        using PEFile peFile = new(string.Empty, new PEReader(assembly), MetadataReaderOptions.Default);

        UniversalAssemblyResolver assemblyResolver = new(null, false, peFile.DetectTargetFrameworkId(), peFile.DetectRuntimePack());

        DecompilerTypeSystem typeSystem = new(peFile, assemblyResolver);

        InnerDecompiler decompiler = new(typeSystem, _settings);

        var syntaxTree = decompiler.DecompileWholeModuleAsSingleFile();

        syntaxTree.AcceptVisitor(new CSharpOutputVisitor(writer, _formattingOptions));
        return new(true);
    }
}
