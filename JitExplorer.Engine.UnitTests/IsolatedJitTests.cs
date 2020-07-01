using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace JitExplorer.Engine.UnitTests
{
    public class IsolatedJitTests
    {
        private readonly ITestOutputHelper output;

        public IsolatedJitTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test()
        {
            string source = "namespace Testing { public class Program { public static void Main(string[] args) { int i = 0; JitExplorer.Signal.__Jit(); } } }";

            var jit = new IsolatedJit();

            string jitOut = jit.CompileJitAndDisassemble(source);

            output.WriteLine(jitOut);

            jitOut.Should().NotBeNullOrEmpty();

            //var sPipe = new System.IO.Pipes.NamedPipeServerStream("MyTest.Pipe", System.IO.Pipes.PipeDirection.InOut);
            //sPipe.WaitForConnection();
            //sPipe.ReadByte();

            //var cpipe = new System.IO.Pipes.NamedPipeClientStream(".", "MyTest.Pipe", System.IO.Pipes.PipeDirection.InOut, System.IO.Pipes.PipeOptions.None);
            //cpipe.Connect();

            // jit

            //cpipe.WriteByte(1);
        }
    }
}
