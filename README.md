# JitExplorer

Like [compiler explorer](https://godbolt.org/), but for .NET JIT.

![image](https://user-images.githubusercontent.com/12851828/86087777-dc5a9100-ba59-11ea-8145-cea9971b95d3.png)

Eventual goal is for window on the left to have syntax highlighted C# code.
https://github.com/aelij/RoslynPad
https://github.com/robinrodricks/ScintillaNET.Demo

Window on the right has JIT assembler
This is generated via code copy pasted from BenchmarkDotNet: https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64

Have pull downs to select CLR version, debug/release etc.
