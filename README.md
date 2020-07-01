# JitExplorer

Like [Compiler Explorer](https://godbolt.org/), but for .NET JIT.

Supports generics:

![image](https://user-images.githubusercontent.com/12851828/86298224-1a6ac880-bbb2-11ea-90e4-7bd5114284e9.png)

# TODO:

- Why is jitted code not release version matching benchmarkdotnet? Is it related to roslyn compile?
- Background jit, without button
- Better UI, pull down for language version, emit ass line nos, all jit compilation options etc.
- Better status bar, progress spinner
https://stackoverflow.com/questions/6359848/wpf-loading-spinner
https://www.wpf-tutorial.com/common-interface-controls/statusbar-control/
- Code auto complete, like RoslynPad


# References

https://github.com/aelij/RoslynPad
https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64
