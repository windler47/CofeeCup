using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Globalization;

namespace CoffeeCup
{
    /// <summary>
    /// Логика взаимодействия для DataPicker.xaml
    /// </summary>
    public partial class DataPicker : Window
    {
        CoffeeCup.App app = (CoffeeCup.App)App.Current;
        Dictionary<int, Product> xProducts = new Dictionary<int, Product>();
        Dictionary<int, Customer> xCustomers = new Dictionary<int, Customer>();
        public DataPicker()
        {

            InitializeComponent();
            if (app.InitializedocPath())
                //TODO: Write workaround
                { MessageBox.Show("Произошла ошибка при открытии файла с данными"); }
            xProducts = AppPublicFunctions.GetGoods(app);
            xCustomers = AppPublicFunctions.GetCustomers(app);
            List<Customer> dCustomers = xCustomers.Values.ToList<Customer>();
            List<Product> dProducts = xProducts.Values.ToList<Product>();
            CustomersDataGrid.DataContext = dCustomers;
            ProductsDataGrid.DataContext = dProducts;
        }    
        private void AppExit(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        private void OkKlick(object sender, RoutedEventArgs e) {
            MessageBox.Show("");
        }
        #region SINGLE CLICK EDITING
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else
                    {
                        DataGridRow row = FindVisualParent<DataGridRow>(cell);
                        if (row != null && !row.IsSelected)
                        {
                            row.IsSelected = true;
                        }
                    }
                }
            }
        }
        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
        #endregion
    }
    public class MultConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((int)value).ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null) {
                int res = 0;
                res = int.Parse((string)value);
                return res;
            }
            return 0;
        }
    }
}
