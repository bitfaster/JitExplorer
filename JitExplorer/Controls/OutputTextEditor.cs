using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace JitExplorer.Controls
{
    public class OutputTextEditor : BindableTextEditor
    {
        public OutputTextEditor() 
        {
            this.IsReadOnly = true;
            this.WordWrap = true;
            this.ShowLineNumbers = false;
            //this.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Asm");
        }
    }
}
