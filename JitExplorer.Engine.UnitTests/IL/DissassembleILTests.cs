using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace JitExplorer.Engine.UnitTests.IL
{
    public class DissassembleILTests
    {
        private readonly ITestOutputHelper output;

        public DissassembleILTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var m = this.GetType().GetMethod(nameof(Test));

            string data = JitExplorer.Engine.IL.Formatter.FormatMethodBody(m);

            this.output.WriteLine(data);
        }
    }
}
