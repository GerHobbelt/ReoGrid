/*****************************************************************************
 * 
 * ReoGrid - .NET Spreadsheet Control
 * 
 * http://reogrid.net/
 *
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 * PURPOSE.
 *
 * Author: Jing <lujing at unvell.com>
 *
 * Copyright (c) 2012-2016 Jing <lujing at unvell.com>
 * Copyright (c) 2012-2016 unvell.com, all rights reserved.
 * 
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace unvell.ReoGrid.Views
{
	internal abstract class LayerViewport : Viewport
	{
		public LayerViewport(IViewportController vc)
			: base(vc)
		{
		}

		public override IView GetViewByPoint(Graphics.Point p)
		{
			return base.GetChildrenByPoint(p);
		}

		public override void UpdateView()
		{
			if (this.children != null)
			{
				foreach (var child in this.children)
				{
					child.Bounds = this.bounds;
					child.ScaleFactor = this.scaleFactor;

					if (child is IViewport)
					{
						var viewport = (IViewport)child;
						viewport.ViewStart = this.viewStart;
						viewport.VisibleRegion = this.visibleRegion;
						viewport.ScrollableDirections = this.ScrollableDirections;
					}

					child.UpdateView();
				}
			}
		}
	}

	class SheetViewport : LayerViewport
	{
		public SheetViewport(IViewportController vc)
			: base(vc)
		{
            this.children = new List<IView>(4)
                {
                    new CellsViewport(vc) { PerformTransform = false },

#if DRAWING
					new DrawingViewport(vc) { PerformTransform = false },
#if COMMENT
					new CommentViewport(vc) { PerformTransform = false },
#endif // COMMENT
#endif // DRAWING

				new CellsForegroundView(vc) { PerformTransform = false },
                };
        }
	}

}

namespace unvell.ReoGrid
{
    using unvell.ReoGrid.Data;
    using unvell.ReoGrid.Views;

	partial class Worksheet
	{
		internal void InitViewportController()
		{
			this.viewportController = new NormalViewportController(this);
		}
		public IList<DataProvider> DataProviders
        {
            get
            {
				SheetViewport stport = (this.viewportController as NormalViewportController).View.Children.FirstOrDefault(x => x is SheetViewport) as SheetViewport;
				if (stport == null)
					return null;
				CellsViewport cvport = stport.Children.FirstOrDefault(x => x is CellsViewport) as CellsViewport;
				if (cvport == null)
					return null;
				return cvport.DataProviders;
			}
        }
		public void RegisterDataProvider(DataProvider dataprovider)
        {
			SheetViewport stport = (this.viewportController as NormalViewportController).View.Children.FirstOrDefault(x => x is SheetViewport) as SheetViewport;
            if (stport == null)
				return;
			CellsViewport cvport = stport.Children.FirstOrDefault(x => x is CellsViewport) as CellsViewport;
			if (cvport == null)
				return;
			cvport.RegisterDataProvider(dataprovider);
		}
		public void UnregisterDataProvider(DataProvider dataprovider)
		{
			SheetViewport stport = (this.viewportController as NormalViewportController).View.Children.FirstOrDefault(x => x is SheetViewport) as SheetViewport;
			if (stport == null)
				return;
			CellsViewport cvport = stport.Children.FirstOrDefault(x => x is CellsViewport) as CellsViewport;
			if (cvport == null)
				return;
			cvport.UnregisterDataProvider(dataprovider);
		}
	}
}