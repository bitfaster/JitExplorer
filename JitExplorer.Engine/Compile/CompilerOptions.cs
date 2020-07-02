using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace JitExplorer.Engine.Compile
{
    public class CompilerOptions
    {
        public CompilerOptions()
        {
            this.OptimizationLevel = OptimizationLevel.Release;
            this.LanguageVersion = LanguageVersion.Default;
            this.OutputKind = OutputKind.DynamicallyLinkedLibrary;
            this.Platform = Platform.AnyCpu;
            this.AllowUnsafe = false;
        }

        public OptimizationLevel OptimizationLevel { get; set; }

        // Does this control framework version?
        public LanguageVersion LanguageVersion { get; set; }

        public OutputKind OutputKind { get; set; }

        public Platform Platform { get; set; }

        public bool AllowUnsafe { get; set; }
    }
}
