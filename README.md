# JitExplorer

Like [compiler explorer](https://godbolt.org/), but for .NET JIT.

![image](https://user-images.githubusercontent.com/12851828/86214409-03839200-bb30-11ea-92d2-9b50af60df5c.png)

# TODO:

- Why is jitted code not release version? Is it related to roslyn compile?
- Async UI, progress bar
- Richer UI, pull down for x86/x64, debug/release, language version, platform, emit ass line nos, tiered compilation
- IsolatedJit Events/status for compile/jit/dissassemble, show in status bar
https://www.wpf-tutorial.com/common-interface-controls/statusbar-control/
- Code auto complete, like RoslynPad


# References

https://github.com/aelij/RoslynPad
https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64
