using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine.Disassemble
{
    public class DefaultSymbolNameProvider : ISymbolNameProvider
    {
        public string TranslateMethodTable(string methodTable)
        {
            return methodTable;
        }

        public string TranslateSignature(string methodSignature)
        {
            return methodSignature;
        }
    }
}
