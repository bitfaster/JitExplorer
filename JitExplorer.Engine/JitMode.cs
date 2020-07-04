using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine
{
    [Flags]
    public enum JitMode
    {
        Default = 0,
        Tiered = 1,
        Quick = 2,
        QuickLoop = 4,
        Legacy = 8,
    }
}
