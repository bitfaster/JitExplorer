using JitExplorer.Engine;
using JitExplorer.Engine.Compile;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer
{
    public class JitKey : IEquatable<JitKey>
    {
        public JitKey(string sourceCode, CompilerOptions compilerOptions, JitMode jitMode)
        {
            this.SourceCode = sourceCode;
            this.CompilerOptions = compilerOptions;
            this.JitMode = jitMode;
        }

        public string SourceCode { get; }

        public CompilerOptions CompilerOptions { get; }

        public JitMode JitMode { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as JitKey);
        }

        public bool Equals(JitKey other)
        {
            return other != null &&
                   SourceCode == other.SourceCode &&
                   EqualityComparer<CompilerOptions>.Default.Equals(CompilerOptions, other.CompilerOptions) &&
                   JitMode == other.JitMode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SourceCode, CompilerOptions, JitMode);
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
