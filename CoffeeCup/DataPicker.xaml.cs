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
        Dictionary<int, string> dgoods = new Dictionary<int, string>();
        public DataPicker()
        {
            InitializeComponent();
            XElement xmlDoc = XElement.Load(app.docPath);
            dgoods = GetGoods(xmlDoc);
            this.DataContext = dgoods;
            tDataGrid.ItemsSource = dgoods;
        }
        public static Dictionary<int, string> GetGoods(XElement xmlDoc)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
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
                string productName = (string)(from el1 in el.Elements("Свойство")
                                              where (string)el1.Attribute("Имя") == "Наименование"
                                              select el1).ElementAt(0);
                #endregion
                //Console.WriteLine("{0} Товар: {1}", el.Element("Ссылка").Attribute("Нпп"), productName);
                result.Add((int)el.Element("Ссылка").Attribute("Нпп"), productName);
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
    }
}
