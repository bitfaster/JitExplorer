using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows;
using ICSharpCode.AvalonEdit.Utils;
using ICSharpCode.AvalonEdit.Highlighting;

namespace JitExplorer.Controls
{
    public class AssemblyTextEditor : TextEditor
    {
        public AssemblyTextEditor() : base(new TextArea())
        {
            this.IsReadOnly = true;
            this.WordWrap = true;
            this.ShowLineNumbers = false;
            this.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Asm");
        }

        protected override void OnDocumentChanged(EventArgs e)
        {
            base.OnDocumentChanged(e);

            var sp = this.Document.ServiceProvider;

            var wrapped = new ServiceProviderWrapper(sp, new LineAddressResolver());

            this.Document.ServiceProvider = wrapped;
        }

        public static readonly DependencyProperty ShowMemoryAddressesProperty =
            DependencyProperty.Register("ShowMemoryAddresses", typeof(bool), typeof(TextEditor),
                                        new FrameworkPropertyMetadata(Boxes.False, OnShowMemoryAddressesChanged));

        public bool ShowMemoryAddresses
        {
            get { return (bool)GetValue(ShowMemoryAddressesProperty); }
            set { SetValue(ShowMemoryAddressesProperty, Boxes.Box(value)); }
        }

        static void OnShowMemoryAddressesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextEditor editor = (TextEditor)d;
            var leftMargins = editor.TextArea.LeftMargins;
            if ((bool)e.NewValue)
            {
                LineNumberMargin lineNumbers = new MemoryAddressMargin();
                Line line = (Line)DottedLineMargin.Create();
                leftMargins.Insert(0, lineNumbers);
                leftMargins.Insert(1, line);
                var lineNumbersForeground = new Binding("LineNumbersForeground") { Source = editor };
                line.SetBinding(Line.StrokeProperty, lineNumbersForeground);
                lineNumbers.SetBinding(Control.ForegroundProperty, lineNumbersForeground);
            }
            else
            {
                for (int i = 0; i < leftMargins.Count; i++)
                {
                    if (leftMargins[i] is LineNumberMargin)
                    {
                        leftMargins.RemoveAt(i);
                        if (i < leftMargins.Count && DottedLineMargin.IsDottedLineMargin(leftMargins[i]))
                        {
                            leftMargins.RemoveAt(i);
                        }
                        break;
                    }
                }
            }
        }

        public class ServiceProviderWrapper : IServiceProvider
        {
            private readonly IServiceProvider wrapped;
            private readonly LineAddressResolver lineAddressResolver;

            public ServiceProviderWrapper(IServiceProvider wrapped, LineAddressResolver lineAddressResolver)
            {
                this.wrapped = wrapped;
                this.lineAddressResolver = lineAddressResolver;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(LineAddressResolver))
                {
                    return this.lineAddressResolver;
                }

                return this.wrapped.GetService(serviceType);
            }
        }
    }
}
