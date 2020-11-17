﻿using System.Windows.Controls;
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
			if(provider.Trigger.TryGetTarget(out var tigger))
            {
				sheet.workbook.ControlInstance.Children.Add(tigger);
				tigger.Visibility = System.Windows.Visibility.Collapsed;
			}
            if (provider.Selector.TryGetTarget(out var selector))
            {
				sheet.workbook.ControlInstance.Children.Add(selector);
			}
			
			DataProviders.Add(provider);
		}
		public System.Collections.Generic.List<DataProvider> DataProviders { get; set; }
			= new System.Collections.Generic.List<DataProvider>();
		private void DrawDataProvider(CellDrawingContext dc)
		{
			if (DataProviders.Count == 0)
				return;
            DataProviders.ForEach(x =>
            {
                if (x.Trigger.TryGetTarget(out var tigger))
                {
					tigger.Visibility = System.Windows.Visibility.Collapsed;
                }
            });
            DataProviders.ForEach(x =>
            {
                if (x.Selector.TryGetTarget(out var selector))
                {
                    selector.IsOpen = false;
                }
            });
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
						if (cell.DataProvider.Trigger.TryGetTarget(out var tigger))
						{
							tigger.Visibility = System.Windows.Visibility.Collapsed;
							
							tigger.Height = cell.Height;
						}
                        return;
                    }
                    else
                    {
						mt.TryTransform(new System.Windows.Point(cell.Left, cell.Bottom), out var leftbottom);
						if (cell.DataProvider.Trigger.TryGetTarget(out var tigger))
						{
							tigger.Height = cell.Height;
							tigger.Visibility = System.Windows.Visibility.Collapsed;
							var position = (tigger.Parent as Canvas).PointToScreen(leftbottom);
							tigger.Margin = new System.Windows.Thickness(righttop.X, righttop.Y, 0, 0);
							tigger.Visibility = System.Windows.Visibility.Visible;
							Rectangle rectangle = new Rectangle(position.X, position.Y, righttop.X - leftbottom.X, righttop.Y - leftbottom.Y);
							cell.DataProvider.Update(rectangle, cell);
						}
					}
				}
			}
            else
            {
				if (cell.DataProvider.Trigger.TryGetTarget(out var tigger))
                {
					tigger.Margin = new System.Windows.Thickness(this.Left + scaledSelectionRect.Right, this.Top + scaledSelectionRect.Top, 0, 0);
					tigger.Visibility = System.Windows.Visibility.Visible;
				}
			}
		}
    }
}
