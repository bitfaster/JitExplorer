using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace JitExplorer.Engine.Compile
{
    public class CompilerOptions : IEquatable<CompilerOptions>
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

        public override bool Equals(object obj)
        {
            return Equals(obj as CompilerOptions);
        }

        public bool Equals(CompilerOptions other)
        {
            return other != null &&
                   OptimizationLevel == other.OptimizationLevel &&
                   LanguageVersion == other.LanguageVersion &&
                   OutputKind == other.OutputKind &&
                   Platform == other.Platform &&
                   AllowUnsafe == other.AllowUnsafe;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(OptimizationLevel, LanguageVersion, OutputKind, Platform, AllowUnsafe);
        }

        public static bool operator ==(CompilerOptions left, CompilerOptions right)
        {
            return EqualityComparer<CompilerOptions>.Default.Equals(left, right);
        }

        public static bool operator !=(CompilerOptions left, CompilerOptions right)
        {
            return !(left == right);
        }
    }
}
