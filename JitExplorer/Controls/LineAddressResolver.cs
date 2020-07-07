using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Controls
{
    public interface ILineAddressResolver
    {
        string GetAddress(int line);
    }

    public class EmptyAddressResolver : ILineAddressResolver
    {
        public string GetAddress(int line)
        {
            return string.Empty;
        }
    }

    public class LineAddressResolver : ILineAddressResolver
    {
        private readonly Dictionary<int, string> lineAddresses;

        public LineAddressResolver(Dictionary<int, string> lineAddresses)
        {
            this.lineAddresses = lineAddresses;
        }

        public string GetAddress(int line)
        {
            if (this.lineAddresses.TryGetValue(line, out var a))
            {
                return a;
            }

            return string.Empty;
        }
    }
}
