using ICSharpCode.AvalonEdit.Editing;
using JitExplorer.Component;
using JitExplorer.Model;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JitExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow 
    {
        public AppModel AppModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            
            this.AppModel = new AppModel();
            DataContext = this.AppModel;
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)this.AppModel.HandleKeyDownEvent);

            this.AssemblerView.MouseDoubleClick += AssemblerView_MouseDoubleClick;

            this.Loaded += ThinBorder.SetThinBorder;

            if (IntPtr.Size == 8)
            { 
                this.Title = "JitExplorer (x64)";
                var i = (ComboBoxItem)this.Platform.Items[1];
                i.IsEnabled = false;
            }
            else
            {
                this.Title = "JitExplorer (x86)";
                this.Platform.SelectedIndex = 1;
                var i = (ComboBoxItem)this.Platform.Items[0];
                i.IsEnabled = false;
            }
        }

        // Scroll to line
        private void AssemblerView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // https://www.codeproject.com/Articles/42490/Using-AvalonEdit-WPF-Text-Editor?msg=4395053#xx4395053xx
            // https://github.com/icsharpcode/AvalonEdit/blob/master/ICSharpCode.AvalonEdit/Editing/TextArea.cs
            // https://github.com/aelij/RoslynPad/blob/f9cf2b3f14333d73210aa91329ec162324de2b70/src/RoslynPad.Editor.Shared/CodeTextEditor.cs - see OnMouseHover
            var t = e.Device.Target as ICSharpCode.AvalonEdit.Editing.TextArea;

            if (t != null)
            {
                var p = t.Caret.Position;
                var d = t.Document;

                var textLine = d.GetLineByNumber(p.Line);
                var str = d.Text.Substring(textLine.Offset, textLine.Length);

                if (this.AppModel.Disassembly != null)
                {
                    if (this.AppModel.Disassembly.AsmToSourceLineIndex.TryGetValue(p.Line, out var targetLine))
                    {
                        if (targetLine < this.CodeEditor.Document.LineCount)
                        {
                            var ceLine = this.CodeEditor.TextArea.Document.GetLineByNumber(targetLine);
                            this.CodeEditor.ScrollTo(targetLine, 0);
                            this.CodeEditor.TextArea.Selection = Selection.Create(this.CodeEditor.TextArea, ceLine.Offset, ceLine.EndOffset);
                        }
                    }

                    if (this.AppModel.Disassembly.AsmLineToAsmLineIndex.TryGetValue(p.Line, out targetLine))
                    {
                        if (targetLine < this.AssemblerView.Document.LineCount)
                        {
                            var ceLine = this.AssemblerView.TextArea.Document.GetLineByNumber(targetLine);
                            this.AssemblerView.ScrollTo(targetLine, 0);
                            this.AssemblerView.TextArea.Selection = Selection.Create(this.AssemblerView.TextArea, ceLine.Offset, ceLine.EndOffset);
                        }
                    }

                    e.Handled = true;
                    return;
                }
            }
        }
    }
}
