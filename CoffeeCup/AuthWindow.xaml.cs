using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Navigation;

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
            string authlink = app.GAuthGetLink();
            AuthUrl.NavigateUri = new Uri(authlink);
            textAuthUrl.Text = authlink;
        }
        private void AuthOKClick(object sender, RoutedEventArgs e)
        {
            app.GAuthStep2(GAccessCode.Text);
            DataPicker tDataPicker = new DataPicker();
            tDataPicker.Show();
            this.Close();
        }
        private void AppExit(object sender, RoutedEventArgs e) {
            System.Windows.Application.Current.Shutdown();
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            System.Diagnostics.Process.Start(e.Uri.ToString());     
        }
    }
    public class HalfConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            double hw = ((double)value) / 2;
            return hw;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new System.NotImplementedException();
        }
    }
}