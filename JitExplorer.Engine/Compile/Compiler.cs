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
    public class ParsedTree
    {
        public ParsedTree(SyntaxTree syntaxTree, EmbeddedText embeddedText)
        {
            this.SyntaxTree = syntaxTree;
            this.EmbeddedText = embeddedText;
        }

        public SyntaxTree SyntaxTree { get; }

        public EmbeddedText EmbeddedText { get; }
    }

    public class Compiler
    {
        private readonly CompilerOptions compilerOptions;

        private static readonly MemoryStream Empty = new MemoryStream(Array.Empty<byte>());

        public Compiler(CompilerOptions compilerOptions)
        {
            this.compilerOptions = compilerOptions;
        }

        // https://stackoverflow.com/questions/50649795/how-to-debug-dll-generated-from-roslyn-compilation
        public Compilation Compile(string assemblyName, params ParsedTree[] parsedTrees)
        {
            var peStream = new MemoryStream();
            var pdbStream = new MemoryStream();

            var compilation = CsCompile(assemblyName, parsedTrees.Select(t => t.SyntaxTree));

            var emitOptions = new EmitOptions(
                debugInformationFormat: DebugInformationFormat.Pdb,
                pdbFilePath: Path.ChangeExtension(assemblyName, ".pdb")
            );

            var result = compilation.Emit(
                peStream,
                pdbStream,
                embeddedTexts: parsedTrees.Select(t => t.EmbeddedText),
                options: emitOptions);

            if (result.Success)
            {
                peStream.Position = pdbStream.Position = 0;

                return new Compilation(peStream, pdbStream, Array.Empty<CompileDiagnostic>());
            }

            var errors = result.Diagnostics
                .Where(x =>
                    x.IsWarningAsError ||
                    x.Severity == DiagnosticSeverity.Error)
                .OrderBy(x => x.Location.SourceSpan.Start)
                .Select(x => new CompileDiagnostic(x))
                .ToArray();

            return new Compilation(Empty, Empty, errors);
        }

        public ParsedTree Parse(string sourceCodePath, string sourceCode)
        {
            var buffer = Encoding.UTF8.GetBytes(sourceCode);
            var sourceText = SourceText.From(buffer, buffer.Length, Encoding.UTF8, canBeEmbedded: true);

            var options = CSharpParseOptions.Default.WithLanguageVersion(this.compilerOptions.LanguageVersion);
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, options, path: sourceCodePath);

            var syntaxRootNode = syntaxTree.GetRoot() as CSharpSyntaxNode;

            return new ParsedTree(
                CSharpSyntaxTree.Create(syntaxRootNode, null, sourceCodePath, Encoding.UTF8), 
                EmbeddedText.FromSource(sourceCodePath, sourceText));
        }

        private CSharpCompilation CsCompile(string assemblyName, IEnumerable<SyntaxTree> syntaxTrees)
        {
            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees,
                options: new CSharpCompilationOptions(this.compilerOptions.OutputKind)
                    .WithOptimizationLevel(this.compilerOptions.OptimizationLevel)
                    .WithPlatform(this.compilerOptions.Platform)
                    .WithAllowUnsafe(this.compilerOptions.AllowUnsafe));

            compilation = compilation.AddReferences(MetadataReferences);

            return compilation;
        }

        public static MetadataReference[] MetadataReferences { get; } = EnumMetadataReferences().ToArray();

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

