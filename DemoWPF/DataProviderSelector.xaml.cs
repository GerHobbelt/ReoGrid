using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace unvell.ReoGrid.WPFDemo
{
    /// <summary>
    /// DataProviderSelector.xaml 的交互逻辑
    /// </summary>
    public partial class DataProviderSelector : UserControl, Data.IDataProviderSelector
    {
        public DataProviderSelector()
        {
            InitializeComponent();
            DataContext = this;
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(DataProviderSelector), new PropertyMetadata(null));

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public Action<object> SelectedItemChangedCallback { get; set; }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(DataProviderSelector), new PropertyMetadata(null,new PropertyChangedCallback(OnSelectecItemChanged)));

        private static void OnSelectecItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as DataProviderSelector).OnSelectecItemChanged();
        }

        private void OnSelectecItemChanged()
        {
            if (SelectedItemChangedCallback != null)
                SelectedItemChangedCallback(SelectedItem);
        }
    }
}
