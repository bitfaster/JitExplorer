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
        public string CompileJitAndDisassemble(string sourceCode)
        {
            if (!sourceCode.Contains("JitExplorer.Signal.__Jit();"))
            {
                return "Please include this method call to trigger JIT: JitExplorer.Signal.__Jit();";
            }

            using var c = Compile("test.exe", sourceCode);

            if (!c.Succeeded)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var e in c.Messages)
                {
                    sb.AppendLine(e.ToString());
                }

                return sb.ToString();
            }

            WriteExeToDisk("test.exe", c);
            using (var p = Execute("test.exe"))
            {
                //Thread.Sleep(1000);
                using var cpipe = new System.IO.Pipes.NamedPipeClientStream(".", "MyTest.Pipe", System.IO.Pipes.PipeDirection.InOut, System.IO.Pipes.PipeOptions.None);
                cpipe.Connect(1000);

                var result = AttachAndDecompile(p.Id, "Testing.Program", "Main");

                // signal dissassemble complete
                cpipe.WriteByte(1);

                p.WaitForExit(1000);

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

        private Compilation Compile(string assembylyName, string source)
        {
            var options = new CompilerOptions() { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication };
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
                compilation.programExecutable.WriteTo(fs);
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

        private Process Execute(string path)
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

            proc.Start();

            return proc;
        }

        private DisassemblyResult AttachAndDecompile(int processId, string className, string methodName)
        {
            // filter out the synchronization code
            string[] filtered = {
                "JitExplorer.Signal.__Jit()",
                "System.IO.Pipes.NamedPipeServerStream..ctor(System.String, System.IO.Pipes.PipeDirection)",
                "System.IO.Pipes.NamedPipeServerStream.WaitForConnection()",
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
    }
}
