using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using unvell.ReoGrid.Data;
using unvell.ReoGrid.Graphics;
using unvell.ReoGrid.Rendering;

namespace unvell.ReoGrid.Views
{
    partial class CellsViewport
    {
		public void RegisterDataProvider(DataProvider provider)
        {
            if (DataProviders.Contains(provider))
				return;
			sheet.workbook.ControlInstance.Children.Add(provider.Trigger);
			sheet.workbook.ControlInstance.Children.Add(provider.Selector);
			provider.Trigger.Visibility = System.Windows.Visibility.Collapsed;
			DataProviders.Add(provider);
		}
		public System.Collections.Generic.List<DataProvider> DataProviders { get; set; }
			= new System.Collections.Generic.List<DataProvider>();
		private void DrawDataProvider(CellDrawingContext dc)
		{
			if (DataProviders.Count == 0)
				return;
			DataProviders.ForEach(x => x.Trigger.Visibility = System.Windows.Visibility.Collapsed);
			DataProviders.ForEach(x => x.Selector.IsOpen = false);
			if (sheet.SelectionRange.IsEmpty || dc.DrawMode != DrawMode.View)
				return;

			var g = dc.Graphics as WPFGraphics;
			var scaledSelectionRect = GetScaledAndClippedRangeRect(this,
				sheet.SelectionRange.StartPos, sheet.SelectionRange.StartPos, 0);

            Cell cell = sheet.GetCell(sheet.SelectionRange.StartPos);
            if (cell == null || cell.DataProvider == null) return;

            if (g.TransformStack.Count!=0)
            {
				MatrixTransform mt = g.TransformStack.Peek();
				if (mt.TryTransform(new System.Windows.Point(cell.Right, cell.Top), out var righttop))
                {
                    if (righttop.X < this.Left || righttop.Y < this.Top)
                    {
                        cell.DataProvider.Trigger.Visibility = System.Windows.Visibility.Collapsed;
                        return;
                    }
					mt.TryTransform(new System.Windows.Point(cell.Left, cell.Bottom), out var leftbottom);
					var position = (cell.DataProvider.Trigger.Parent as Canvas).PointToScreen(leftbottom);
					cell.DataProvider.Trigger.Margin = new System.Windows.Thickness(righttop.X, righttop.Y, 0, 0);
					cell.DataProvider.Trigger.Visibility = System.Windows.Visibility.Visible;
					Rectangle rectangle = new Rectangle(position.X, position.Y, righttop.X - leftbottom.X, righttop.Y - leftbottom.Y);
					cell.DataProvider.Update(rectangle, cell);
				}
			}
            else
            {
				cell.DataProvider.Trigger.Margin = new System.Windows.Thickness(this.Left + scaledSelectionRect.Right, this.Top + scaledSelectionRect.Top, 0, 0);
				cell.DataProvider.Trigger.Visibility = System.Windows.Visibility.Visible;
			}
		}
    }
}
