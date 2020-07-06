using System;
using System.Collections.Generic;
using System.Text;
using static JitExplorer.Engine.Disassemble.DisassemblyPrettifier;

namespace JitExplorer.Engine
{
    public class DissassemblyBuilder
    {
        private int lineNo = 1;
        private StringBuilder sb = new StringBuilder();
        private Dictionary<int, string> addresses = new Dictionary<int, string>();

        public void AddLine()
        {
            this.AddLine(string.Empty, string.Empty);
        }

        public void AddLine(string text)
        {
            this.AddLine(text, string.Empty);
        }

        public void AddLine(string text, string address)
        {
            sb.AppendLine(text);
            addresses.Add(lineNo++, address);
            AddText(text);
        }

        private void AddText(string text)
        {
            int count = 0, n = 0;

            var newLine = Environment.NewLine;
            if (text != "")
            {
                while ((n = text.IndexOf(newLine, n, StringComparison.InvariantCulture)) != -1)
                {
                    n += newLine.Length;
                    ++count;
                }
            }

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    addresses.Add(lineNo++, string.Empty);
                }
            }
        }

        public Dissassembly Build()
        {
            return new Dissassembly(sb.ToString(), addresses);
        }
    }
}
