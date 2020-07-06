using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine.Disassemble
{
    public interface ISymbolNameProvider
    {
        string TranslateMethodTable(string methodTable);

        string TranslateSignature(string methodSignature);
    }
}
