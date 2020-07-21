using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace JitExplorer.Controls
{
    public class BindableTextEditor : TextEditor
    {
        public BindableTextEditor() : base(new TextArea())
        { }

        public string EditorText
        {
            get { return this.Text;}
            set { this.Text = value; }
        }

        public static readonly DependencyProperty EditorTextProperty =
            DependencyProperty.Register("EditorText", typeof(string), typeof(TextEditor),
                new FrameworkPropertyMetadata(string.Empty, OnEditorTextChanged));

        static void OnEditorTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BindableTextEditor editor = (BindableTextEditor)d;

            var text = e.NewValue as string ?? string.Empty;

            editor.Text = text;
        }
    }
}
