using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine.Compile
{
    public class ParsedTree
    {
        public ParsedTree(SyntaxTree syntaxTree, EmbeddedText embeddedText)
        {
            this.SyntaxTree = syntaxTree;
            this.EmbeddedText = embeddedText;
        }

        public SyntaxTree SyntaxTree { get; }

        public EmbeddedText EmbeddedText { get; }
    }
}
