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
        [Fact]
        public void MethodNoArgs()
        {
            string methodName = "Namespace.Class.Method()";

            var methodInfo = DesktopMethodNameParser.Parse(methodName);

            methodInfo.Name.Should().Be("Method");
            methodInfo.Type.Namespace.Should().Be("Namespace");
            methodInfo.Type.Name.Should().Be("Class");
            
            methodInfo.Args.Should().BeEmpty();
        }

        [Fact]
        public void MethodArrayArg()
        {
            string methodName = "Namespace.Class.Method(Namespace2.String[])";

            var methodInfo = DesktopMethodNameParser.Parse(methodName);

            methodInfo.Name.Should().Be("Method");
            methodInfo.Type.Namespace.Should().Be("Namespace");
            methodInfo.Type.Name.Should().Be("Class");
            
            methodInfo.Args.Should().HaveCount(1);
            methodInfo.Args.First().Name.Should().Be("String[]");
            methodInfo.Args.First().Namespace.Should().Be("Namespace2");
        }

        [Fact]
        public void MethodRefArg()
        {
            string methodName = "Namespace.Class.Method(Int32 ByRef)";

            var methodInfo = DesktopMethodNameParser.Parse(methodName);

            methodInfo.Name.Should().Be("Method");
            methodInfo.Type.Namespace.Should().Be("Namespace");
            methodInfo.Type.Name.Should().Be("Class");

            methodInfo.Args.Should().HaveCount(1);
            methodInfo.Args.First().Name.Should().Be("Int32 ByRef");
            methodInfo.Args.First().Namespace.Should().Be(string.Empty);
        }

        [Fact]
        public void GenericCtor()
        {
            string methodName = "Namespace.GenericType`1[[System.Int32, System.Private.CoreLib]]..ctor(Int32)";

            var methodInfo = DesktopMethodNameParser.Parse(methodName);

            methodInfo.Name.Should().Be("ctor");

            methodInfo.Type.Namespace.Should().Be("Namespace");
            methodInfo.Type.Name.Should().Be("GenericType");

            methodInfo.Type.GenericParameters.Should().HaveCount(1);
            methodInfo.Type.GenericParameters.First().Name.Should().Be("Int32");
            methodInfo.Type.GenericParameters.First().Namespace.Should().Be("System");

            methodInfo.Args.Should().HaveCount(1);
            methodInfo.Args.First().Name.Should().Be("Int32");
            methodInfo.Args.First().Namespace.Should().Be(string.Empty);
        }

        [Fact]
        public void GenericCtorTwo()
        {
            string methodName = "Namespace.GenericType`2[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]]..ctor(Int32)";

            var methodInfo = DesktopMethodNameParser.Parse(methodName);

            methodInfo.Name.Should().Be("ctor");

            methodInfo.Type.Namespace.Should().Be("Namespace");
            methodInfo.Type.Name.Should().Be("GenericType");

            methodInfo.Type.GenericParameters.Should().HaveCount(2);
            methodInfo.Type.GenericParameters.First().Name.Should().Be("Int32");
            methodInfo.Type.GenericParameters.First().Namespace.Should().Be("System");
            methodInfo.Type.GenericParameters.Last().Name.Should().Be("__Canon");
            methodInfo.Type.GenericParameters.Last().Namespace.Should().Be("System");
            
            methodInfo.Args.Should().HaveCount(1);
            methodInfo.Args.First().Name.Should().Be("Int32");
            methodInfo.Args.First().Namespace.Should().Be(string.Empty);
        }

        [Fact]
        public void GenericCtorNestedGeneric()
        {
            string methodName = "Namespace.GenericType`5[[System.Int32, System.Private.CoreLib],[Namespace2.NestedGeneric`2[[System.Int32, System.Private.CoreLib],[System.__Canon, System.Private.CoreLib]], test.exe],[Namespace3.SomeType, test.exe]]..ctor(Int32, Int32, System.Collections.Generic.IEqualityComparer`1<Int32>, Testing.LruPolicy`2<Int32,System.__Canon>, Testing.HitCounter)";

            var methodInfo = DesktopMethodNameParser.Parse(methodName);

            methodInfo.Type.Namespace.Should().Be("Namespace");
            methodInfo.Type.Name.Should().Be("GenericType");
            methodInfo.Type.GenericParameters.Should().HaveCount(3);

            // 0
            methodInfo.Type.GenericParameters.First().Name.Should().Be("Int32");
            methodInfo.Type.GenericParameters.First().Namespace.Should().Be("System");

            // 1
            methodInfo.Type.GenericParameters.Skip(1).First().Name.Should().Be("NestedGeneric");
            methodInfo.Type.GenericParameters.Skip(1).First().Namespace.Should().Be("Namespace2");
            methodInfo.Type.GenericParameters.Skip(1).First().GenericParameters.Should().HaveCount(2);
            methodInfo.Type.GenericParameters.Skip(1).First().GenericParameters.First().Namespace.Should().Be("System");
            methodInfo.Type.GenericParameters.Skip(1).First().GenericParameters.First().Name.Should().Be("Int32");
            methodInfo.Type.GenericParameters.Skip(1).First().GenericParameters.Last().Namespace.Should().Be("System");
            methodInfo.Type.GenericParameters.Skip(1).First().GenericParameters.Last().Name.Should().Be("__Canon");

            // 2
            methodInfo.Type.GenericParameters.Skip(2).First().Name.Should().Be("SomeType");
            methodInfo.Type.GenericParameters.Skip(2).First().Namespace.Should().Be("Namespace3");

            methodInfo.Name.Should().Be("ctor");
        }

        [Fact]
        public void SingleGenericArg()
        {
            string methodName = "Namespace.Foo.ctor(System.Collections.Generic.IEqualityComparer`1<Int32>)";

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
            string methodName = "Namespace.Foo.ctor(Namespace2.SomeGeneric`2<Int32,System.__Canon>)";

            var methodInfo = DesktopMethodNameParser.Parse(methodName);

            var args = methodInfo.Args.ToArray();
            args.Length.Should().Be(1);

            args[0].Name.Should().Be("SomeGeneric");
            args[0].Namespace.Should().Be("Namespace2");

            var gen = args[0].GenericParameters.ToArray();

            gen.Should().HaveCount(2);
            gen[0].Name.Should().Be("Int32");
            gen[0].Namespace.Should().Be(string.Empty);
            gen[1].Name.Should().Be("__Canon");
            gen[1].Namespace.Should().Be("System");
        }

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

        [Fact]
        public void TokenizeMethodArgsByRef()
        {
            string test = "Int32 ByRef";

            var r = DesktopMethodNameParser.TokenizeMethodArgs(test);

            var ra = r.ToArray();

            ra.Length.Should().Be(1);
            ra[0].Should().Be(test);
        }
    }
}
