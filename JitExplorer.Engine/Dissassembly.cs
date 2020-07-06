using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine
{
    public class Dissassembly
    {
        public Dissassembly(string text, Dictionary<int, string> lineAddresses)
        {
            this.Text = text;
            this.LineAddresses = lineAddresses;
        }

        public Dictionary<int, string> LineAddresses { get; }

        public string Text { get; }
    }
}
