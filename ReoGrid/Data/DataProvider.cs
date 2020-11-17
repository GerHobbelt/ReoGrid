using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using unvell.ReoGrid.Graphics;

namespace unvell.ReoGrid.Data
{

    public enum HookType : int
    {
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7,
        WH_HARDWARE = 8,
        WH_DEBUG = 9,
        WH_SHELL = 10,
        WH_FOREGROUNDIDLE = 11,
        WH_CALLWNDPROCRET = 12,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }

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


    public class DataProvider : IDisposable
    {
        private readonly HookProc _mouseHook;
        private IntPtr _hMouseHook;
        private bool listen = false;
        private bool hooked = false;
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
        static extern IntPtr LoadLibrary(string lpFileName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr UnhookWindowsHookEx(IntPtr hook);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

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
            selector.Child = new System.Windows.Controls.ListBox();
            WeakEventManager<Popup, RoutedEventArgs>.AddHandler(selector, "Unloaded", new EventHandler<RoutedEventArgs>(Selector_Unloaded));
            SelectionChangedEventHandler = new EventHandler<SelectionChangedEventArgs>(DataProviderSelector_SelectionChanged);
            string xamlpath = "pack://application:,,,/unvell.ReoGrid;component/Data/ComboToggle.xaml";
            System.Windows.Resources.StreamResourceInfo xamlinfo = System.Windows.Application.GetResourceStream(new Uri(xamlpath));
            //StreamReader reader = new StreamReader(xamlinfo.Stream, System.Text.Encoding.ASCII);
            
            //string xaml = reader.ReadToEnd();
            ResourceDictionary Parse_Resource = System.Windows.Markup.XamlReader.Load(xamlinfo.Stream) as ResourceDictionary;
            toggle.Resources = Parse_Resource;
            _mouseHook = OnMouseHook;
            Hook();
        }
        private void Hook()
        {
            var hModule = GetModuleHandle(null);
            // 你可能会在网上搜索到下面注释掉的这种代码，但实际上已经过时了。
            //   下面代码在 .NET Core 3.x 以上可正常工作，在 .NET Framework 4.0 以下可正常工作。
            //   如果不满足此条件，你也可能可以正常工作，详情请阅读本文后续内容。
            // var hModule = Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]);
            _hMouseHook = SetWindowsHookEx(
                HookType.WH_MOUSE_LL,
                _mouseHook,
                hModule,
                0);
            if (_hMouseHook == IntPtr.Zero)
            {
                int errorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(errorCode);
            }
            hooked = true;
        }
        private void Unhook()
        {
            if (hooked && _hMouseHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hMouseHook);
                _hMouseHook = IntPtr.Zero;
                hooked = false;
            }
        }

        private void Selector_Unloaded(object sender, RoutedEventArgs e)
        {
            Unhook();
        }

        private IntPtr OnMouseHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // 在这里，你可以处理全局鼠标消息。
            if (listen)
            {
                
            }
            return CallNextHookEx(new IntPtr(0), nCode, wParam, lParam);
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

        public void Dispose()
        {
            Unhook();
        }

        public WeakReference<ToggleButton> Trigger { get; private set; }
        public WeakReference<Popup> Selector { get; private set; }
        public WeakReference<System.Windows.Controls.ListBox> DataProviderSelector { get;private set; }
    }
}
