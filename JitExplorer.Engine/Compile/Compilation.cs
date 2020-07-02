using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JitExplorer.Engine.Compile
{
    public class Compilation : IDisposable
    {
        private readonly MemoryStream assembly;
        private readonly CompileDiagnostic[] diagnostics;

        public bool Succeeded => Assembly.Length > 0;

        public MemoryStream Assembly => this.assembly;

        public IEnumerable<CompileDiagnostic> Diagnostics => this.diagnostics;

        public Compilation(MemoryStream assembly, CompileDiagnostic[] diagnostics)
        {
            this.assembly = assembly;
            this.diagnostics = diagnostics;
        }

        public void Dispose()
        {
            this.Assembly.Dispose();
        }
    }
}
