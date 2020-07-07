
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine.Walk
{
    public class CustomWalker : CSharpSyntaxWalker
    {
        static int Tabs = 0;
        public override void Visit(SyntaxNode node)
        {
            Tabs++;
            var indents = new String('\t', Tabs);

            // find method declaration node

//            Console.WriteLine(indents + node.Kind());
            base.Visit(node);
            Tabs--;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            // parent = ClassDeclarationSyntax
            // parent.parent = NamespaceDeclarationSyntax
            // modifiers
            // parameterlist
            // type parameter list
            // identifier = name

            // source line:
            var line = node.GetLocation().GetMappedLineSpan().StartLinePosition.Line;

            base.VisitMethodDeclaration(node);
        }
    }
}
