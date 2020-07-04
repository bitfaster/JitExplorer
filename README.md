# JitExplorer

Like [Compiler Explorer](https://godbolt.org/), but for .NET JIT.

C# source code is compiled to an executable using [Roslyn](https://github.com/dotnet/roslyn), then executed. While it is executing, and after JIT is complete, [ClrMD](https://github.com/microsoft/clrmd) is attached and the methods are decompiled using [Iced](https://github.com/0xd4d/iced). The code that does this comes from [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64).

The advantage of this approach vs using [JitDasm](https://github.com/0xd4d/JitDasm) is that it supports dissassembly of generic methods:

![image](https://user-images.githubusercontent.com/12851828/86521372-c3106680-be04-11ea-90bd-81aead515b07.png)

# TODO:

- JIT is not consistent in some cases. Attempt to provide some degree of warmup.
- Auto background jit, without button.
- Hide program, just show JitExplorer.JitAndDissassemble.This()
- Better UI: Asm combos to pick method and type, emit assembly addr nos, choose instr format, depth
- Short names in asm
- UI bugs: fix toggle button style
- Different TextEditor for error messages with propery highlighting
- Full code auto complete, like RoslynPad
- Hyperlinks for navigation
- Copy as markdown for C# & Asm


# References

https://github.com/aelij/RoslynPad
https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64
https://github.com/icsharpcode/ILSpy
