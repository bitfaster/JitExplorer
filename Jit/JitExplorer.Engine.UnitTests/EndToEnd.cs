﻿using JitExplorer.Engine.Compile;
using JitExplorer.Engine.Disassemble;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public void Test()
        {
            // Real impl
            // - need to generate a well defined entry point, it will have 2 signals (which replace sleeps)
            // - entry point executed, JIT is done
            // - then wait for signal to stop, when Disassemble is done
            // - entry point will need to invoke whatever code user has typed in, like the benchmark.
            //   or use reflection to try to prepare all non-generic methods (visit syntax tree to generate this code)

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
                // Is it related to release build or embedded debug info?
                var r = AttachAndDecompile(p.Id, "Testing.Program", "Main");

                p.WaitForExit();

                foreach (var result in r.Methods)
                {
                    output.WriteLine(result.Name);
                }
            }
        }

        private Compilation Compile(string assembylyName)
        {
            var options = new CompilerOptions() { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication };
            Compiler c = new Compiler(options);

            string source = "namespace Testing { public class Program { public static void Main() { int i = 0; System.Threading.Thread.Sleep(50000); } } public class Testy {} }";

            return c.Compile(assembylyName, "program.cs", source);
        }

        private void WriteExeToDisk(string path, Compilation compilation)
        {
            using (var fs = File.OpenWrite(path))
            {
                compilation.programExecutable.WriteTo(fs);
            }
        }

        private Process Execute(string path)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = string.Empty,
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
                "results.txt"
                );

            return ClrMdDisassembler.AttachAndDisassemble(settings);
        }
    }
}