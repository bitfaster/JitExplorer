using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace JitExplorer.Engine.Metadata
{
    // Dissassebled method names come from Microsoft.Diagnostics.Runtime.Desktop.DesktopMethod
    public class DesktopMethodNameParser
    {
        // "Testing.Program.Main(System.String[])"
        // This will enable the combo box stuff - because then we can group by namespace and type
        public static MethodInfo Parse(string methodFullSignature)
        {
            int firstParan = methodFullSignature.IndexOf('(');

            string stem = methodFullSignature.Substring(0, firstParan);
            int d = stem.LastIndexOf('.');
            string methodName = methodFullSignature.Substring(d + 1, stem.Length - d - 1);

            string qn = stem.Substring(0, d);

            var type = ExtractClassType(qn, 1, '[', ']');
            var args = ExtractArgs(methodFullSignature.Substring(firstParan + 1, methodFullSignature.Length - firstParan - 2));

            return new MethodInfo(methodName, type, args);
        }

        // "Testing.Program"
        // "System.String[]"
        // "System.Int32"
        // "Testing.ConcurrentLru`2[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]]."
        // "Int32"	
        private static ClassInfo ExtractClassType(string qn, int offset, char open, char close)
        {
            List<ClassInfo> genericParams = new List<ClassInfo>();
            int start = Math.Max(0, qn.IndexOf('[') + offset);
            foreach (var t in ExtractDelimited(qn, start, open, close))
            {
                // nested generic
                if (t.IndexOf('`') != -1)
                {
                    genericParams.Add(ExtractClassType(t, offset, open, close));
                }
                else
                {
                    // System.__Canon, System.Private.CoreLib
                    var p = t.Split(',')[0];
                    genericParams.Add(ExtractClassType(p, offset, open, close));
                }
            }    

            if (genericParams.Count > 0)
            {
                // fixup
                int sq = qn.IndexOf('`');
                qn = qn.Substring(0, sq);
            }

            int d = qn.LastIndexOf('.');

            if (d > 0)
            { 
                string ns = qn.Substring(0, d);
                string typeName = qn.Substring(d + 1, qn.Length - d - 1);

                return new ClassInfo(ns, typeName, genericParams);
            }

            return new ClassInfo(string.Empty, qn, genericParams);
        }

        // "Testing.LruPolicy`2<Int32,System.__Canon>"
        private static ClassInfo ExtractArgClassType(string typeStr)
        {
            List<ClassInfo> genericParams = new List<ClassInfo>();

            foreach (var t in ExtractDelimited(typeStr, 0, '<', '>'))
            {
                // nested generic
                if (t.IndexOf('`') != -1)
                {
                    genericParams.Add(ExtractArgClassType(t));
                }
                else
                {
                    // Int32,System.__Canon
                    var typeParams = t.Split(',');

                    foreach (var typeParam in typeParams)
                    {
                        genericParams.Add(ExtractArgClassType(typeParam));
                    }
                }
            }

            if (genericParams.Count > 0)
            {
                // fixup
                int sq = typeStr.IndexOf('`');
                typeStr = typeStr.Substring(0, sq);
            }

            int d = typeStr.LastIndexOf('.');

            if (d > 0)
            {
                string ns = typeStr.Substring(0, d);
                string typeName = typeStr.Substring(d + 1, typeStr.Length - d - 1);

                return new ClassInfo(ns, typeName, genericParams);
            }

            return new ClassInfo(string.Empty, typeStr, genericParams);
        }

        // "System.String[]"
        // System.Collections.Generic.IEqualityComparer`1<Int32>
        private static IEnumerable<ClassInfo> ExtractArgs(string argsString)
        {
            if (string.IsNullOrEmpty(argsString))
            {
                yield break;
            }

            foreach (var arg in TokenizeMethodArgs(argsString))
            {
                yield return ExtractArgClassType(arg);
            }
        }

        // one, two<two, two>, three
        public static IEnumerable<string> TokenizeMethodArgs(string input)
        {
            int count = 0;
            int openedAt = 0;

            for (int i = 0; i< input.Length; i++)
            {
                if (input[i] == '<')
                {
                    count++;
                }

                if(input[i] == '>')
                { 
                    count--; 
                }

                // skip leading spaces
                if (input[i] == ' ' && count == 0)
                {
                    openedAt = i + 1;
                }

                if (input[i] == ',' && count == 0)
                {
                    yield return input.Substring(openedAt, i - openedAt);
                    openedAt = i + 1;
                }
            }

            yield return input.Substring(openedAt, input.Length - openedAt);
        }

        // [one], [two[two]], [three]
        public static IEnumerable<string> ExtractDelimited(string input, int start, char open, char close)
        {
            int count = 0;
            int openedAt = -1;

            for (int i = start; i < input.Length; i++)
            {
                if (input[i] == open)
                {
                    if (openedAt == -1)
                    {
                        openedAt = i + 1;
                    }

                    count++;
                }

                if (input[i] == close)
                {
                    count--;
                }

                if (count == 0 && openedAt != -1)
                {
                    // if it's an array, skip it
                    int len = i - openedAt;

                    if (len > 0)
                    {
                        yield return input.Substring(openedAt, i - openedAt);
                    }

                    openedAt = -1;
                }
            }
        }
    }
}
