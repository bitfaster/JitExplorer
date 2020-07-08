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
        private readonly string attributeToReplace;
        public AttributeStatementRewriter(string attributeToReplace)
        {
            this.attributeToReplace = attributeToReplace;
        }

        // http://roslynquoter.azurewebsites.net/
        // replace [Jit.This] with 
        // [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
        {
            if (node.Parent is MethodDeclarationSyntax && node.ToString() == $"[{this.attributeToReplace}]")
            {
                // preserve trivia, else pdb line indexing will be screwed up
                var leadingWhiteSpace = node.GetLeadingTrivia().Where(t =>
                    t.Kind() == SyntaxKind.WhitespaceTrivia).ToSyntaxTriviaList();

                var endingWhitespcae = node.GetTrailingTrivia().Where(t =>
                    t.Kind() == SyntaxKind.WhitespaceTrivia || t.Kind() == SyntaxKind.EndOfLineTrivia).ToSyntaxTriviaList();

                return SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                    SyntaxFactory.Attribute(
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
                                                        SyntaxFactory.IdentifierName("NoInlining"))))))))
                    .WithLeadingTrivia(leadingWhiteSpace)
                    .WithTrailingTrivia(endingWhitespcae);
            }

            return base.VisitAttributeList(node);
        }
    }
}
