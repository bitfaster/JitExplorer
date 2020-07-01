# JitExplorer

Like [compiler explorer](https://godbolt.org/), but for .NET JIT.

![image](https://user-images.githubusercontent.com/12851828/86214409-03839200-bb30-11ea-92d2-9b50af60df5c.png)

# TODO:

- Why is jitted code not release version? Is it related to roslyn compile?
- Background jit, without button
- Better UI, pull down for language version, emit ass line nos, all jit compilation options etc.
- Better status bar, progress spinner
https://stackoverflow.com/questions/6359848/wpf-loading-spinner
https://www.wpf-tutorial.com/common-interface-controls/statusbar-control/
- Code auto complete, like RoslynPad


# References

https://github.com/aelij/RoslynPad
https://github.com/dotnet/BenchmarkDotNet/tree/master/src/BenchmarkDotNet.Disassembler.x64
