using System;
using System.Collections.Generic;
using System.Linq;
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

        private static readonly string plusChar = "+";

        // "SomeType"	
        // "Namespace.SomeType"
        // "Namespace.SomeTypeArray[]"
        // "Namespace.GenericType`1[[Namespace.SomeType, Assembly.Fully.Qualified]]"
        // "Namespace.GenericType`2[[Namespace.SomeType, Assembly.Fully.Qualified],[Namespace.SomeType, Assembly.Fully.Qualified]]"
        public static ClassInfo ExtractClassType(ReadOnlySpan<char> classString)
        {
            List<ClassInfo> genericParams = new List<ClassInfo>();
            int start = classString.IndexOf('[') + 1;
            foreach (var c in ExtractDelimited(classString, start, '[', ']'))
            {
                // nested generic
                if (c.IndexOf('`') != -1)
                {
                    genericParams.Add(ExtractClassType(c));
                }
                else
                {
                    // System.__Canon, System.Private.CoreLib
                    var p = c.Split(',')[0];
                    genericParams.Add(ExtractClassType(p));
                }
            }

            if (genericParams.Count > 0)
            {
                ReadOnlySpan<char> nested = ReadOnlySpan<char>.Empty;
                int plus = classString.IndexOf('+');
                if (plus != -1 && plus < start)
                {
                    // this is a nested class
                    nested = classString.Slice(plus + 1, start - plus - 2);
                    nested = plusChar.AsSpan().Concat(nested);
                }

                // fixup
                int backtick = classString.IndexOf('`');
                classString = classString.Slice(0, backtick);

                classString = classString.Concat(nested);
            }

            int dot = classString.LastIndexOf('.');

            if (dot > 0)
            {
                string ns = classString.Slice(0, dot).ToString();
                string typeName = classString.Slice(dot + 1, classString.Length - dot - 1).ToString();

                return new ClassInfo(ns, typeName, genericParams);
            }

            return new ClassInfo(string.Empty, classString.ToString(), genericParams);
        }

        // "Namespace.SomeType"
        // "Namespace.SomeType[]"
        // "Namespace.GenericType`1<Int32>"
        // "Namespace.GenericType`2<Int32,System.__Canon>"
        // "Namespace.GenericType`1<Namespace.GenericType`1<Int32>>"
        // "Node<Int32,Int32>[]"
        public static ClassInfo ExtractArgClassType(ReadOnlySpan<char> classString)
        {
            List<ClassInfo> genericParams = new List<ClassInfo>();

            foreach (var c in ExtractDelimited(classString, 0, '<', '>'))
            {
                // nested generic
                if (c.IndexOf('`') != -1)
                {
                    genericParams.Add(ExtractArgClassType(c));
                }
                else
                {
                    // Int32,System.__Canon
                    var typeParams = c.Split(',');

                    foreach (var typeParam in typeParams)
                    {
                        genericParams.Add(ExtractArgClassType(typeParam));
                    }
                }
            }

            bool isArray = classString[classString.Length - 2] == '[' && classString[classString.Length - 1] == ']';

            if (genericParams.Count > 0)
            {
                // fixup
                int backtick = classString.IndexOf('`');

                if (backtick == -1)
                {
                    backtick = classString.IndexOf('<');
                }

                classString = classString.Slice(0, backtick);
            }
            else if (isArray)
            {
                // we are going to mark as IsArray, so chop the chars off
                classString = classString.Slice(0, classString.Length - 2);
            }

            int dot = classString.LastIndexOf('.');

            if (dot > 0)
            {
                string ns = classString.Slice(0, dot).ToString();
                string typeName = classString.Slice(dot + 1, classString.Length - dot - 1).ToString();

                return new ClassInfo(ns, typeName, isArray, genericParams);
            }

            return new ClassInfo(string.Empty, classString.ToString(), isArray, genericParams);
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

        // Possible optimization
        // https://gist.github.com/LordJZ/92b7decebe52178a445a0b82f63e585a
        // one, two<two>, three<three, three> =>
        // 1. one
        // 2. two<two>
        // 3. three<three, three>
        public static IEnumerable<string> TokenizeMethodArgs(ReadOnlySpan<char> input)
        {
            var list = new List<string>();

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
                    // if next chars are !" ByRef"
                    int s = i;
                    int e = Math.Min(input.Length - i, i + 6);
                    var next = input.Slice(s, e);

                    ReadOnlySpan<char> byref = " ByRef";

                    if (!next.Equals(byref, StringComparison.Ordinal))
                    {
                        openedAt = i + 1;
                    }
                }

                if (input[i] == ',' && count == 0)
                {
                    list.Add(input.Slice(openedAt, i - openedAt).ToString());
                    openedAt = i + 1;
                }
            }

            list.Add(input.Slice(openedAt, input.Length - openedAt).ToString());

            return list;
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
