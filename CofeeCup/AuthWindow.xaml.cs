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
using Google.GData.Client;
using Google.GData.Spreadsheets;

namespace CofeeCup
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        CofeeCup.App app = (CofeeCup.App)CofeeCup.App.Current;
        public AuthWindow()
        {
            InitializeComponent();       
            textAuthUrl.Text = app.GAuth2();
        }

        private void AuthOKClick(object sender, RoutedEventArgs e)
        {
            app.parameters.AccessCode = GAccessCode.Text;
            OAuthUtil.GetAccessToken(app.parameters);
            GOAuth2RequestFactory requestFactory = new GOAuth2RequestFactory(null, "CoffeeCup", app.parameters);
            SpreadsheetsService service = new SpreadsheetsService("CoffeeCup");
            service.RequestFactory = requestFactory;
            SpreadsheetQuery query = new SpreadsheetQuery();
            SpreadsheetFeed feed = service.Query(query);

        }
    }
}
