using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JitExplorer.Engine.Compile
{
    public class Compilation : IDisposable
    {
        private readonly Stream assembly;
        private readonly Stream pdb;
        private readonly CompileDiagnostic[] diagnostics;

        public bool Succeeded => Assembly.Length > 0;

        public Stream Assembly => this.assembly;

        public Stream Pdb => this.pdb;

        public IEnumerable<CompileDiagnostic> Diagnostics => this.diagnostics;

        public Compilation(Stream assembly, Stream pdb, CompileDiagnostic[] diagnostics)
        {
            this.assembly = assembly;
            this.pdb = pdb;
            this.diagnostics = diagnostics;
        }

        public void Dispose()
        {
            this.Assembly.Dispose();
            this.pdb?.Dispose();
        }
    }
}
