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
using System.IO;
using System.Xml;
using System.Data;

namespace CoffeeCup
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class ListSpreadsheets: Window
    {
        CoffeeCup.App app = (CoffeeCup.App)CoffeeCup.App.Current;
        public ListSpreadsheets()
        {
            InitializeComponent();
            XmlReader xmlFile = XmlReader.Create(app.FindWorksheet());
            DataSet ds = new DataSet();
            ds.ReadXml(xmlFile);
            dataGrid1.ItemsSource = ds.Tables["title"];
        }
    }
}
