using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JitExplorer.Engine.Metadata
{
    public static class SpanExtensions
    {
        public static ReadOnlySpan<char> Concat(this ReadOnlySpan<char> first, ReadOnlySpan<char> second)
        {
            return new string(first.ToArray().Concat(second.ToArray()).ToArray()).ToArray();
        }
    }
}
