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

        private string correctSource = "namespace JitExplorer { public class Test { ["+ RuntimeDisassembler.AttributeName + "] public static void Execute() { int i = 0; } } }";

        [Fact]
        public void SimpleProgramProducesDissassembledOutput()
        {
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

            string jitOut = jit.CompileJitAndDisassemble(correctSource, config).AsmText;

            output.WriteLine(jitOut);

            jitOut.Should().StartWith("Test.Execute()");
        }

        [Fact]
        public void BuildFailBuildIsWorking()
        {
            string badSource = "asdsad";

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

            var result = jit.CompileJitAndDisassemble(badSource, config);
            result.IsSuccess.Should().BeFalse();

            result = jit.CompileJitAndDisassemble(badSource, config);
            result.IsSuccess.Should().BeFalse();

            result = jit.CompileJitAndDisassemble(correctSource, config);
            result.IsSuccess.Should().BeTrue();
        }
    }
}
