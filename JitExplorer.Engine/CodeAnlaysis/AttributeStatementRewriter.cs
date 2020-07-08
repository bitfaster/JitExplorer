using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine.CodeAnlaysis
{
    public class AttributeStatementRewriter : CSharpSyntaxRewriter
    {
        // http://roslynquoter.azurewebsites.net/
        // replace [Jit.This] with 
        // [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
        {
            if (node.Parent is MethodDeclarationSyntax && node.ToString() == "[Jit.This]")
            {
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
                                                        SyntaxFactory.IdentifierName("NoInlining"))))))));
            }

            return base.VisitAttributeList(node);
        }
    }
}
