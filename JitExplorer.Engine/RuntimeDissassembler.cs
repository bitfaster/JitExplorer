using BitFaster.Caching.Lru;
using JitExplorer.Engine.Compile;
using JitExplorer.Engine.Disassemble;
using JitExplorer.Engine.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace JitExplorer.Engine
{
    // Requires source code calls JitExplorer.Signal.__Jit();
    public class RuntimeDissassembler
    {
        public event EventHandler<ProgressEventArgs> Progress;

        private readonly string exeName;
        private readonly string csFileName;
        private static ClassicLru<string, SourceCodeProvider> sourceCodeProviders = new ClassicLru<string, SourceCodeProvider>(10);

        public RuntimeDissassembler(string exeName)
        {
            ValidateExeName(exeName);
            this.exeName = exeName;
            this.csFileName = "program.cs";
        }

        public Dissassembly CompileJitAndDisassemble(string sourceCode, Config config)
        {
            if (!sourceCode.Contains("JitExplorer.Signal.__Jit();"))
            {
                return new Dissassembly("Please include this method call to trigger JIT: JitExplorer.Signal.__Jit();", new Dictionary<int, string>());
            }

            this.Progress?.Invoke(this, new ProgressEventArgs() { StatusMessage = "Compiling..." });
            using var c = Compile(this.exeName, sourceCode, config);

            if (!c.Succeeded)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var e in c.Diagnostics)
                {
                    sb.AppendLine(e.ToString());
                }

                return new Dissassembly(sb.ToString(), new Dictionary<int, string>());
            }

            this.Progress?.Invoke(this, new ProgressEventArgs() { StatusMessage = "Writing to disk..." });
            WriteSourceToDisk(sourceCode);
            WriteExeToDisk(this.exeName, c);

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
                var result = AttachAndDecompile(p.Id, "Testing.Program", "Main", sourceCode);

                // signal dissassemble complete
                cpipe.WriteByte(1);

                if (!p.WaitForExit(1000))
                {
                    p.Kill();
                }

                return FormatResult(result);
            }
        }

        private Compilation Compile(string assembylyName, string source, Config config)
        {
            if (config.CompilerOptions.OutputKind != Microsoft.CodeAnalysis.OutputKind.ConsoleApplication)
            {
                throw new ArgumentOutOfRangeException("OutputKind must be ConsoleApplication");
            }

            Compiler c = new Compiler(config.CompilerOptions);

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
}";

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

            var jitSyntax = c.Parse("jitexpl.cs", jitExplSource);
            // var assSyntax = c.CreateSyntaxTree("assemblyinfo.cs", assemblyInfo);
            var syntax = c.Parse(this.csFileName, source);
            return c.Compile(assembylyName, syntax, jitSyntax);
        }

        private void WriteSourceToDisk(string source)
        {
            File.WriteAllText(this.csFileName, source);
        }

        private void WriteExeToDisk(string path, Compilation compilation)
        {
            using (var fs = File.OpenWrite(path))
            {
                compilation.Assembly.WriteTo(fs);
            }

            if (compilation.Pdb != Stream.Null)
            {
                string pdbPath = Path.ChangeExtension(path, ".pdb");

                using (var fs = File.OpenWrite(pdbPath))
                {
                    compilation.Pdb.WriteTo(fs);
                }
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
            var settingsFileName = Path.ChangeExtension(path, ".runtimeconfig.json");

            if (!File.Exists(settingsFileName))
            {
                File.WriteAllText(settingsFileName, json);
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
                    Arguments = path,
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
            // filter out the synchronization code
            string[] filtered = {
                "JitExplorer.Signal.__Jit()",
                "System.IO.Pipes.NamedPipeServerStream..ctor(System.String, System.IO.Pipes.PipeDirection, Int32, System.IO.Pipes.PipeTransmissionMode, System.IO.Pipes.PipeOptions, Int32, Int32, System.IO.HandleInheritability)",
                "System.IO.Pipes.NamedPipeServerStream..ctor(System.String, System.IO.Pipes.PipeDirection)",
                "System.IO.Pipes.NamedPipeServerStream.WaitForConnection()",
                "System.IO.Pipes.PipeStream.ReadByte()",
                "System.IO.Pipes.PipeStream.Dispose(Boolean)",
                "Interop+Kernel32.ConnectNamedPipe(Microsoft.Win32.SafeHandles.SafePipeHandle, IntPtr)",
            };

            // cache 1 source provider per version of the input source code
            // (user can dissassemble the same source with different JIT options)
            var sourceProvider = sourceCodeProviders.GetOrAdd(source, (s) => new SourceCodeProvider());

            var settings = new Settings(
                processId,
                className,
                methodName,
                true,
                3,
                "results.txt",
                filtered,
                sourceProvider);

            return ClrMdDisassembler.AttachAndDisassemble(settings);
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
                    builder.AddLine(element.TextRepresentation, element.Address);

                    if (element.Source is Asm asm)
                    {
                        checked
                        {
                            totalSizeInBytes += (uint)asm.Instruction.ByteLength;
                        }
                    }
                }

                sb.AppendLine($"; Total bytes of code {totalSizeInBytes}");

                builder.AddLine();
            }

            foreach (var withProblems in result.Methods
            .Where(method => !string.IsNullOrEmpty(method.Problem))
            .GroupBy(method => method.Problem))
            {
                builder.AddLine(withProblems.Key);

                foreach (var withProblem in withProblems)
                {
                    builder.AddLine(withProblem.Name);
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
