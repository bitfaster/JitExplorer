# JitExplorer

Like [compiler explorer](https://godbolt.org/), but for .NET JIT.

![image](https://user-images.githubusercontent.com/12851828/86090161-71f81f80-ba5e-11ea-9d45-7fbc13bb44cc.png)

Eventual goal is for window on the left to have syntax completing C# code.
https://github.com/aelij/RoslynPad
https://github.com/robinrodricks/ScintillaNET.Demo

Window on the right has JIT assembler
This is generated via code copy pasted from BenchmarkDotNet: https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64

Have pull downs to select CLR version, debug/release etc.
