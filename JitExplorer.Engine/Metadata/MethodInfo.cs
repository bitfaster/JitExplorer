using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JitExplorer.Engine.Metadata
{
    public class MethodInfo
    {
        public MethodInfo(string name, ClassInfo type, IEnumerable<ClassInfo> args)
        {
            this.Name = name;
            this.Type = type;
            this.Args = args;
        }

        public string Name { get; }

        public ClassInfo Type { get; }

        public IEnumerable<ClassInfo> Args { get; }

        public string ToCallSite()
        {
            if (!this.Args.Any())
            { return $"{this.Type.Namespace}.{this.Type.Name}.{this.Name}()"; }
            return string.Empty;
        }
    }
}
