using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JitExplorer.Engine.Metadata
{
    public class MethodNameFormatter
    {
        public static string Short(MethodInfo methodInfo)
        {
            return $"{methodInfo.Type.CompactName()}.{methodInfo.Name}({GenerateArgs(methodInfo.Args)})";
        }

        private static string GenerateArgs(IEnumerable<ClassInfo> args)
        {
            return string.Join(',', args.Select(a => a.CompactName()));
        }
    }
}
