# JitExplorer

Like [compiler explorer](https://godbolt.org/), but for .NET JIT.

![image](https://user-images.githubusercontent.com/12851828/86090161-71f81f80-ba5e-11ea-9d45-7fbc13bb44cc.png)

# TODO:

- Why is jitted code not release version? Is it related to roslyn compile?
- Async UI, progress bar
- Richer UI, pull down for x86/x64, debug/release, language version, platform, emit ass line nos, tiered compilation
- IsolatedJit Events/status for compile/jit/dissassemble, show in status bar
- Code auto complete, like RoslynPad


# References

https://github.com/aelij/RoslynPad
https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64
