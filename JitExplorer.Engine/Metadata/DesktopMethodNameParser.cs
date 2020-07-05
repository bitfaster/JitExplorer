using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine.Metadata
{
    // Parse the dissassebled method names that come from Microsoft.Diagnostics.Runtime.Desktop.DesktopMethod.GetFullSignature()
    public class DesktopMethodNameParser
    {
        public static MethodInfo Parse(string methodFullSignature)
        {
            int firstParan = methodFullSignature.IndexOf('(', StringComparison.Ordinal);

            var stem2 = methodFullSignature.AsSpan(0, firstParan);
            int d = stem2.LastIndexOf('.');
            string methodName = methodFullSignature.Substring(d + 1, stem2.Length - d - 1);

            var qn = stem2.Slice(0, d);

            var type = ExtractClassType(qn);
            var args = ExtractArgs(methodFullSignature.Substring(firstParan + 1, methodFullSignature.Length - firstParan - 2));

            return new MethodInfo(methodName, type, args);
        }

        // "SomeType"	
        // "Namespace.SomeType"
        // "Namespace.SomeTypeArray[]"
        // "Namespace.GenericType`1[[Namespace.SomeType, Assembly.Fully.Qualified]]"
        // "Namespace.GenericType`2[[Namespace.SomeType, Assembly.Fully.Qualified],[Namespace.SomeType, Assembly.Fully.Qualified]]"
        private static ClassInfo ExtractClassType(ReadOnlySpan<char> qn)
        {
            List<ClassInfo> genericParams = new List<ClassInfo>();
            int start = qn.IndexOf('[') + 1;
            foreach (var t in ExtractDelimited(qn, start, '[', ']'))
            {
                // nested generic
                if (t.IndexOf('`') != -1)
                {
                    genericParams.Add(ExtractClassType(t));
                }
                else
                {
                    // System.__Canon, System.Private.CoreLib
                    var p = t.Split(',')[0];
                    genericParams.Add(ExtractClassType(p));
                }
            }

            if (genericParams.Count > 0)
            {
                // fixup
                int sq = qn.IndexOf('`');
                qn = qn.Slice(0, sq);
            }

            int d = qn.LastIndexOf('.');

            if (d > 0)
            {
                string ns = qn.Slice(0, d).ToString();
                string typeName = qn.Slice(d + 1, qn.Length - d - 1).ToString();

                return new ClassInfo(ns, typeName, genericParams);
            }

            return new ClassInfo(string.Empty, qn.ToString(), genericParams);
        }

        // "Namespace.SomeType"
        // "Namespace.GenericType`1<Int32>"
        // "Namespace.GenericType`2<Int32,System.__Canon>"
        // "Namespace.GenericType`1<Namespace.GenericType`1<Int32>>"
        private static ClassInfo ExtractArgClassType(ReadOnlySpan<char> typeStr)
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
                typeStr = typeStr.Slice(0, sq);
            }

            int d = typeStr.LastIndexOf('.');

            if (d > 0)
            {
                string ns = typeStr.Slice(0, d).ToString();
                string typeName = typeStr.Slice(d + 1, typeStr.Length - d - 1).ToString();

                return new ClassInfo(ns, typeName, genericParams);
            }

            return new ClassInfo(string.Empty, typeStr.ToString(), genericParams);
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

        // one, two<two, two>, three =>
        // 1. one
        // 2. two<two, two>
        // 3. three
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

        // [one], [two[two]], [three] =>
        // 1. one
        // 2. two[two]
        // 3. three
        public static IEnumerable<string> ExtractDelimited(ReadOnlySpan<char> input, int start, char open, char close)
        {
            var list = new List<string>();
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
                        list.Add(input.Slice(openedAt, i - openedAt).ToString());
                    }

                    openedAt = -1;
                }
            }

            return list;
        }
    }
}
