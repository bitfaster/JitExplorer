using JitExplorer.Engine.Disassemble;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine.Metadata
{
    public class CompactSymbolNameProvider : ISymbolNameProvider
    {
        // TranslateMethodTable:
        // Testing.ConcurrentLru`2[[System.Int32, System.Private.CoreLib],[System.String, System.Private.CoreLib]]
        public string TranslateMethodTable(string methodTable)
        {
            var classInfo = DesktopMethodNameParser.ExtractClassType(methodTable);
            return classInfo.CompactName();
        }

        public string TranslateSignature(string methodSignature)
        {
            try
            {
                var methodInfo = DesktopMethodNameParser.Parse(methodSignature);
                return MethodNameFormatter.Short(methodInfo);
            }
            catch (Exception ex)
            {
                return methodSignature;
            }
        }
    }
}
