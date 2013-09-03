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
        List<Customer> custList;
        List<Product> prodList;
        public DataPicker()
        {
            InitializeComponent();
            if (app.InitializexmlDoc()){ 
                MessageBox.Show("Произошла ошибка при открытии файла с данными");
                MainWindow tmainWindow = new MainWindow();
                tmainWindow.Show();
                this.Close();
            }
            Dictionary<int, Product> ProductsDic = app.GetGoods();
            Dictionary<int, Customer> CustomersDic = app.GetCustomers();
            custList = CustomersDic.Values.ToList<Customer>();
            prodList = ProductsDic.Values.ToList<Product>();
            app.GetRealisations(CustomersDic, ProductsDic);
            if (app.GetCustomerData(ref custList)) {
                MessageBox.Show("Ошибка обращения к документу");
            }
            CustomersDataGrid.DataContext = custList;
            ProductsDataGrid.DataContext = prodList;
        }    
        private void AppExit(object sender, RoutedEventArgs e)
        {
            app.GracefulShutdown();
        }
        private void OkKlick(object sender, RoutedEventArgs e) {
            app.UploadData();
            MessageBox.Show("Это успех!");
            app.GracefulShutdown();
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

        private void SavePData(object sender, RoutedEventArgs e) {
            app.SaveProductData(prodList);
            MessageBox.Show("Это успех!");
        }
        private void LoadPData(object sender, RoutedEventArgs e) {
            app.LoadProductData(ref prodList);
            MessageBox.Show("Это успех!");
        }
        private void SaveCData(object sender, RoutedEventArgs e) {
            app.SaveCustomerData(custList);
            MessageBox.Show("Это успех!");
        }
        private void LoadCData(object sender, RoutedEventArgs e) {
            app.LoadCustomerData(ref custList);
            MessageBox.Show("Это успех!");
        }
    }
}
