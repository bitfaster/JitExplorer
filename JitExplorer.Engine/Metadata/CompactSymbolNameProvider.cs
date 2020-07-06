using JitExplorer.Engine.Disassemble;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine.Metadata
{
    public class CompactSymbolNameProvider : ISymbolNameProvider
    {
        public string TranslateMethodTable(string methodTable)
        {
            var classInfo = DesktopMethodNameParser.ExtractClassType(methodTable);
            return classInfo.CompactName();
        }

        public string TranslateSignature(string methodSignature)
        {
            var methodInfo = DesktopMethodNameParser.Parse(methodSignature);
            return MethodNameFormatter.Short(methodInfo);
        }
    }
}
