using BitFaster.Caching.Lru;
using JitExplorer.Engine.CodeAnlaysis;
using JitExplorer.Engine.Compile;
using JitExplorer.Engine.Disassemble;
using JitExplorer.Engine.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JitExplorer.Engine
{
    public class RuntimeDisassembler
    {
        public const string AttributeName = "Jit.This";

        public event EventHandler<ProgressEventArgs> Progress;

        private const string directory = "Workspace";
        private readonly string exeName;
        private readonly string csFileName;
        private static ClassicLru<string, SourceCodeProvider> sourceCodeProviders = new ClassicLru<string, SourceCodeProvider>(10);

        public RuntimeDisassembler(string exeName)
        {
            ValidateExeName(exeName);
            this.exeName = exeName;
            this.csFileName = "program.cs";
        }

        public Dissassembly CompileJitAndDisassemble(string sourceCode, Config config)
        {
            this.Progress?.Invoke(this, new ProgressEventArgs() { StatusMessage = "Compiling..." });
            (ExtractMarkedMethod userMethod, Compile.Compilation compilation) = Compile(this.exeName, sourceCode, config);
            using (compilation)
            {
                if (!compilation.Succeeded || !userMethod.Success)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var e in compilation.Diagnostics)
                    {
                        sb.AppendLine(e.ToString());
                    }

                    foreach (var e in userMethod.Errors)
                    {
                        sb.AppendLine(e.ToString());
                    }

                    return new Dissassembly(sb.ToString());
                }

                this.Progress?.Invoke(this, new ProgressEventArgs() { StatusMessage = "Writing to disk..." });
                MkDir();
                WriteSourceToDisk(sourceCode);
                WriteExeToDisk(this.exeName, compilation);

                this.Progress?.Invoke(this, new ProgressEventArgs() { StatusMessage = "Jitting..." });
                using (var p = Execute(this.exeName, config))
                {
                    using var cpipe = new System.IO.Pipes.NamedPipeClientStream(".", "JitExplorer.Pipe", System.IO.Pipes.PipeDirection.InOut, System.IO.Pipes.PipeOptions.None);

                    try
                    {
                        cpipe.Connect(3000);
                    }
                    catch
                    {
                        p.Kill();
                        throw;
                    }

                    this.Progress?.Invoke(this, new ProgressEventArgs() { StatusMessage = "Dissassembling..." });

                    var result = AttachAndDecompile(p.Id, userMethod.Method.Type.QualifedName, userMethod.Method.Name, sourceCode);

                    // signal dissassemble complete
                    cpipe.WriteByte(1);

                    if (!p.WaitForExit(1000))
                    {
                        p.Kill();
                    }

                    return FormatResult(result);
                }
            }
        }

        private static string GetSourceCode(string methodCallSite)
        {
            // this is more compact than the roslyn equiv:
            // http://roslynquoter.azurewebsites.net/
            var jitExplSource = @"namespace JitExplorer 
{ 
    public static class Signal 
    { 
        public static void __Jit() 
        { 
            using (var sPipe = new System.IO.Pipes.NamedPipeServerStream(""JitExplorer.Pipe"", System.IO.Pipes.PipeDirection.InOut))
            {
                sPipe.WaitForConnection();
                sPipe.ReadByte(); // wait for signal that code is dissassembled
            }
        } 
    } 
}
namespace Jit
{
    using System;
    public class This : Attribute {}


    public class Program
    {
        public static void Main(string[] args)
        {
             methodCallSite;
             JitExplorer.Signal.__Jit();
        }
    }
}
";
            return jitExplSource.Replace("methodCallSite", methodCallSite);
        }

        private (ExtractMarkedMethod, Compile.Compilation) Compile(string assemblyName, string source, Config config)
        {
            if (config.CompilerOptions.OutputKind != Microsoft.CodeAnalysis.OutputKind.ConsoleApplication)
            {
                throw new ArgumentOutOfRangeException("OutputKind must be ConsoleApplication");
            }

            // DebuggableAttribute:
            // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.debuggableattribute.-ctor?view=netcore-3.1#System_Diagnostics_DebuggableAttribute__ctor_System_Diagnostics_DebuggableAttribute_DebuggingModes_

            // This seems to result in DotPeek reporting assembly framework as .NET core instead of 4.8.
            // But it still shows the .dll as debug, even with correct DebuggableAttribute.
            //            var assemblyInfo = @"
            //using System.Diagnostics;
            //using System.Reflection;
            //using System.Runtime.CompilerServices;
            //using System.Runtime.Versioning;

            //[assembly: CompilationRelaxations(8)]
            //[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
            //[assembly: Debuggable(false, false)]
            //[assembly: TargetFramework("".NETCoreApp,Version=v3.1"", FrameworkDisplayName = """")]
            //[assembly: AssemblyCompany(""Test"")]
            //[assembly: AssemblyConfiguration(""Release"")]
            //[assembly: AssemblyFileVersion(""1.0.0.0"")]
            //[assembly: AssemblyInformationalVersion(""1.0.0"")]
            //[assembly: AssemblyProduct(""Test"")]
            //[assembly: AssemblyTitle(""Test"")]
            //[assembly: AssemblyVersion(""1.0.0.0"")]";

            Compiler c = new Compiler(config.CompilerOptions);
            var syntax = c.Parse(this.csFileName, source);
            var methodExtractor = new ExtractMarkedMethod(AttributeName);
            var node = syntax.SyntaxTree.GetRoot();
            methodExtractor.Visit(node);

            // don't ever inline the entry point
            var rewrite = new AttributeStatementRewriter(AttributeName);
            var rewritten = rewrite.Visit(syntax.SyntaxTree.GetRoot()) as CSharpSyntaxNode;

            // preserve source path after re-write, else cannot make source links
            // TODO: preserve trivia - this is why we end up with empty ; lines in the ASM
            var rewrittenWithPath = CSharpSyntaxTree.Create(rewritten, null, syntax.SyntaxTree.FilePath, syntax.SyntaxTree.Encoding, syntax.SyntaxTree.DiagnosticOptions);
            syntax = new ParsedTree(rewrittenWithPath, syntax.EmbeddedText);

            if (methodExtractor.Success)
            {
                var jitSyntax = c.Parse("jitexpl.cs", GetSourceCode(methodExtractor.Method.ToCallSite()));
                // var assSyntax = c.CreateSyntaxTree("assemblyinfo.cs", assemblyInfo);

                return (methodExtractor, c.Compile(assemblyName, syntax, jitSyntax));
            }

            return (methodExtractor, new Compile.Compilation(new MemoryStream(), new MemoryStream(), Array.Empty<CompileDiagnostic>()));
        }

        private const int maxRetryAttempts = 3;

        private static readonly Task CompletedTask = Task.FromResult(false);

        private AsyncRetryPolicy retryPolicy = Policy
                .Handle<IOException>()
                .WaitAndRetryAsync(maxRetryAttempts, i => TimeSpan.FromMilliseconds(50));

        private void WriteSourceToDisk(string source)
        {
            this.retryPolicy.ExecuteAsync(() => { File.WriteAllText(WkPath(this.csFileName), source); return CompletedTask; } );
        }

        private string WkPath(string path)
        {
            return Path.Combine(directory, path);
        }

        private void MkDir()
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void WriteExeToDisk(string path, Compile.Compilation compilation)
        {
            this.retryPolicy.ExecuteAsync(() => 
            {
                using (var fs = File.OpenWrite(WkPath(path)))
                {
                    compilation.Assembly.WriteTo(fs);
                }

                return CompletedTask;
            });

            if (compilation.Pdb != Stream.Null)
            {
                this.retryPolicy.ExecuteAsync(() =>
                {
                    string pdbPath = WkPath(Path.ChangeExtension(path, ".pdb"));

                    using (var fs = File.OpenWrite(pdbPath))
                    {
                        compilation.Pdb.WriteTo(fs);
                    }

                    return CompletedTask;
                });
            }

            string json = @"
{
    ""runtimeOptions"": 
    {
        ""tfm"": ""netcoreapp3.1"",
        ""framework"": 
        {
            ""name"": ""Microsoft.NETCore.App"",
            ""version"": ""3.1.0""
        }
    }
 }";

            // "test.runtimeconfig.json"
            var settingsFileName = WkPath(Path.ChangeExtension(path, ".runtimeconfig.json"));

            if (!File.Exists(settingsFileName))
            {
                this.retryPolicy.ExecuteAsync(() =>
                {
                    File.WriteAllText(settingsFileName, json);

                    return CompletedTask;
                });
            }
        }

        private Process Execute(string path, Config config)
        {
            // Process.Start("dotnet", assemblyPath);
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = WkPath(path),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };

            // https://docs.microsoft.com/en-us/dotnet/core/run-time-config/compilation
            // https://docs.microsoft.com/en-us/dotnet/framework/configure-apps/file-schema/runtime/uselegacyjit-element
            bool tieredCompilation = (config.JitMode & JitMode.Tiered) == JitMode.Tiered;
            bool quickJit = (config.JitMode & JitMode.Quick) == JitMode.Quick;
            bool quickLoopJit = (config.JitMode & JitMode.QuickLoop) == JitMode.QuickLoop;
            bool useLegacyJit = (config.JitMode & JitMode.Legacy) == JitMode.Legacy;

            proc.StartInfo.Environment["COMPlus_TieredCompilation"] = tieredCompilation ? "1" : "0";
            proc.StartInfo.Environment["COMPlus_TC_QuickJit"] = quickJit ? "1" : "0";
            proc.StartInfo.Environment["COMPlus_TC_QuickJitForLoops"] = quickLoopJit ? "1" : "0";
            proc.StartInfo.Environment["COMPlus_useLegacyJit"] = useLegacyJit ? "1" : "0";

            proc.Start();

            return proc;
        }

        private DisassemblyResult AttachAndDecompile(int processId, string className, string methodName, string source)
        {
            // cache 1 source provider per version of the input source code
            // (user can dissassemble the same source with different JIT options)
            // TODO: Key here should include compile options, but not jit options
            var sourceProvider = sourceCodeProviders.GetOrAdd(source, (s) => new SourceCodeProvider(directory));

            var settings = new Settings(
                processId,
                className,
                methodName,
                true,
                3,
                "results.txt",
                Array.Empty<string>(),
                sourceProvider);

            var dissassembler = new ClrMdDisassembler();

            return dissassembler.AttachAndDisassemble(settings);
        }

        private static Dissassembly FormatResult(DisassemblyResult result)
        {
            var builder = new DissassemblyBuilder();

            int referenceIndex = 0;
            int methodIndex = 0;

            
            foreach (var method in result.Methods.Where(method => string.IsNullOrEmpty(method.Problem)))
            {
                referenceIndex++;
                var f = new AsmFormat(printInstructionAddresses: false);

                var pretty = DisassemblyPrettifier.Prettify(method, result, $"M{methodIndex++:00}", f);

                // Note leading empty hidden unicode char for syntax highlighting
                builder.AddLine($"‎{MethodNameFormatter.Short(DesktopMethodNameParser.Parse(method.Name))}");

                // TODO: reverse lookup of method into compiled syntax tree/find defn line number, and insert
                // so that user can navigate there.

                ulong totalSizeInBytes = 0;
                foreach (var element in pretty)
                {
                    switch (element)
                    {
                        case DisassemblyPrettifier.Reference reference:
                            // reference ID = label
                            builder.AddReference(element.TextRepresentation, reference.Id);
                            break;
                        case DisassemblyPrettifier.Label label:
                            builder.AddLabel(label.Id);
                            break;
                        default:
                            if (element.Source is Sharp)
                            {
                                builder.AddLine(element.TextRepresentation, element.Address, element.LineNo);
                            }    
                            else
                            {
                                builder.AddLine(element.TextRepresentation, element.Address, 0);
                            }
                            break;
                    } 

                    if (element.Source is Asm asm)
                    {
                        checked
                        {
                            totalSizeInBytes += (uint)asm.Instruction.ByteLength;
                        }
                    }
                }

                builder.AddLine($"; Total bytes of code {totalSizeInBytes}");
                builder.AddLine();
            }

            foreach (var withProblems in result.Methods
            .Where(method => !string.IsNullOrEmpty(method.Problem))
            .GroupBy(method => method.Problem))
            {
                builder.AddLine($";{withProblems.Key}");

                foreach (var withProblem in withProblems)
                {
                    builder.AddLine($";{MethodNameFormatter.Short(DesktopMethodNameParser.Parse(withProblem.Name))}");
                }
            }

            return builder.Build();
        }

        private static void ValidateExeName(string exeName)
        { 
            if (!exeName.EndsWith(".exe"))
            {
                throw new ArgumentException("Invalid exe name");
            }
        }
    }
}
