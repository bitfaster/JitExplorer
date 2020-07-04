using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine
{
    public class Config
    {
        public LanguageVersion LanguageVersion { get; set; }

        public OptimizationLevel OptimizationLevel { get; set; }

        public Platform Platform { get; set; }

        public JitMode JitMode { get; set; }
    }
}
