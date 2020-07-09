# JitExplorer

Like [Compiler Explorer](https://godbolt.org/), but for .NET JIT.

![.NET Core](https://github.com/bitfaster/JitExplorer/workflows/.NET%20Core/badge.svg) ![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/bitfaster/JitExplorer)

C# source code is compiled to an executable using [Roslyn](https://github.com/dotnet/roslyn), then executed. While it is executing, and after JIT is complete, [ClrMD](https://github.com/microsoft/clrmd) is attached and the methods are decompiled using [Iced](https://github.com/0xd4d/iced). The code that does this comes from [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64).

The advantage of this approach vs using [JitDasm](https://github.com/0xd4d/JitDasm) is that it supports dissassembly of generic methods:

![image](https://user-images.githubusercontent.com/12851828/87002460-829b4a80-c16e-11ea-93dd-7a0712682b30.png)

# References

https://github.com/aelij/RoslynPad
https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64
https://github.com/icsharpcode/ILSpy
