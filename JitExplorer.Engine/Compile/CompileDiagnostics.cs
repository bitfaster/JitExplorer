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
        private readonly string text;

        public CompileDiagnostic(Diagnostic diagnostic)
        {
            var lineSpan = diagnostic.Location.GetMappedLineSpan().Span;

            this.startLine = lineSpan.Start.Line;
            this.startCharacter = lineSpan.Start.Character;
            this.endLine = lineSpan.End.Line;
            this.endCharacter = lineSpan.End.Character;
            this.severity = diagnostic.Severity.ToString().ToLower();
            this.text = diagnostic.GetMessage();
        }

        public override string ToString() => $"({startLine + 1},{startCharacter + 1}) {severity} {id}: {text}";
    }
}
