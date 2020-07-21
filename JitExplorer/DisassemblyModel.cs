using JitExplorer.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace JitExplorer
{
    public class DisassemblyModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Disassembly disassembly;

        public DisassemblyModel(Disassembly disassembly)
        {
            this.disassembly = disassembly;

            
        }

        public string AsmText
        {
            get { return this.disassembly.AsmText; }
        }

        public string OutputText
        {
            get { return this.disassembly.OutputText; }
        }

        public Dictionary<int, string> AsmLineAddressIndex
        {
            get { return this.disassembly.AsmLineAddressIndex; }
        }

        public Dictionary<int, int> AsmToSourceLineIndex
        {
            get { return this.disassembly.AsmToSourceLineIndex; }
        }

        public Dictionary<int, int> AsmLineToAsmLineIndex
        {
            get { return this.disassembly.AsmLineToAsmLineIndex; }
        }
    }
}
