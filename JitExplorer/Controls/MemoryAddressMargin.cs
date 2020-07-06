﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;

namespace JitExplorer.Controls
{
	// https://github.com/icsharpcode/AvalonEdit/blob/28b887f78c821c7fede1d4fc461bde64f5f21bd1/ICSharpCode.AvalonEdit/Editing/LineNumberMargin.cs
	public class MemoryAddressMargin : LineNumberMargin
	{
		/// <inheritdoc/>
		protected override void OnRender(DrawingContext drawingContext)
		{
			TextView textView = this.TextView;

			var addressResolver = (ILineAddressResolver)textView.Document.ServiceProvider.GetService(typeof(ILineAddressResolver));

			Size renderSize = this.RenderSize;
			if (textView != null && textView.VisualLinesValid)
			{
				var foreground = (Brush)GetValue(Control.ForegroundProperty);
				foreach (VisualLine line in textView.VisualLines)
				{
					FormattedText text = TextFormatterFactory.CreateFormattedText(
						this,
						//lineNumber.ToString(CultureInfo.CurrentCulture),
						addressResolver.GetAddress(line.FirstDocumentLine.LineNumber),
						typeface, emSize, foreground
					);
					double y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);
					drawingContext.DrawText(text, new Point(renderSize.Width - text.Width, y - textView.VerticalOffset));
				}
			}
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			typeface = this.CreateTypeface();
			emSize = (double)GetValue(TextBlock.FontSizeProperty);

			FormattedText text = TextFormatterFactory.CreateFormattedText(
				this,
				//new string('9', 6),
				"7FFED9580410",
				typeface,
				emSize,
				(Brush)GetValue(Control.ForegroundProperty)
			);
			return new Size(text.Width, 0);
		}
	}
}
