using BitFaster.Caching.Lru;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine.Metadata
{
    public interface IMethodNameProvider
    {
        string GetName(string signature);
    }

    public class ShortMethodNameProvider : IMethodNameProvider
    {
        private Dictionary<string, string> cache = new Dictionary<string, string>(16, StringComparer.Ordinal);

        public string GetName(string signature)
        {
            if (this.cache.TryGetValue(signature, out var methodName))
            {
                return methodName;
            }

            var methodInfo = DesktopMethodNameParser.Parse(signature);
            var shortened = MethodNameFormatter.Short(methodInfo);

            this.cache.Add(signature, shortened);

            return shortened;
        }
    }
}
