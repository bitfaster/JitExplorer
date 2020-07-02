using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine.Compile
{
    public class CompileDiagnostic
    {
        private readonly int startLine;
        private readonly int startCharacter;
        private readonly int endLine;
        private readonly int endCharacter;

        private readonly string severity;
        private readonly string id;
        private readonly string message;

        public CompileDiagnostic(Diagnostic diagnostic)
        {
            var lineSpan = diagnostic.Location.GetMappedLineSpan().Span;

            this.startLine = lineSpan.Start.Line;
            this.startCharacter = lineSpan.Start.Character;
            this.endLine = lineSpan.End.Line;
            this.endCharacter = lineSpan.End.Character;
            this.severity = diagnostic.Severity.ToString().ToLower();
            this.message = diagnostic.GetMessage();
            this.id = diagnostic.Id;
        }

        // Visual studio format
        // CompileDiagnostics.cs(31,38,31,41): error CS1002: ; expected
        public override string ToString() => $"({startLine + 1},{startCharacter + 1},{endLine + 1},{endCharacter + 1}): {severity} {id}: {message}";
    }
}
