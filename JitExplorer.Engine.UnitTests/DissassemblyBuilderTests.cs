using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JitExplorer.Engine.UnitTests
{
    public class DissassemblyBuilderTests
    {
        [Fact]
        public void WhenAddingLinesTheAddressesAreCountedCorrectly()
        {
            var b = new DissassemblyBuilder();

            b.AddLine();
            b.AddLine("line2");
            b.AddLine("line3", "03");
            b.AddLine("line4" + Environment.NewLine + "stillline4", "04");
            b.AddLine("line6", "06");

            var r = b.Build();

            string address;
            r.LineAddresses.TryGetValue(1, out address).Should().BeFalse();
            r.LineAddresses.TryGetValue(2, out address).Should().BeFalse();
            r.LineAddresses[3].Should().Be("03");
            r.LineAddresses[4].Should().Be("04");
            r.LineAddresses.TryGetValue(5, out address).Should().BeFalse();
            r.LineAddresses[6].Should().Be("06");
        }
    }
}
