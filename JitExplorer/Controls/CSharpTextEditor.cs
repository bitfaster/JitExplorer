using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Controls
{
    public class CSharpTextEditor : BindableTextEditor
    {
        public CSharpTextEditor()
        {
            this.ShowLineNumbers = true;
            this.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
        }
    }
}
