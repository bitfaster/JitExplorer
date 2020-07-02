using JitExplorer.Engine.Compile;
using JitExplorer.Engine.Disassemble;
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
    public class IsolatedJit
    {
        public event EventHandler<ProgressEventArgs> Progress;

        public string CompileJitAndDisassemble(string sourceCode, Config config)
        {
            if (!sourceCode.Contains("JitExplorer.Signal.__Jit();"))
            {
                return "Please include this method call to trigger JIT: JitExplorer.Signal.__Jit();";
            }

            this.Progress?.Invoke(this, new ProgressEventArgs() { StatusMessage = "Compiling..." });
            using var c = Compile("test.exe", sourceCode, config);

            if (!c.Succeeded)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var e in c.Diagnostics)
                {
                    sb.AppendLine(e.ToString());
                }

                return sb.ToString();
            }

            this.Progress?.Invoke(this, new ProgressEventArgs() { StatusMessage = "Writing to disk..." });
            WriteExeToDisk("test.exe", c);

            this.Progress?.Invoke(this, new ProgressEventArgs() { StatusMessage = "Jitting..." });
            using (var p = Execute("test.exe", config))
            {
                using var cpipe = new System.IO.Pipes.NamedPipeClientStream(".", "MyTest.Pipe", System.IO.Pipes.PipeDirection.InOut, System.IO.Pipes.PipeOptions.None);
                
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
                var result = AttachAndDecompile(p.Id, "Testing.Program", "Main");

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
            var options = new CompilerOptions() 
            { 
                OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication,
                OptimizationLevel = config.OptimizationLevel,
                Platform = config.Platform,
            };

            Compiler c = new Compiler(options);

            // string pipeName = "MyTest.Pipe";

            var jitExplSource = @"namespace JitExplorer 
{ 
    public static class Signal 
    { 
        public static void __Jit() 
        { 
            using (var sPipe = new System.IO.Pipes.NamedPipeServerStream(""MyTest.Pipe"", System.IO.Pipes.PipeDirection.InOut))
            {
                sPipe.WaitForConnection();
                sPipe.ReadByte(); // wait for signal that code is dissassembled
            }
        } 
    } 
}";

            var jitSyntax = c.CreateSyntaxTree("jitexpl.cs", jitExplSource);
            var syntax = c.CreateSyntaxTree("program.cs", source);
            return c.Compile(assembylyName, syntax, jitSyntax);
        }

        private void WriteExeToDisk(string path, Compilation compilation)
        {
            using (var fs = File.OpenWrite(path))
            {
                compilation.Assembly.WriteTo(fs);
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
        },
        ""configProperties"": 
        {
            ""TieredCompilation"": ""false""
        }
    }
 }";

            if (!File.Exists("test.runtimeconfig.json"))
            {
                File.WriteAllText("test.runtimeconfig.json", json);
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
            bool tieredCompilation = false;
            bool quickJit = false;
            bool quickLoopJit = false;
            bool useLegacyJit = config.UseTieredCompilation;

            proc.StartInfo.Environment["COMPlus_TieredCompilation"] = tieredCompilation ? "1" : "0";
            proc.StartInfo.Environment["COMPlus_TC_QuickJit"] = quickJit ? "1" : "0";
            proc.StartInfo.Environment["COMPlus_TC_QuickJitForLoops"] = quickLoopJit ? "1" : "0";
            proc.StartInfo.Environment["COMPlus_useLegacyJit"] = useLegacyJit ? "1" : "0";

            proc.Start();

            return proc;
        }

        private DisassemblyResult AttachAndDecompile(int processId, string className, string methodName)
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

            var settings = new Settings(
                processId,
                className,
                methodName,
                true,
                3,
                "results.txt",
                filtered
                );

            return ClrMdDisassembler.AttachAndDisassemble(settings);
        }

        private static string FormatResult(DisassemblyResult result)
        {
            StringBuilder sb = new StringBuilder();

            int referenceIndex = 0;
            int methodIndex = 0;
            foreach (var method in result.Methods.Where(method => string.IsNullOrEmpty(method.Problem)))
            {
                referenceIndex++;

                var pretty = DisassemblyPrettifier.Prettify(method, result, $"M{methodIndex++:00}");

                sb.AppendLine($"{method.Name}");

                foreach (var element in pretty)
                {
                    sb.AppendLine(element.TextRepresentation);
                }

                sb.AppendLine();
            }

            foreach (var withProblems in result.Methods
            .Where(method => !string.IsNullOrEmpty(method.Problem))
            .GroupBy(method => method.Problem))
            {
                sb.AppendLine(withProblems.Key);

                foreach (var withProblem in withProblems)
                {
                    sb.AppendLine(withProblem.Name);
                }
            }

            return sb.ToString();
        }
    }
}
