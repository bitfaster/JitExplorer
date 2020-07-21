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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Wrapping;

namespace JitExplorer.Controls
{
    public class AssemblyTextEditor : BindableTextEditor
    {
        private ILineAddressResolver lineAddressResolver = new EmptyAddressResolver();

        public AssemblyTextEditor()
        {
            this.IsReadOnly = true;
            this.WordWrap = true;
            this.ShowLineNumbers = false;
            this.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Asm");
        }

        public Dictionary<int, string> AsmLineAddressIndex
        {
            set { this.lineAddressResolver = new LineAddressResolver(value); this.HookAddressResolver(); } 
        }

        protected override void OnDocumentChanged(EventArgs e)
        {
            base.OnDocumentChanged(e);
            this.HookAddressResolver();
        }

        private void HookAddressResolver()
        {
            var sp = this.Document.ServiceProvider;
            var wrapped = new ServiceProviderWrapper(sp, this.lineAddressResolver);
            this.Document.ServiceProvider = wrapped;
        }

        public static readonly DependencyProperty AsmLineAddressIndexProperty =
            DependencyProperty.Register("AsmLineAddressIndex", typeof(Dictionary<int, string>), typeof(TextEditor),
                                        new FrameworkPropertyMetadata(null, OnAsmLineAddressIndexChanged));

        static void OnAsmLineAddressIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AssemblyTextEditor editor = (AssemblyTextEditor)d;

            var index = e.NewValue as Dictionary<int, string>;

            if (index != null)
            {
                editor.AsmLineAddressIndex = index;
            }
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
            private readonly ILineAddressResolver lineAddressResolver;

            public ServiceProviderWrapper(IServiceProvider wrapped, ILineAddressResolver lineAddressResolver)
            {
                this.wrapped = wrapped;
                this.lineAddressResolver = lineAddressResolver;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(ILineAddressResolver))
                {
                    return this.lineAddressResolver;
                }

                return this.wrapped.GetService(serviceType);
            }
        }
    }
}
