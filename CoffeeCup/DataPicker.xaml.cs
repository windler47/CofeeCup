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
            xProducts = GetGoods(xmlDoc);
            xCustomers = GetCustomers(xmlDoc);
            List<Customer> dCustomers = xCustomers.Values.ToList<Customer>();
            List<Product> dProducts = xProducts.Values.ToList<Product>();
            CustomersDataGrid.DataContext = dCustomers;
            ProductsDataGrid.DataContext = xProducts.Values;
        }
        public static Dictionary<int, Product> GetGoods(XElement xmlDoc)
        {
            Dictionary<int, Product> result = new Dictionary<int, Product>();
            IEnumerable<XElement> goods =
                from fobject in xmlDoc.Elements("Объект")
                where (string)fobject.Attribute("ИмяПравила") == "Номенклатура"
                select fobject;

            foreach (XElement el in goods)
            {
                #region Qery data from Xml
                bool isGroup = (from el1 in el.Element("Ссылка").Elements("Свойство")
                                where (string)el1.Attribute("Имя") == "ЭтоГруппа" && (string)el1.Element("Значение") == "true"
                                select el1).Any();
                if (isGroup) continue;
                Product tproduct = new Product((string)(from el1 in el.Elements("Свойство")
                                              where (string)el1.Attribute("Имя") == "Наименование"
                                              select el1).ElementAt(0));
                #endregion
                result.Add((int)el.Element("Ссылка").Attribute("Нпп"),tproduct);
            }
            return result;
        }
        public static Dictionary<int, Customer> GetCustomers(XElement xmlDoc)
        {
            Dictionary<int, Customer> result = new Dictionary<int, Customer>();
            IEnumerable<XElement> obj =
                from fobject in xmlDoc.Elements("Объект")
                where (string)fobject.Attribute("ИмяПравила") == "Контрагенты"
                select fobject;
            foreach (XElement el in obj)
            {
                bool isGroup = (from el1 in el.Element("Ссылка").Elements("Свойство")
                                where (string)el1.Attribute("Имя") == "ЭтоГруппа" && (string)el1.Element("Значение") == "true"
                                select el1).Any();
                if (isGroup) continue;
                Customer customer = new Customer((string)(from el1 in el.Elements("Свойство")
                                                          where (string)el1.Attribute("Имя") == "Наименование"
                                                          select el1).ElementAt(0));
                result.Add((int)el.Element("Ссылка").Attribute("Нпп"), customer);
            }
            return result;
        }
        public static List<Realization> GetRealisations(XElement xmlDoc, Dictionary<int, Customer> Customers, Dictionary<int, Product> Products)
        {
            List<Realization> result = new List<Realization>();
            IEnumerable<XElement> obj =
                from fobject in xmlDoc.Elements("Объект")
                where (string)fobject.Attribute("ИмяПравила") == "РеализацияТоваровУслуг"
                select fobject;
            foreach (XElement ProductRealisation in obj)
            {
                bool IsDelited = (from prop in ProductRealisation.Elements("Свойство")
                                  where (string)prop.Attribute("Имя") == "ПометкаУдаления" && (string)prop.Element("Значение") == "true"
                                  select prop).Any();

                bool IsCommited = (from prop in ProductRealisation.Elements("Свойство")
                                   where (string)prop.Attribute("Имя") == "Проведен" && (string)prop.Element("Значение") == "true"
                                   select prop).Any();
                if (IsDelited || !IsCommited) continue;


            }
            return result;
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
