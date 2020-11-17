using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using unvell.ReoGrid.Graphics;

namespace unvell.ReoGrid.Data
{
    //public interface IDataProviderSelector
    //{

    //    Action<object> SelectedItemChangedCallback { get; set; }
    //}
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
            var toggle = new ToggleButton();
            Trigger = new WeakReference<ToggleButton>(toggle);
            WeakEventManager<ToggleButton, RoutedEventArgs>.AddHandler(toggle, "Checked", new EventHandler<RoutedEventArgs>(Trigger_Checked));
            WeakEventManager<ToggleButton, RoutedEventArgs>.AddHandler(toggle, "Unchecked", new EventHandler<RoutedEventArgs>(Trigger_Unchecked));
            //toggle.Checked += Toggle_Checked;
            //toggle.Unchecked += Trigger_Unchecked;
            var selector = new Popup();
            Selector = new WeakReference<Popup>(selector);
            WeakEventManager<Popup, EventArgs>.AddHandler(selector, "Opened", new EventHandler<EventArgs>(Selector_Opened));
            WeakEventManager<Popup, EventArgs>.AddHandler(selector, "Closed", new EventHandler<EventArgs>(Selector_Closed));
            selector.Opened += Selector_Opened;
            selector.Closed += Selector_Closed;
            selector.Child = new System.Windows.Controls.ListBox();
            SelectionChangedEventHandler = new EventHandler<SelectionChangedEventArgs>(DataProviderSelector_SelectionChanged);
            string xamlpath = "pack://application:,,,/unvell.ReoGrid;component/Data/ComboToggle.xaml";
            System.Windows.Resources.StreamResourceInfo xamlinfo = System.Windows.Application.GetResourceStream(new Uri(xamlpath));
            //StreamReader reader = new StreamReader(xamlinfo.Stream, System.Text.Encoding.ASCII);
            
            //string xaml = reader.ReadToEnd();
            ResourceDictionary Parse_Resource = System.Windows.Markup.XamlReader.Load(xamlinfo.Stream) as ResourceDictionary;
            toggle.Resources = Parse_Resource;
        }

        public System.Collections.IEnumerable ItemsSource 
        {
            get
            {
                if (Selector.TryGetTarget(out var selector))
                    return (selector.Child as System.Windows.Controls.ListBox).ItemsSource;
                else
                    return null;
            }
            set
            {
                if (Selector.TryGetTarget(out var selector))
                    (selector.Child as System.Windows.Controls.ListBox).ItemsSource=value;
            }
        }
        public object SelectedItem
        {
            get
            {
                if (Selector.TryGetTarget(out var selector))
                    return (selector.Child as System.Windows.Controls.ListBox).SelectedItem;
                else
                    return null;
            }
            set
            {
                if (Selector.TryGetTarget(out var selector))
                    (selector.Child as System.Windows.Controls.ListBox).SelectedItem = value;
            }
        }
        private EventHandler<SelectionChangedEventArgs> SelectionChangedEventHandler;
        private void Selector_Opened(object sender, EventArgs e)
        {
            if (Selector.TryGetTarget(out var selector))
            {
                var listbox = (selector.Child as ListBox);
                WeakEventManager<ListBox, SelectionChangedEventArgs>.AddHandler(listbox, "SelectionChanged", SelectionChangedEventHandler);
            }
        }

        private void DataProviderSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Selector.TryGetTarget(out var selector))
            {
                var listbox = (selector.Child as ListBox);
                WeakEventManager<ListBox, SelectionChangedEventArgs>.RemoveHandler(listbox, "SelectionChanged", SelectionChangedEventHandler);
                selector.IsOpen = false;
            }
        }

        private void Selector_Closed(object sender, EventArgs e)
        {
            if (Trigger.TryGetTarget(out var toggle))
            {
                toggle.IsChecked = false;
                ActiveCell.TryGetTarget(out var cell);
                if (Selector.TryGetTarget(out var selector))
                    SelectorClosed?.Invoke(this, new SelectorClosedEventArgs(cell, (selector.Child as System.Windows.Controls.ListBox).SelectedItem));
            }
        }

        public WeakReference<Cell> ActiveCell { get; private set; }
        internal void Update(Rectangle rectangle, Cell cell)
        {
            if(Selector.TryGetTarget(out var selector))
            {
                selector.MinWidth = rectangle.Width;
                selector.Placement = PlacementMode.AbsolutePoint;
                selector.HorizontalOffset = rectangle.X;
                selector.VerticalOffset = rectangle.Y;
            }
            
            ActiveCell = new WeakReference<Cell>(cell);
        }
        private Action<object> Callback;
        private void Trigger_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Selector.TryGetTarget(out var selector))
                selector.IsOpen = false;
        }

        private void Trigger_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ActiveCell.TryGetTarget(out var cell);
            var eventargs = new SelectorOpeningEventArgs(cell);
            SelectorOpening?.Invoke(this, eventargs);
            if (eventargs.IsCancelled) return;
            if (Selector.TryGetTarget(out var selector))
                selector.IsOpen = true;
        }

        public WeakReference<ToggleButton> Trigger { get; private set; }
        public WeakReference<Popup> Selector { get; private set; }
        public WeakReference<System.Windows.Controls.ListBox> DataProviderSelector { get;private set; }
    }
}
