using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JitExplorer.Engine.CodeAnlaysis
{
    public class AttributeStatementRewriter : CSharpSyntaxRewriter
    {
        private readonly string left;
        private readonly string right;

        public AttributeStatementRewriter(string attributeToReplace)
        {
            var tokens = attributeToReplace.Split('.');

            this.left = tokens[0];
            this.right = tokens[1];
        }

        public override SyntaxNode VisitAttribute(AttributeSyntax node)
        {
            if (node.Name is QualifiedNameSyntax qn)
            {
                if (qn.Left is IdentifierNameSyntax lid && lid.Identifier.Text == left
                    && qn.Right is IdentifierNameSyntax rid && rid.Identifier.Text == right)
                {
                    // important to preserve whitespace else source line indexing becomes wrong
                    var trailingTrivia = node.GetTrailingTrivia().Where(t =>
                        t.Kind() == SyntaxKind.WhitespaceTrivia || t.Kind() == SyntaxKind.EndOfLineTrivia).ToSyntaxTriviaList();

                    return SyntaxFactory.Attribute(
                                        SyntaxFactory.QualifiedName(
                                            SyntaxFactory.QualifiedName(
                                                SyntaxFactory.QualifiedName(
                                                    SyntaxFactory.IdentifierName("System"),
                                                    SyntaxFactory.IdentifierName("Runtime")),
                                                SyntaxFactory.IdentifierName("CompilerServices")),
                                            SyntaxFactory.IdentifierName("MethodImpl")))
                                    .WithArgumentList(
                                        SyntaxFactory.AttributeArgumentList(
                                            SyntaxFactory.SingletonSeparatedList<AttributeArgumentSyntax>(
                                                SyntaxFactory.AttributeArgument(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.IdentifierName("System"),
                                                                    SyntaxFactory.IdentifierName("Runtime")),
                                                                SyntaxFactory.IdentifierName("CompilerServices")),
                                                            SyntaxFactory.IdentifierName("MethodImplOptions")),
                                                        SyntaxFactory.IdentifierName("NoInlining"))))))
                                    .WithTrailingTrivia(trailingTrivia);
                }    
            }

            return base.VisitAttribute(node);
        }
    }
}
