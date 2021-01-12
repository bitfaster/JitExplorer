using JitExplorer.Engine;
using JitExplorer.Engine.Compile;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Commands
{
    public class JitKey : IEquatable<JitKey>
    {
        public JitKey(string sourceCode, Config config)
        {
            this.SourceCode = sourceCode;
            this.Config = config;
        }

        public string SourceCode { get; }

        public Config Config { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as JitKey);
        }

        public bool Equals(JitKey other)
        {
            return other != null &&
                   SourceCode == other.SourceCode &&
                   EqualityComparer<Config>.Default.Equals(Config, other.Config);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SourceCode, Config);
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
