using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine
{
    public class Config
    {
        public OptimizationLevel OptimizationLevel { get; set; }

        public Platform Platform { get; set; }

        public JitMode JitMode { get; set; }
    }
}
