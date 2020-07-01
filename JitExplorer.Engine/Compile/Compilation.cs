using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JitExplorer.Engine.Compile
{
    public class Compilation : IDisposable
    {
        public readonly MemoryStream programExecutable;
        public readonly Message[] Messages;

        public bool Succeeded => programExecutable.Length > 0;

        public Compilation(MemoryStream programExecutable, Message[] messages)
        {
            this.programExecutable = programExecutable;
            Messages = messages;
        }

        public void Dispose()
        {
            this.programExecutable.Dispose();
        }
    }
}
