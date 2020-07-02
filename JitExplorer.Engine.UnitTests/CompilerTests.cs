using System;
using Xunit;

using JitExplorer.Engine.Compile;
using FluentAssertions;
using System.Reflection;

namespace JitExplorer.Engine.UnitTests
{
    public class CompilerTests
    {
        private string simpleProgramSource = "public class Program { public static void Main() {} }";

        [Fact]
        public void SimpleProgramCanCompile()
        {
            var options = new CompilerOptions() { OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication };
            Compiler c = new Compiler(options);

            var syntax = c.CreateSyntaxTree("foo.cs", simpleProgramSource);
            using (var exec = c.Compile("test.exe", syntax))
            {
                exec.Succeeded.Should().BeTrue();
            }
        }

        [Fact]
        public void SimpleProgramInReleaseModeIsJitOptimizationEnabled()
        {
            var options = new CompilerOptions() 
            { 
                OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication,
                OptimizationLevel = Microsoft.CodeAnalysis.OptimizationLevel.Release,
            };

            Compiler c = new Compiler(options);

            var syntax = c.CreateSyntaxTree("foo.cs", simpleProgramSource);
            using (var exec = c.Compile("test.exe", syntax))
            {
                var a = Assembly.Load(exec.Assembly.ToArray());
                a.IsJitOptimizationDisabled().Should().BeFalse();
            }
        }

        [Fact]
        public void SimpleProgramInDebugModeIsDebug()
        {
            var options = new CompilerOptions()
            {
                OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication,
                OptimizationLevel = Microsoft.CodeAnalysis.OptimizationLevel.Debug,
            };

            Compiler c = new Compiler(options);

            var syntax = c.CreateSyntaxTree("foo.cs", simpleProgramSource);
            using (var exec = c.Compile("test.exe", syntax))
            {
                var a = Assembly.Load(exec.Assembly.ToArray());
                a.IsJitOptimizationDisabled().Should().BeTrue();
                a.IsDebug().Should().BeTrue();
            }
        }
    }
}
