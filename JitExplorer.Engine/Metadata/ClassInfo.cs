using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace JitExplorer.Engine.Metadata
{
    // __Canon expl: https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Object.cs#L221
    [DebuggerDisplay("{CompactName()}")]
    public class ClassInfo
    {
        public ClassInfo(string @namespace, string name)
            : this(@namespace, name, Array.Empty<ClassInfo>())
        {
        }

        public ClassInfo(string @namespace, string name, IEnumerable<ClassInfo> genericParams)
            : this(@namespace, name, false, genericParams)
        {
        }

        public ClassInfo(string @namespace, string name, bool isArray, IEnumerable<ClassInfo> genericParams)
        {
            this.Namespace = @namespace;
            this.Name = name;
            this.IsArray = isArray;
            this.GenericParameters = genericParams;
        }

        public string Namespace { get; }

        public string Name { get; }

        public bool IsArray { get; }

        public IEnumerable<ClassInfo> GenericParameters { get; }

        public string CompactName()
        {
            if (this.GenericParameters.Any())
            {
                var sb = new StringBuilder();

                sb.Append($"{Name}<");

                foreach (var g in this.GenericParameters)
                {
                    sb.Append(g.CompactName());
                    sb.Append(", ");
                }

                sb.Remove(sb.Length - 2, 2);
                sb.Append(">");

                if (IsArray)
                {
                    sb.Append("[]");
                }

                return sb.ToString();
            }

            if (IsArray)
            {
                return $"{Name}[]";
            }    

            return $"{Name}";
        }
    }
}
