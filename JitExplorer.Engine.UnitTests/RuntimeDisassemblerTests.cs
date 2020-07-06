using FluentAssertions;
using JitExplorer.Engine.Compile;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace JitExplorer.Engine.UnitTests
{
    public class RuntimeDisassemblerTests
    {
        private readonly ITestOutputHelper output;

        public RuntimeDisassemblerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void SimpleProgramProducesDissassembledOutput()
        {
            string source = "namespace Testing { public class Program { public static void Main(string[] args) { int i = 0; JitExplorer.Signal.__Jit(); } } }";

            var jit = new RuntimeDisassembler("test2.exe");

            var compilerOptions = new CompilerOptions()
            {
                Platform = Microsoft.CodeAnalysis.Platform.X64,
                OptimizationLevel = OptimizationLevel.Release,
                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.Default,
                OutputKind = OutputKind.ConsoleApplication,
            };

            var config = new Config()
            {
                CompilerOptions = compilerOptions,
                JitMode = JitMode.Default,
            };

            string jitOut = jit.CompileJitAndDisassemble(source, config).Text;

            output.WriteLine(jitOut);

            jitOut.Should().StartWith("Program.Main(String[])");
        }
    }
}
