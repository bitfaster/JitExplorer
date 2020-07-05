using FluentAssertions;
using JitExplorer.Engine.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace JitExplorer.Engine.UnitTests.Metadata
{
    public class DesktopMethodNameParserTests
    {

        // "Testing.ConcurrentLru`2[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]]..ctor(Int32)"
        // "Testing.TemplateConcurrentLru`5[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib],[Testing.LruPolicy`2[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]], test.exe],[Testing.HitCounter, test.exe]].TryGet(Int32, System.__Canon ByRef)"
        // "Testing.TemplateConcurrentLru`5[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib],[Testing.LruPolicy`2[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]], test.exe],[Testing.HitCounter, test.exe]]..ctor(Int32, Int32, System.Collections.Generic.IEqualityComparer`1<Int32>, Testing.LruPolicy`2<Int32,System.__Canon>, Testing.HitCounter)"
        // "System.Collections.Concurrent.ConcurrentDictionary`2[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]].TryGetValue(Int32, System.__Canon ByRef)"
        // "System.Collections.Concurrent.ConcurrentQueue`1[[System.__Canon, System.Private.CoreLib]]..ctor()"
        // "System.Collections.Concurrent.ConcurrentDictionary`2[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]]..ctor(Int32, Int32, Boolean, System.Collections.Generic.IEqualityComparer`1<Int32>)"

        // "Testing.Program.Main()"  
        [Fact]
        public void MethodNoArgs()
        {
            string methodName = "Testing.Program.Main()";

            var methodInfo = DesktopMethodNameParser.Parse(methodName);

            methodInfo.Type.Namespace.Should().Be("Testing");
            methodInfo.Type.Name.Should().Be("Program");
            methodInfo.Name.Should().Be("Main");
            methodInfo.Args.Should().BeEmpty();
        }


        // "Testing.Program.Main(System.String[])"  
        [Fact]
        public void Method1Arg()
        {
            string methodName = "Testing.Program.Main(System.String[])";

            var methodInfo = DesktopMethodNameParser.Parse(methodName);

            methodInfo.Type.Namespace.Should().Be("Testing");
            methodInfo.Type.Name.Should().Be("Program");
            methodInfo.Name.Should().Be("Main");

            methodInfo.Args.Should().HaveCount(1);
            methodInfo.Args.First().Name.Should().Be("String[]");
            methodInfo.Args.First().Namespace.Should().Be("System");
        }

        // "Testing.ConcurrentLru`2[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]]..ctor(Int32)"
        [Fact]
        public void GenericCtorTwo()
        {
            string methodName = "Testing.ConcurrentLru`2[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]]..ctor(Int32)";

            var methodInfo = DesktopMethodNameParser.Parse(methodName);

            methodInfo.Type.Namespace.Should().Be("Testing");
            methodInfo.Type.Name.Should().Be("ConcurrentLru");
            methodInfo.Type.GenericParameters.Should().HaveCount(2);
            methodInfo.Type.GenericParameters.First().Name.Should().Be("Int32");
            methodInfo.Type.GenericParameters.First().Namespace.Should().Be("System");
            methodInfo.Type.GenericParameters.Last().Name.Should().Be("__Canon");
            methodInfo.Type.GenericParameters.Last().Namespace.Should().Be("System");
            methodInfo.Name.Should().Be("ctor");

            methodInfo.Args.Should().HaveCount(1);
            methodInfo.Args.First().Name.Should().Be("Int32");
            methodInfo.Args.First().Namespace.Should().Be(string.Empty);
        }

        // "Testing.TemplateConcurrentLru`5[
        // [System.Int32, System.Private.CoreLib],
        // [System.__Canon, System.Private.CoreLib],
        // [System.__Canon, System.Private.CoreLib],
        // - [Testing.LruPolicy`2[[System.Int32, System.Private.CoreLib], [System.__Canon, System.Private.CoreLib]], test.exe],
        // [Testing.HitCounter, test.exe]]..ctor(Int32, Int32, System.Collections.Generic.IEqualityComparer`1<Int32>, Testing.LruPolicy`2<Int32,System.__Canon>, Testing.HitCounter)"
        [Fact]
        public void GenericCtorNestedGeneric()
        {
            string methodName = "Testing.TemplateConcurrentLru`5[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib],[Testing.LruPolicy`2[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]], test.exe],[Testing.HitCounter, test.exe]]..ctor(Int32, Int32, System.Collections.Generic.IEqualityComparer`1<Int32>, Testing.LruPolicy`2<Int32,System.__Canon>, Testing.HitCounter)";

            var methodInfo = DesktopMethodNameParser.Parse(methodName);

            methodInfo.Type.Namespace.Should().Be("Testing");
            methodInfo.Type.Name.Should().Be("TemplateConcurrentLru");
            methodInfo.Type.GenericParameters.Should().HaveCount(5);

            // 0
            methodInfo.Type.GenericParameters.First().Name.Should().Be("Int32");
            methodInfo.Type.GenericParameters.First().Namespace.Should().Be("System");

            // 1
            methodInfo.Type.GenericParameters.Skip(1).First().Name.Should().Be("__Canon");
            methodInfo.Type.GenericParameters.Skip(1).First().Namespace.Should().Be("System");

            // 2
            methodInfo.Type.GenericParameters.Skip(2).First().Name.Should().Be("__Canon");
            methodInfo.Type.GenericParameters.Skip(2).First().Namespace.Should().Be("System");

            // 3
            methodInfo.Type.GenericParameters.Skip(3).First().Name.Should().Be("LruPolicy");
            methodInfo.Type.GenericParameters.Skip(3).First().Namespace.Should().Be("Testing");
            methodInfo.Type.GenericParameters.Skip(3).First().GenericParameters.Should().HaveCount(2);
            methodInfo.Type.GenericParameters.Skip(3).First().GenericParameters.First().Namespace.Should().Be("System");
            methodInfo.Type.GenericParameters.Skip(3).First().GenericParameters.First().Name.Should().Be("Int32");
            methodInfo.Type.GenericParameters.Skip(3).First().GenericParameters.Last().Namespace.Should().Be("System");
            methodInfo.Type.GenericParameters.Skip(3).First().GenericParameters.Last().Name.Should().Be("__Canon");

            // 4
            methodInfo.Type.GenericParameters.Skip(4).First().Name.Should().Be("HitCounter");
            methodInfo.Type.GenericParameters.Skip(4).First().Namespace.Should().Be("Testing");

            methodInfo.Name.Should().Be("ctor");
        }

        [Fact]
        public void SingleGenericArg()
        {
            string methodName = "Testing.Foo.ctor(System.Collections.Generic.IEqualityComparer`1<Int32>)";

            var methodInfo = DesktopMethodNameParser.Parse(methodName);

            var args = methodInfo.Args.ToArray();
            args.Length.Should().Be(1);

            args[0].Name.Should().Be("IEqualityComparer");
            args[0].Namespace.Should().Be("System.Collections.Generic");
            
            var gen = args[0].GenericParameters.ToArray();

            gen.Should().HaveCount(1);
            gen[0].Name.Should().Be("Int32");
            gen[0].Namespace.Should().Be(string.Empty);
        }


        [Fact]
        public void SingleGenericArgWithTwoTypeParams()
        {
            string methodName = "Testing.Foo.ctor(Testing.LruPolicy`2<Int32,System.__Canon>)";

            var methodInfo = DesktopMethodNameParser.Parse(methodName);

            var args = methodInfo.Args.ToArray();
            args.Length.Should().Be(1);

            // This is returning __Canon
            args[0].Name.Should().Be("LruPolicy");
            args[0].Namespace.Should().Be("Testing");

            var gen = args[0].GenericParameters.ToArray();

            gen.Should().HaveCount(2);
            gen[0].Name.Should().Be("Int32");
            gen[0].Namespace.Should().Be(string.Empty);
            gen[1].Name.Should().Be("__Canon");
            gen[1].Namespace.Should().Be("System");
        }

        //[Fact]
        //public void GenericArgs()
        //{
        //    string methodName = "Testing.Foo.ctor(Int32, System.Collections.Generic.IEqualityComparer`1<Int32>, Testing.LruPolicy`2<Int32,System.__Canon>, Testing.HitCounter)";

        //    var methodInfo = DesktopMethodNameParser.Parse(methodName);

        //    var args = methodInfo.Args.ToArray();

        //    args.Length.Should().Be(4);

        //    args[0].Name.Should().Be("Int32");
        //    args[0].Namespace.Should().Be(string.Empty);

        //    args[1].Name.Should().Be("IEqualityComparer`1<Int32>");
        //    args[1].Namespace.Should().Be("System.Collections.Generic");
        //    args[1].GenericParameters.Should().HaveCount(1);
        //    args[1].GenericParameters.First().Should().Be("Int32");
        //}

        [Fact]
        public void ExtractTypesNested()
        {
            string test = "[one], [two[two]], [three]";

            var r = DesktopMethodNameParser.ExtractDelimited(test, 0, '[', ']');

            var ra = r.ToArray();

            ra.Length.Should().Be(3);
            ra[0].Should().Be("one");
            ra[1].Should().Be("two[two]");
            ra[2].Should().Be("three");
        }

        [Fact]
        public void ExtractTypesArray()
        {
            string test = "string[]";

            var r = DesktopMethodNameParser.ExtractDelimited(test, 0, '[', ']');

            r.Should().BeEmpty();
        }

        [Fact]
        public void ExtractTypesFromNamed()
        {
            string test = "SomeClass`3[[one], [two[two]], [three]].";

            int start = test.IndexOf('[') + 1;
            var r = DesktopMethodNameParser.ExtractDelimited(test, start, '[', ']');

            var ra = r.ToArray();

            ra.Length.Should().Be(3);
            ra[0].Should().Be("one");
            ra[1].Should().Be("two[two]");
            ra[2].Should().Be("three");
        }

        [Fact]
        public void TokenizeMethodArgs()
        {
            string test = "one, two<two, two>, three";

            var r = DesktopMethodNameParser.TokenizeMethodArgs(test);

            var ra = r.ToArray();

            ra.Length.Should().Be(3);
            ra[0].Should().Be("one");
            ra[1].Should().Be("two<two, two>");
            ra[2].Should().Be("three");
        }
    }
}
