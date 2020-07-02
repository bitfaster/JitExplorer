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
    public class Compiler
    {
        private readonly CompilerOptions compilerOptions;

        public Compiler(CompilerOptions compilerOptions)
        {
            this.compilerOptions = compilerOptions;
        }

        public Compilation Compile(string assemblyName, params SyntaxTree[] syntaxTrees)
        {
            var peStream = new MemoryStream();
            var compilation = CsCompile(assemblyName, syntaxTrees);

            var result = compilation.Emit(
                peStream,
                options: new EmitOptions());

            if (result.Success)
            {
                peStream.Seek(0, SeekOrigin.Begin);

                return new Compilation(peStream, Array.Empty<CompileDiagnostic>());
            }

            var errors = result.Diagnostics
                .Where(x =>
                    x.IsWarningAsError ||
                    x.Severity == DiagnosticSeverity.Error)
                .OrderBy(x => x.Location.SourceSpan.Start)
                .Select(x => new CompileDiagnostic(x))
                .ToArray();

            return new Compilation(new MemoryStream(0), errors);
        }

        public SyntaxTree CreateSyntaxTree(string sourceCodePath, string sourceCode)
        {
            var buffer = Encoding.UTF8.GetBytes(sourceCode);
            var sourceText = SourceText.From(buffer, buffer.Length, Encoding.UTF8, canBeEmbedded: true);

            var options = CSharpParseOptions.Default.WithLanguageVersion(this.compilerOptions.LanguageVersion);
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, options, path: sourceCodePath);

            var syntaxRootNode = syntaxTree.GetRoot() as CSharpSyntaxNode;
            return CSharpSyntaxTree.Create(syntaxRootNode, null, sourceCodePath, Encoding.UTF8);
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

