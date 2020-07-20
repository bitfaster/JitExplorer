using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JitExplorer.Engine.UnitTests
{
    public class DisassemblyBuilderTests
    {
        [Fact]
        public void WhenAddingLinesTheAddressesAreCountedCorrectly()
        {
            var b = new DisassemblyBuilder();

            b.AddLine();
            b.AddLine("line2");
            b.AddLine("line3", "03", 0);
            b.AddLine("line4" + Environment.NewLine + "stillline4", "04", 0);
            b.AddLine("line6", "06", 0);

            var r = b.Build();

            string address;
            r.AsmLineAddressIndex.TryGetValue(1, out address).Should().BeFalse();
            r.AsmLineAddressIndex.TryGetValue(2, out address).Should().BeFalse();
            r.AsmLineAddressIndex[3].Should().Be("03");
            r.AsmLineAddressIndex[4].Should().Be("04");
            r.AsmLineAddressIndex.TryGetValue(5, out address).Should().BeFalse();
            r.AsmLineAddressIndex[6].Should().Be("06");
        }
    }
}
