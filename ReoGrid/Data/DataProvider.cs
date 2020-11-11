using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using unvell.ReoGrid.Graphics;

namespace unvell.ReoGrid.Data
{
    public interface IDataProviderSelector
    {
        System.Collections.IEnumerable ItemsSource { get; set; }
        object SelectedItem { get; set; }
        Action<object> SelectedItemChangedCallback { get; set; }
    }
    public class SelectorOpeningEventArgs : EventArgs
    {
        public SelectorOpeningEventArgs(Cell cell)
        {
            Cell = cell;
        }
        public Cell Cell { get;private set; }
        public bool IsCancelled { get; set; } = false;
    }
    public class SelectorClosedEventArgs : EventArgs
    {
        public SelectorClosedEventArgs(Cell cell,object selecteditem)
        {
            Cell = cell;
            SelectedItem = selecteditem;
        }
        public Cell Cell { get; private set; }
        public object SelectedItem { get; private set; }
    }
    public class DataProvider
    {
        public event EventHandler<SelectorOpeningEventArgs> SelectorOpening;
        public event EventHandler<SelectorClosedEventArgs> SelectorClosed;
        public DataProvider()
        {
            Trigger = new ToggleButton();
            Trigger.Checked += Trigger_Checked;
            Trigger.Unchecked += Trigger_Unchecked;
            Selector = new Popup();
            Selector.Opened += Selector_Opened;
            Selector.Closed += Selector_Closed;
            Selector.MouseDown += Selector_PreviewMouseDown;
            Callback = (o) =>
            {
                Selector.IsOpen = false;
                SelectorClosed?.Invoke(this, new SelectorClosedEventArgs(ActiveCell, o));
            };
        }

        private void Selector_Opened(object sender, EventArgs e)
        {
            _DataProviderSelector.SelectedItemChangedCallback = Callback;
        }

        private void Selector_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Selector.IsOpen = false;
        }

        private IDataProviderSelector _DataProviderSelector;

        public IDataProviderSelector DataProviderSelector
        {
            get { return _DataProviderSelector; }
            set 
            {
                _DataProviderSelector = value;
                Selector.Child = value as UIElement;
            }
        }

        private void Selector_Closed(object sender, EventArgs e)
        {
            Trigger.IsChecked = false;
            _DataProviderSelector.SelectedItemChangedCallback = null;
        }

        public Cell ActiveCell { get; private set; }
        internal void Update(Rectangle rectangle, Cell cell)
        {
            //Selector.PlacementTarget = Trigger.Parent as UIElement;
            Selector.MinWidth = rectangle.Width;
            Selector.Placement = PlacementMode.AbsolutePoint;
            Selector.HorizontalOffset = rectangle.X;
            Selector.VerticalOffset = rectangle.Y;
            ActiveCell = cell;
        }
        private Action<object> Callback;
        private void Trigger_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            Selector.IsOpen = false;
            
        }

        private void Trigger_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            var eventargs = new SelectorOpeningEventArgs(ActiveCell);
            SelectorOpening?.Invoke(this, eventargs);
            if (eventargs.IsCancelled) return;
            Selector.IsOpen = true;
        }

        public ToggleButton Trigger { get; private set; }
        public Popup Selector { get; private set; }
    }
}
