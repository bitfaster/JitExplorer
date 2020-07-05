using JitExplorer.Engine.Compile;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine
{
    public class Config : IEquatable<Config>
    {
        public CompilerOptions CompilerOptions { get; set; }

        public JitMode JitMode { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Config);
        }

        public bool Equals(Config other)
        {
            return other != null &&
                   EqualityComparer<CompilerOptions>.Default.Equals(CompilerOptions, other.CompilerOptions) &&
                   JitMode == other.JitMode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CompilerOptions, JitMode);
        }

        public static bool operator ==(Config left, Config right)
        {
            return EqualityComparer<Config>.Default.Equals(left, right);
        }

        public static bool operator !=(Config left, Config right)
        {
            return !(left == right);
        }
    }
}
