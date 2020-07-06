# JitExplorer

Like [Compiler Explorer](https://godbolt.org/), but for .NET JIT.

C# source code is compiled to an executable using [Roslyn](https://github.com/dotnet/roslyn), then executed. While it is executing, and after JIT is complete, [ClrMD](https://github.com/microsoft/clrmd) is attached and the methods are decompiled using [Iced](https://github.com/0xd4d/iced). The code that does this comes from [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64).

The advantage of this approach vs using [JitDasm](https://github.com/0xd4d/JitDasm) is that it supports dissassembly of generic methods:

![image](https://user-images.githubusercontent.com/12851828/86548694-2d530500-bef2-11ea-8e61-d8ab3e7974c9.png)

# TODO:

- JIT is not consistent in some cases. Attempt to provide some degree of warmup.
- Auto background jit, without button.
- Hide program, just show JitExplorer.JitAndDissassemble.This()
- Navigate from asm assembly names to source code via (line x) insertion. If there is a dictionary of methods to line numbers, can build the same map from Asm. So can click c# as well. 
https://joshvarty.com/2014/07/26/learn-roslyn-now-part-4-csharpsyntaxwalker/
https://stackoverflow.com/questions/51392704/how-to-use-roslyn-to-determine-line-codes-position-in-a-source-file
- MVVM
- Better UI: Asm combos to pick method and type, choose instr format, depth
- Double click error to navigate to source
- UI bugs: fix toggle button style
- Different TextEditor for error messages with proper highlighting
- Full code auto complete, like RoslynPad
- Copy as markdown for C# & Asm


# References

https://github.com/aelij/RoslynPad
https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64
https://github.com/icsharpcode/ILSpy
