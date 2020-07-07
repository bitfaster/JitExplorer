using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine
{
    public class DissassemblyBuilder
    {
        private int lineNo = 1;
        private StringBuilder sb = new StringBuilder();
        private Dictionary<int, string> addresses = new Dictionary<int, string>();

        public void AddLine()
        {
            sb.AppendLine(string.Empty);
            lineNo++;
        }

        public void AddLine(string text)
        {
            HandleMultiline(text);
        }

        public void AddLine(string text, string address)
        {
            addresses.Add(lineNo, address);
            HandleMultiline(text);
        }

        private void HandleMultiline(string text)
        {
            sb.AppendLine(text);
            lineNo++;

            int n = 0;

            if (text != "")
            {
                while ((n = text.IndexOf(Environment.NewLine, n, StringComparison.InvariantCulture)) != -1)
                {
                    n += Environment.NewLine.Length;
                    lineNo++;
                }
            }
        }

        public Dissassembly Build()
        {
            return new Dissassembly(true, sb.ToString(), addresses);
        }
    }
}
