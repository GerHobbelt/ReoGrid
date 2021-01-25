using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace unvell.ReoGrid.Events
{
    public class BeforeCellDataChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Get instance of current editing cell. This property may be null.
        /// </summary>
        public Cell Cell { get; protected set; }
        public object NewData { get; protected set; }
        public object OldData { get; protected set; }
        public bool IsCancelled { get; set; }
        /// <summary>
        /// Create instance for CellEventArgs with specified cell.
        /// </summary>
        /// <param name="cell">Instance of current editing cell.</param>
        public BeforeCellDataChangedEventArgs(Cell cell, object newobj, object oldobj)
        {
            this.Cell = cell;
            this.NewData = newobj;
            this.OldData = oldobj;
            IsCancelled = false;
        }
    }
    public class BeforeDragCellDataChangedEventArgs : EventArgs
    {
        public List<CellPosition> FromCells { get; internal set; }
        public List<CellPosition> ToCells { get; internal set; }
        public bool IsCancelled { get; set; } = false;
        /// <summary>
        /// Create instance for CellEventArgs with specified cell.
        /// </summary>
        /// <param name="cell">Instance of current editing cell.</param>
        public BeforeDragCellDataChangedEventArgs()
        {
            IsCancelled = false;
        }
    }

    public class DragCellEventArgs : CellEventArgs
    {
        public bool IsCancelled { get; set; } = false;
        public List<CellPosition> FromCells { get; internal set; }
        public List<CellPosition> ToCells { get; internal set; }
        /// <summary>
        /// Create instance for CellEventArgs with specified cell.
        /// </summary>
        /// <param name="cell">Instance of current editing cell.</param>
        public DragCellEventArgs(Cell cell) : base(cell)
        {

        }
    }
}
