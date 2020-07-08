
using JitExplorer.Engine.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JitExplorer.Engine.CodeAnlaysis
{
    public class ExtractMarkedMethod : CSharpSyntaxWalker
    {
        private readonly string attributeName;

        public ExtractMarkedMethod(string attributeName)
        {
            this.attributeName = attributeName;
        }

        private bool wasFound = false;

        private Metadata.MethodInfo methodInfo;

        private List<string> errors = new List<string>();

        public bool Success => this.wasFound && this.errors.Count == 0;

        public Metadata.MethodInfo Method => this.methodInfo;

        public IEnumerable<string> Errors
        { 
            get 
            {
                if (this.wasFound)
                {
                    return this.errors;
                }
                return this.errors.Concat(new[] { $"A public static method must be marked with the attribute [{this.attributeName}]." });
            } 
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            if (node.Name.ToString() == this.attributeName)
            {
                if (this.wasFound)
                {
                    CreateError(node, $"The attribute [{this.attributeName}] cannot be used to decorate multiple methods.");
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

                            var classInfo = new ClassInfo(nameSpace, className);
                            this.methodInfo = new Metadata.MethodInfo(name, classInfo, Array.Empty<ClassInfo>());

                            this.wasFound = true;
                        }
                    }
                }
                else
                {
                    CreateError(node.Parent.Parent, $"The attribute [{this.attributeName}] must be used to decorate a method.");
                }
            }

            base.VisitAttribute(node);
        }

        private void ValidateSignature(MethodDeclarationSyntax methodSyntax)
        {
            if (methodSyntax.ParameterList.Parameters.Any())
            {
                CreateError(methodSyntax, $"The method decorated with [{this.attributeName}] cannot have parameters.");
            }

            if (!methodSyntax.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                CreateError(methodSyntax, $"The method decorated with [{this.attributeName}] must be a public method.");
            }

            if (!methodSyntax.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                CreateError(methodSyntax, $"The method decorated with [{this.attributeName}] must be a static method.");
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
