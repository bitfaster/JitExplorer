# JitExplorer

Like [compiler explorer](https://godbolt.org/), but for .NET JIT.

Window on the left has c# code
https://github.com/robinrodricks/ScintillaNET.Demo

Window on the right has JIT assembler
Will need to compile the c# code then disassemble. Can use same method as benchmarkdotnet via iced.
Can probably re-use this: https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64

Have pull downs to select CLR version, debug/release etc.
