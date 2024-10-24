using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;

namespace ScreenPositionFromCaret
{
	internal sealed class ViewportAdornment1 : IDisposable
	{

		private readonly IWpfTextView view;
		private readonly TextBlock textBlock;
		private readonly Window testWindow;

		public ViewportAdornment1(IWpfTextView view)
		{
			if (view == null)
			{
				throw new ArgumentNullException("view");
			}

			this.view = view;

			this.view.GotAggregateFocus += GotAggregateFocus;
			this.view.ViewportLeftChanged += ViewportLeftChanged;
			this.view.Caret.PositionChanged += CaretPositionChanged;
			this.view.LayoutChanged += LayoutChanged;
			this.view.Closed += ViewClosed;

			ThreadHelper.ThrowIfNotOnUIThread();

			textBlock = new TextBlock()
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
				FontSize = 28
			};
			testWindow = new Window()
			{
				Background = new SolidColorBrush(GetThemeBackgroundColor()),
				Foreground = new SolidColorBrush(GetThemeForegroundColor()),
				Top = 0,
				Left = 0,
				WindowStyle = WindowStyle.None,
				Content = textBlock,
				Focusable = false,
				Owner = Application.Current.MainWindow,
			};
			testWindow.Show();
		}

		private void ViewportLeftChanged(object sender, EventArgs e)
		{
			if (GetCaretTextBounds() is TextBounds tb)
				MoveWindow(tb);
			else
				testWindow.Visibility = Visibility.Hidden;
		}

		private void ViewClosed(object sender, EventArgs e)
		{
			testWindow.Close();
		}

		private void GotAggregateFocus(object sender, EventArgs e)
		{
			if (GetCaretTextBounds() is TextBounds tb)
				MoveWindow(tb);
			else
				testWindow.Visibility = Visibility.Hidden;
		}

		private TextBounds? GetCaretTextBounds()
		{
			try
			{
				// get the caret position - will throw if off screen
				return view.TextViewLines.GetCharacterBounds(view.Caret.Position.BufferPosition);
			}
			catch { }
			return default;
		}
		private void MoveWindow(TextBounds caretPosition)
		{
			try
			{
				// get the top-left position of the caret line
				Point screenPosition = view.VisualElement.PointToScreen(new Point(0, caretPosition.Top - view.ViewportTop));
				// get the bottom-right position of the caret line
				Point screenPositionRight = view.VisualElement.PointToScreen(new Point(view.ViewportWidth, caretPosition.Bottom - view.ViewportTop));
				double xScale = testWindow.GetDpiXScale();
				double yScale = testWindow.GetDpiYScale();
				Rect windowPosition = new Rect()
				{
					X = (screenPosition.X) / xScale,
					Y = (screenPositionRight.Y) / yScale,
					Width = (screenPositionRight.X - screenPosition.X) / xScale,
					Height = 2 * (screenPositionRight.Y - screenPosition.Y) / yScale
				};
				testWindow.Visibility = Visibility.Visible;
				testWindow.Left = windowPosition.Left;
				testWindow.Top = windowPosition.Top;
				testWindow.Width = windowPosition.Width;
				testWindow.Height = windowPosition.Height;
				testWindow.Topmost = true;
				textBlock.FontSize = (testWindow.Height - 20) / 1.5;
				textBlock.Text = $"Pos: {windowPosition.Left:F0},{windowPosition.Top:F0} Size: {windowPosition.Width:F0} x {windowPosition.Height:F0}";
			}
			catch
			{
				testWindow.Visibility = Visibility.Hidden;
			}
		}
		private void LayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
		{
			if (GetCaretTextBounds() is TextBounds tb)
				MoveWindow(tb);
			else
				testWindow.Visibility = Visibility.Hidden;
		}
		private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
		{
			if (GetCaretTextBounds() is TextBounds caretPosition)
			{
				MoveWindow(caretPosition);
			}
		}
		public void Dispose()
		{
			testWindow.Close();
		}

		private Color GetThemeBackgroundColor()
		{
			// Get background color using VSColorTheme.GetThemedColor()
			var backgroundColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);

			return Color.FromArgb(
				backgroundColor.A,
				backgroundColor.R,
				backgroundColor.G,
				backgroundColor.B);
		}
		private Color GetThemeForegroundColor()
		{
			var foregroundColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);

			return Color.FromArgb(
				foregroundColor.A,
				foregroundColor.R,
				foregroundColor.G,
				foregroundColor.B);
		}
	}
}
