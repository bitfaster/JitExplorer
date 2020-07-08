
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine.Walk
{
    public class ExtractMarkedMethod : CSharpSyntaxWalker
    {
        private bool wasFound = false;

        private string method = string.Empty;

        private List<string> errors = new List<string>();

        public bool Success => this.wasFound && this.errors.Count == 0;

        public string Method => this.method;

        public IEnumerable<string> Errors => this.errors;

        public override void VisitAttribute(AttributeSyntax node)
        {
            if (node.Name.ToString() == "Jit.This")
            {
                if (this.wasFound)
                {
                    CreateError(node, "The attribute [Jit.This] cannot be used to decorate multiple methods.");
                }

                if (node.Parent.Parent is MethodDeclarationSyntax methodSyntax)
                {
                    string name = methodSyntax.Identifier.ValueText;

                    ValidateSignature(methodSyntax);

                    if (methodSyntax.Parent is ClassDeclarationSyntax classSyntax)
                    {
                        string className = classSyntax.Identifier.ValueText;

                        if (classSyntax.Parent is NamespaceDeclarationSyntax namespaceSyntax)
                        {
                            string nameSpace = (namespaceSyntax.Name as IdentifierNameSyntax).Identifier.ValueText;

                            this.method = $"{nameSpace}.{className}.{name}();";
                            this.wasFound = true;
                        }
                    }
                }
                else
                {
                    CreateError(node.Parent.Parent, "");
                }
            }

            base.VisitAttribute(node);
        }

        private void ValidateSignature(MethodDeclarationSyntax methodSyntax)
        {
            if (methodSyntax.ParameterList.Parameters.Any())
            {
                CreateError(methodSyntax, "The method decorated with [Jit.This] cannot have parameters.");
            }

            if (!methodSyntax.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                CreateError(methodSyntax, "The method decorated with [Jit.This] must be a public method.");
            }

            if (!methodSyntax.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                CreateError(methodSyntax, "The method decorated with [Jit.This] must be a static method.");
            }
        }

        private void CreateError(SyntaxNode node, string message)
        {
            var start = node.GetLocation().GetMappedLineSpan().StartLinePosition;
            var end = node.GetLocation().GetMappedLineSpan().EndLinePosition;
            string error = $"({start.Line},{start.Character},{end.Line},{end.Character}) {message}";

            this.errors.Add(error);
        }
    }
}
