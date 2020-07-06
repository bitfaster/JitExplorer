using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine
{
    public class Dissassembly
    {
        public Dissassembly(bool isSuccess, string text, Dictionary<int, string> lineAddresses)
        {
            this.IsSuccess = isSuccess;
            this.Text = text;
            this.LineAddresses = lineAddresses;
        }

        public bool IsSuccess { get; }

        public Dictionary<int, string> LineAddresses { get; }

        public string Text { get; }
    }
}
