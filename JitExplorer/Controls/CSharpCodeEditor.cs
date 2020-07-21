using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using JitExplorer.Completion;
using JitExplorer.Engine.Compile;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace JitExplorer.Controls
{
    public class CSharpCodeEditor : TextEditor
    {
        private readonly RoslynCodeCompletion codeCompletion;
        private CompletionWindow completionWindow;

        public CSharpCodeEditor() : base(new TextArea())
        {
            this.TextArea.TextEntering += OnTextEntering;
            this.TextArea.TextEntered += OnTextEntered;

            this.codeCompletion = new RoslynCodeCompletion(Compiler.MetadataReferences);
            this.codeCompletion.Initialize();
        }

        private void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                var result = codeCompletion.CompleteAsync(this.Text, this.CaretOffset, '.').Result;

                // Open code completion after the user has pressed dot:
                completionWindow = new CompletionWindow(this.TextArea);
                completionWindow.CompletionList.CompletionData.AddRange(result);

                // TODO: how to correctly style this?
                completionWindow.CompletionList.Background = SystemColors.ControlDarkDarkBrush;

                completionWindow.Show();
                completionWindow.Closed += delegate {
                    completionWindow = null;
                };
            }
        }

        private void OnTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }
    }
}
