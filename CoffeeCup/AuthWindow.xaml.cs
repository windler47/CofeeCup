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

namespace CoffeeCup
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        CoffeeCup.App app = (CoffeeCup.App)CoffeeCup.App.Current;
        public AuthWindow()
        {
            InitializeComponent();
            textAuthUrl.Text = app.GetGAuthLink();
        }

        private void AuthOKClick(object sender, RoutedEventArgs e)
        {
            app.parameters.AccessCode = GAccessCode.Text;
            app.GAuthStep2();
            this.Close();
        }
    }
}
