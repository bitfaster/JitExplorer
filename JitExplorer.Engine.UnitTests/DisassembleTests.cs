using BitFaster.Caching.Lru;
using Iced.Intel;
using JitExplorer.Engine.Disassemble;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace JitExplorer.Engine.UnitTests
{
    public class DisassembleTests
    {
        private readonly ITestOutputHelper output;

        public DisassembleTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        // This only passes in Release build
        [Fact]
        public void BenchmarkDotNetMethod()
        {
            // works if wrapper is invoked - it is jitted.
            Wrapper();

            // also works with PrepareMethod
            //var methodInfo = typeof(DisassembleTests).GetMethod("Wrapper");
            //RuntimeHelpers.PrepareMethod(methodInfo.MethodHandle);

            string fn = typeof(DisassembleTests).FullName;

            // Settings(int processId, string typeName, string methodName, bool printSource, int maxDepth, string resultsPath)
            var settings = new Settings(
                Process.GetCurrentProcess().Id,
                typeof(DisassembleTests).FullName,
                "Wrapper",
                true,
                3,
                "results.txt",
                Array.Empty<string>());

            var result = ClrMdDisassembler.AttachAndDisassemble(settings);

            // From
            // \src\BenchmarkDotNet\Disassemblers\Exporters\HtmlDisassemblyExporter.cs

            int referenceIndex = 0;
            int methodIndex = 0;
            foreach (var method in result.Methods.Where(method => string.IsNullOrEmpty(method.Problem)))
            {
                referenceIndex++;
                var f = new AsmFormat();
                var pretty = DisassemblyPrettifier.Prettify(method, result,  $"M{methodIndex++:00}", f);

                //output.WriteLine($"<tr><th colspan=\"2\" style=\"text-align: left;\">{method.Name}</th><th></th></tr>");
                output.WriteLine($"{method.Name}");

                bool even = false, diffTheLabels = pretty.Count > 1;
                foreach (var element in pretty)
                {
                    if (element is DisassemblyPrettifier.Label label)
                    {
                        even = !even;

                        //output.WriteLine($"<tr class=\"{(even && diffTheLabels ? "evenMap" : string.Empty)}\">");
                        //output.WriteLine($"<td id=\"{referenceIndex}_{label.Id}\" class=\"label\" data-label=\"{referenceIndex}_{label.TextRepresentation}\"><pre><code>{label.TextRepresentation}</pre></code></td>");
                        //output.WriteLine("<td>&nbsp;</td></tr>");

                        output.WriteLine($"{label.TextRepresentation}");

                        continue;
                    }

                    // output.WriteLine($"<tr class=\"{(even && diffTheLabels ? "evenMap" : string.Empty)}\">");
                    // output.WriteLine("<td></td>");

                    if (element is DisassemblyPrettifier.Reference reference)
                    {
                        //output.WriteLine($"<td id=\"{referenceIndex}\" class=\"reference\" data-reference=\"{referenceIndex}_{reference.Id}\"><a href=\"#{referenceIndex}_{reference.Id}\"><pre><code>{reference.TextRepresentation}</pre></code></a></td>");
                        output.WriteLine($"{reference.TextRepresentation}");
                    }
                    else
                    {
                        //output.WriteLine($"<td><pre><code>{element.TextRepresentation}</pre></code></td>"); 
                        output.WriteLine($"{element.TextRepresentation}");
                    }

                    //output.WriteLine("</tr>");
                }

                //output.WriteLine("<tr><td colspan=\"{2}\">&nbsp;</td></tr>");

                foreach (var withProblems in result.Methods
                .Where(method => !string.IsNullOrEmpty(method.Problem))
                .GroupBy(method => method.Problem))
                {
                    output.WriteLine($"<tr><td colspan=\"{2}\"><b>{withProblems.Key}</b></td></tr>");
                    foreach (var withProblem in withProblems)
                    {
                        output.WriteLine($"<tr><td colspan=\"{2}\">{withProblem.Name}</td></tr>");
                    }
                    //output.WriteLine("<tr><td colspan=\"{2}\"></td></tr>");
                }
            }
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Wrapper()
        {
            var lru = new FastConcurrentLru<int, int>(5);
            lru.TryGet(1, out var v);
        }
#pragma warning restore xUnit1013 // Public method should be marked as test
    }
}
