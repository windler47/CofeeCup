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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.GData.Client;
using Google.GData.Spreadsheets;

namespace CofeeCup
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            string CLIENT_ID = "19361090870.apps.googleusercontent.com";
            string CLIENT_SECRET = "CZuF5r88V_6JGsP3pFlnoYDl";

            OAuth2Parameters parameters = new OAuth2Parameters();

        }
    }
}
