using JitExplorer.Engine;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer
{
    public class JitKey : IEquatable<JitKey>
    {
        public JitKey(string sourceCode, OptimizationLevel optimizationLevel, Platform platform, JitMode jitMode)
        {
            this.SourceCode = sourceCode;
            this.OptimizationLevel = this.OptimizationLevel;
            this.Platform = platform;
            this.JitMode = jitMode;
        }

        public string SourceCode { get; }

        public OptimizationLevel OptimizationLevel { get; }

        public Platform Platform { get; }

        public JitMode JitMode { get; }

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
                   JitMode == other.JitMode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SourceCode, OptimizationLevel, Platform, JitMode);
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
