﻿/*****************************************************************************
 * 
 * ReoGrid - .NET Spreadsheet Control
 * 
 * http://reogrid.net
 *
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 * PURPOSE.
 *
 * ReoGrid and ReoGrid Demo project is released under MIT license.
 *
 * Copyright (c) 2012-2016 Jing <lujing at unvell.com>
 * Copyright (c) 2012-2016 unvell.com, all rights reserved.
 * 
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using unvell.ReoGrid.Graphics;
using unvell.ReoGrid.DataFormat;
using System.Globalization;

namespace unvell.ReoGrid.Demo.CellAndRange
{
	public partial class CustomDataFormatDemo : UserControl
	{
		private Worksheet sheet;

		public CustomDataFormatDemo()
		{
			InitializeComponent();

			this.sheet = grid.CurrentWorksheet;
			sheet.Name = "Custom Data Format Demo";

			DataFormatterManager.Instance.DataFormatters[CellDataFormatFlag.Custom] = new MyDataFormatter();

			var cell = this.sheet.Cells["B2"];
			cell.DataFormat = CellDataFormatFlag.Custom;
			cell.Data = 12345.6789;
		}
	}

	class MyDataFormatter : IDataFormatter
	{
		public string FormatCell(Cell cell, CultureInfo culture)
		{
			// Custom formatter only valid for this demo
			if (cell.Worksheet.Name != "Custom Data Format Demo") return null;

			double val = cell.GetData<double>();

			return val < 0 ? string.Format("[{0}]", (-val).ToString("###,###,##0.00")) : val.ToString("###,###,###.00");
		}

		public bool PerformTestFormat()
		{
			return true;
		}
	}
}
