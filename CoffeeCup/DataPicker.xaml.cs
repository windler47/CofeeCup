using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CoffeeCup {
    /// <summary>
    /// Логика взаимодействия для DataPicker.xaml
    /// </summary>
    public partial class DataPicker : Window {
        CoffeeCup.App app = (CoffeeCup.App)App.Current;
        LocalDatabase db;
        List<Customer> custList = new List<Customer>();
        List<Product> prodList = new List<Product>();
        public DataPicker(string documentUrl) {
            Dictionary<int, Product> ProductsDic = app.GetGoods();
            Dictionary<int, Customer> CustomersDic = app.GetCustomers();
            app.GetRealisations(CustomersDic, ProductsDic);
            foreach (List<Realization> realizationsList in app.realizations.Values) {
                foreach (Realization realization in realizationsList) {
                    if (!custList.Exists((e) => { return e.Name == realization.Buyer.Name; })) custList.Add(realization.Buyer);
                    foreach (SellingPosition sp in realization.SellingPositions) {
                        if (!prodList.Exists((e) => { return e.Name == sp.Product.Name; })) {
                            prodList.Add(sp.Product);
                        }
                    }
                }
            }
            db = new LocalDatabase(Path.Combine(app.appPath, "LocalBase.xml"));
            if (app.WorksheetFeedInit(documentUrl)) throw new System.ApplicationException("Ошибка обращения к Google документу.");
            InitializeComponent();
            CustomersDataGrid.DataContext = custList;
            ProductsDataGrid.DataContext = prodList;
        }
        private void AppExit(object sender, RoutedEventArgs e) {
            app.GracefulShutdown();
        }
        private void OkKlick(object sender, RoutedEventArgs e) {
            app.UploadData();
            MessageBox.Show("Это успех!");
            app.GracefulShutdown();
        }
        #region SINGLE CLICK EDITING
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly) {
                if (!cell.IsFocused) {
                    cell.Focus();
                }
                DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid != null) {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow) {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else {
                        DataGridRow row = FindVisualParent<DataGridRow>(cell);
                        if (row != null && !row.IsSelected) {
                            row.IsSelected = true;
                        }
                    }
                }
            }
        }
        static T FindVisualParent<T>(UIElement element) where T : UIElement {
            UIElement parent = element;
            while (parent != null) {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null) {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
        #endregion

        private void SavePData(object sender, RoutedEventArgs e) {
            db.UpdateDatabase(prodList);
            db.SyncToDrive();
            MessageBox.Show("Данные сохранены успешно.");
        }
        private void LoadPData(object sender, RoutedEventArgs e) {
            foreach (Product product in prodList) {
                if (db.Products.ContainsKey(product.Name)) {
                    Product p = db.Products[product.Name];
                    product.IsUploaded = p.IsUploaded;
                    product.CupsuleMult = p.CupsuleMult;
                    product.MachMult = p.MachMult;
                }
            }
        }
        private void SaveCData(object sender, RoutedEventArgs e) {
            db.UpdateDatabase(custList);
            db.SyncToDrive();
            MessageBox.Show("Данные сохранены успешно.");
        }
        private void LoadCData(object sender, RoutedEventArgs e) {
            foreach (Customer customer in custList) {
                if (db.Customers.ContainsKey(customer.Name)) {
                    Customer c = db.Customers[customer.Name];
                    customer.altName = c.altName;
                    customer.Region = c.Region;
                    customer.City = c.City;
                    customer.IsUploaded = c.IsUploaded;
                }
            }
        }
    }
}