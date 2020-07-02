using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer
{
    public class JitKey : IEquatable<JitKey>
    {
        public JitKey(string sourceCode, OptimizationLevel optimizationLevel, Platform platform, bool useTieredCompilation)
        {
            this.SourceCode = sourceCode;
            this.OptimizationLevel = this.OptimizationLevel;
            this.Platform = platform;
            this.UseTieredCompilation = useTieredCompilation;
        }

        public string SourceCode { get; }

        public OptimizationLevel OptimizationLevel { get; }

        public Platform Platform { get; }

        public bool UseTieredCompilation { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as JitKey);
        }

        public bool Equals(JitKey other)
        {
            return other != null &&
                   SourceCode == other.SourceCode &&
                   OptimizationLevel == other.OptimizationLevel &&
                   Platform == other.Platform &&
                   UseTieredCompilation == other.UseTieredCompilation;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SourceCode, OptimizationLevel, Platform, UseTieredCompilation);
        }

        public static bool operator ==(JitKey left, JitKey right)
        {
            return EqualityComparer<JitKey>.Default.Equals(left, right);
        }

        public static bool operator !=(JitKey left, JitKey right)
        {
            return !(left == right);
        }
    }
}
