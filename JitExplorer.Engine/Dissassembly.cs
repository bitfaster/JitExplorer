using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine
{
    public class Dissassembly
    {
        public Dissassembly(string text)
        {
            this.IsSuccess = false;
            this.Text = text;
            this.AsmLineAddressIndex = new Dictionary<int, string>();
            this.AsmToSourceLineIndex = new Dictionary<int, int>();
            this.AsmLineToAsmLineIndex = new Dictionary<int, int>();
        }

        public Dissassembly(string text, Dictionary<int, string> lineAddressIndex, Dictionary<int, int> asmToSourceLineIndex, Dictionary<int, int> asmLineToAsmLineIndex)
        {
            this.IsSuccess = true;
            this.Text = text;
            this.AsmLineAddressIndex = lineAddressIndex;
            this.AsmToSourceLineIndex = asmToSourceLineIndex;
            this.AsmLineToAsmLineIndex = asmLineToAsmLineIndex;
        }

        public bool IsSuccess { get; }

        /// <summary>
        /// Index which maps assembly lines to assembly memory address, if there is an address mapping (shown in margin)
        /// </summary>
        public Dictionary<int, string> AsmLineAddressIndex{ get; }

        /// <summary>
        /// Gets an index which maps comment lines in the assembly output to source code lines
        /// </summary>
        public Dictionary<int, int> AsmToSourceLineIndex { get; }

        /// <summary>
        /// Gets and index which maps Asm lines to other Asm lines, e.g. navigate to a label
        /// </summary>
        public Dictionary<int, int> AsmLineToAsmLineIndex { get; }

        public string Text { get; }
    }
}
