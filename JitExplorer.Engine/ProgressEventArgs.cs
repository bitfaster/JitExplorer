using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine
{
    public class ProgressEventArgs : EventArgs
    {
        public string StatusMessage { get; set; }
    }
}
