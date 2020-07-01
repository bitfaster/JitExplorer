using JitExplorer.Engine.Compile;
using JitExplorer.Engine.Disassemble;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace JitExplorer.Engine.UnitTests
{
    public class EndToEnd
    {
        private readonly ITestOutputHelper output;

        public EndToEnd(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestIsolatedProcess()
        {
            // Real impl
            // - need to generate a well defined entry point, it will have 2 signals (which replace sleeps)
            // - entry point executed, JIT is done
            // - then wait for signal to stop, when Disassemble is done
            // - entry point will need to invoke whatever code user has typed in, like the benchmark.
            //   or use reflection to try to prepare all non-generic methods (visit syntax tree to generate this code)
            //   Let's say the rule is that you must define a non generic class. Will it be enough to prepare method 
            //   on these root objects?

            // 1. compile source code
            // 2. write to disk
            // 3. execute via process
            // 4. attach and decompile
            // 5. print result

            var c = Compile("test.exe");
            WriteExeToDisk("test.exe", c);
            using (var p = Execute("test.exe"))
            {
                Thread.Sleep(1000);

                // TODO: why does compiled .exe not contain the program class? Decompile shows it is there.
                // Because exe can't run
                // test.runtimeconfig.json
                var r = AttachAndDecompile(p.Id, "Testing.Program", "Main");

                p.WaitForExit();

                foreach (var result in r.Methods)
                {
                    output.WriteLine(result.Name);
                }
            }
        }

#if !DEBUG
        // Requires release build
        [Fact]
        public void TestInMem()
        {
            var c = Compile("test.exe");

            var a = Assembly.Load(c.programExecutable.ToArray());

            var t = a.GetType("Testing.Program");
            var m = t.GetMethod("Main");
            RuntimeHelpers.PrepareMethod(m.MethodHandle);

            var r = AttachAndDecompile(Process.GetCurrentProcess().Id, "Testing.Program", "Main");

            foreach (var result in r.Methods)
            {
                output.WriteLine(result.Name);
            }
        }
#endif

        private Compilation Compile(string assembylyName)
        {
            var options = new CompilerOptions() { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication };
            Compiler c = new Compiler(options);

            string source = "namespace Testing { public class Program { public static void Main(string[] args) { int i = 0; System.Threading.Thread.Sleep(2000); } } public class Testy {} }";

            var syntax = c.CreateSyntaxTree("program.cs", source);
            return c.Compile(assembylyName, syntax);
        }

        private void WriteExeToDisk(string path, Compilation compilation)
        {
            using (var fs = File.OpenWrite(path))
            {
                compilation.programExecutable.WriteTo(fs);
            }

            string json = @"{
  ""runtimeOptions"": {
    ""tfm"": ""netcoreapp3.0"",
    ""framework"": {
                ""name"": ""Microsoft.NETCore.App"",
      ""version"": ""3.0.0-preview6-27804-01""
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
            var settings = new Settings(
                processId,
                className,
                methodName,
                true,
                3,
                "results.txt",
                Array.Empty<string>()
                );

            return ClrMdDisassembler.AttachAndDisassemble(settings);
        }
    }
}
