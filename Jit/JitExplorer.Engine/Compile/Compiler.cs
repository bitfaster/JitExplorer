using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JitExplorer.Engine.Compile
{
    public class CompilerOptions
    {
        public CompilerOptions()
        {
            this.OptimizationLevel = OptimizationLevel.Release;
            this.LanguageVersion = LanguageVersion.Default;
            this.OutputKind = OutputKind.DynamicallyLinkedLibrary;
            this.Platform = Platform.AnyCpu;
            this.AllowUnsafe = false;
        }

        public OptimizationLevel OptimizationLevel { get; set; }

        // Does this control framework version?
        public LanguageVersion LanguageVersion { get; set; }

        public OutputKind OutputKind { get; set; }

        public Platform Platform { get; set; }

        public bool AllowUnsafe { get; set; }
    }

    public class Compiler
    {
        private readonly CompilerOptions compilerOptions;

        public Compiler(CompilerOptions compilerOptions)
        {
            this.compilerOptions = compilerOptions;
        }

        public Compilation Compile(string assemblyName, string sourceCodePath, string sourceCode)
        {
            var peStream = new MemoryStream();

            var symbolsName = Path.ChangeExtension(assemblyName, ".pdb");

            var buffer = Encoding.UTF8.GetBytes(sourceCode);
            var sourceText = SourceText.From(buffer, buffer.Length, Encoding.UTF8, canBeEmbedded: true);

            var compilation = Compile(assemblyName, sourceText, sourceCodePath);

            var emitOptions = new EmitOptions(
                //debugInformationFormat: DebugInformationFormat.Embedded,
                //pdbFilePath: symbolsName
            );

            var result = compilation.Emit(
                peStream,
                //embeddedTexts: new[] { EmbeddedText.FromSource(sourceCodePath, sourceText) },
                options: emitOptions);

            if (result.Success)
            {
                peStream.Seek(0, SeekOrigin.Begin);

                return new Compilation(peStream, Array.Empty<Message>());
            }
            else
            {
                var messages = result.Diagnostics
                    .Where(x =>
                        x.IsWarningAsError ||
                        x.Severity == DiagnosticSeverity.Error)
                    .OrderBy(x => x.Location.SourceSpan.Start);

                return new Compilation(new MemoryStream(0),
                    messages.Select(x =>
                    {
                        var lineSpan = x.Location.GetMappedLineSpan().Span;

                        var startLine = lineSpan.Start.Line;
                        var startCharacter = lineSpan.Start.Character;
                        var endLine = lineSpan.End.Line;
                        var endCharacter = lineSpan.End.Character;
                        var severity = x.Severity.ToString().ToLower();

                        return new Message(
                            startLine, startCharacter,
                            endLine, endCharacter,
                            severity, x.Id, x.GetMessage()
                        );
                    }).ToArray());
            }
        }

        private CSharpCompilation Compile(string assemblyName, SourceText sourceText, string sourceCodePath)
        {
            var options = CSharpParseOptions.Default.WithLanguageVersion(this.compilerOptions.LanguageVersion);
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, options, path: sourceCodePath);

            var syntaxRootNode = syntaxTree.GetRoot() as CSharpSyntaxNode;
            var encoded = CSharpSyntaxTree.Create(syntaxRootNode, null, sourceCodePath, Encoding.UTF8);

            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { encoded },
                options: new CSharpCompilationOptions(this.compilerOptions.OutputKind)
                    .WithOptimizationLevel(this.compilerOptions.OptimizationLevel)
                    .WithPlatform(this.compilerOptions.Platform)
                    .WithAllowUnsafe(this.compilerOptions.AllowUnsafe));

            compilation = compilation.AddReferences(MetadataReferences);

            return compilation;
        }

        public MetadataReference[] MetadataReferences { get; } = EnumMetadataReferences().ToArray();

        private static IEnumerable<MetadataReference> EnumMetadataReferences()
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            foreach (var x in Directory.EnumerateFiles(assemblyPath, "*.dll"))
            {
                var fileName = Path.GetFileName(x);

                if (fileName.IndexOf("Native", StringComparison.Ordinal) == -1 &&
                    (fileName.StartsWith("System.") || fileName.StartsWith("Microsoft.")))
                    yield return MetadataReference.CreateFromFile(x);
            }
        }
    }
}

