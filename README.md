# JitExplorer

Like [Compiler Explorer](https://godbolt.org/), but for .NET JIT.

C# source code is compiled to an executable using [Roslyn](https://github.com/dotnet/roslyn), then executed. While it is executing, and after JIT is complete, [ClrMD](https://github.com/microsoft/clrmd) is attached and the methods are decompiled using [Iced](https://github.com/0xd4d/iced). The code that does this comes from [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64).

The advantage of this approach vs using [JitDasm](https://github.com/0xd4d/JitDasm) is that it supports jitting generics:

![image](https://user-images.githubusercontent.com/12851828/86315174-53b92d80-bbde-11ea-8e24-cdfd52ea6d00.png)

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
https://github.com/icsharpcode/ILSpy
