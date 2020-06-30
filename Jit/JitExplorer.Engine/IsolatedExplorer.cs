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
    public class IsolatedExplorer
    {
        public string CompileJitAndDisassemble(string sourceCode)
        {
            var c = Compile("test.exe", sourceCode);

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
                Thread.Sleep(1000);

                // TODO: why does compiled .exe not contain the program class? Decompile shows it is there.
                // Because exe can't run
                // test.runtimeconfig.json
                var result = AttachAndDecompile(p.Id, "Testing.Program", "Main");

                p.WaitForExit();

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

            
            return c.Compile(assembylyName, "program.cs", source);
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
                "results.txt"
                );

            return ClrMdDisassembler.AttachAndDisassemble(settings);
        }
    }
}
