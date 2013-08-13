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
using System.Xml;
using System.Xml.Linq;

namespace CoffeeCup
{
    /// <summary>
    /// Логика взаимодействия для DataPicker.xaml
    /// </summary>
    public partial class DataPicker : Window
    {
        CoffeeCup.App app = (CoffeeCup.App)CoffeeCup.App.Current;
        Dictionary<int, Product> xProducts = new Dictionary<int, Product>();
        Dictionary<int, Customer> xCustomers = new Dictionary<int, Customer>();
        public DataPicker()
        {
            InitializeComponent();
            XElement xmlDoc = XElement.Load(app.docPath);
            xProducts = AppPublicFunctions.GetGoods(xmlDoc);
            xCustomers = AppPublicFunctions.GetCustomers(xmlDoc);
            List<Customer> dCustomers = xCustomers.Values.ToList<Customer>();
            List<Product> dProducts = xProducts.Values.ToList<Product>();
            CustomersDataGrid.DataContext = dCustomers;
            ProductsDataGrid.DataContext = xProducts.Values;
        }
            
        private void AppExit(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
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
}
