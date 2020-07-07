using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JitExplorer.Engine
{
    public class DissassemblyBuilder
    {
        private int lineNo = 1;
        private StringBuilder sb = new StringBuilder();

        private Dictionary<int, string> asmLineToAddressIndex = new Dictionary<int, string>();
        private Dictionary<int, int> asmToSourceLineIndex = new Dictionary<int, int>();

        // label to asm line mapping
        private Dictionary<string, int> asmLabelToLineIndex = new Dictionary<string, int>();
        private List<Tuple<int, string>> linesContainingLabels = new List<Tuple<int, string>>();

        public void AddLine()
        {
            sb.AppendLine(string.Empty);
            lineNo++;
        }

        public void AddLine(string text)
        {
            HandleMultiline(text);
        }

        public void AddLabel(string text)
        {
            this.asmLabelToLineIndex.Add(text, lineNo);
            sb.AppendLine(text);
            lineNo++;
        }

        public void AddReference(string text, string label)
        {
            this.linesContainingLabels.Add(new Tuple<int, string>(lineNo, label));
            sb.AppendLine(text);
            lineNo++;
        }

        public void AddLine(string text, string address, int sourceLine)
        {
            asmLineToAddressIndex.Add(lineNo, address);

            if (sourceLine != 0)
            {
                asmToSourceLineIndex[lineNo] = sourceLine;
            }

            HandleMultiline(text);
        }

        private void HandleMultiline(string text)
        {
            sb.AppendLine(text);
            lineNo++;

            int n = 0;

            if (text != string.Empty)
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
            var asmLineToAsmLineIndex = new Dictionary<int, int>(this.linesContainingLabels.Count);

            foreach (var line in this.linesContainingLabels)
            {
                if (this.asmLabelToLineIndex.TryGetValue(line.Item2, out var index))
                {
                    asmLineToAsmLineIndex.Add(line.Item1, index);
                }
            }

            return new Dissassembly(
                sb.ToString(), 
                asmLineToAddressIndex, 
                asmToSourceLineIndex,
                asmLineToAsmLineIndex);
        }
    }
}
