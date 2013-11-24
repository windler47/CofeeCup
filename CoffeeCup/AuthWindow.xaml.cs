using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Navigation;

namespace CoffeeCup {
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window {
        CoffeeCup.App app = (CoffeeCup.App)CoffeeCup.App.Current;
        string documentName;
        public AuthWindow(string gDocName) {
            InitializeComponent();
            string authlink = app.GAuthGetLink();
            AuthUrl.NavigateUri = new Uri(authlink);
            textAuthUrl.Text = authlink;
            documentName = gDocName;
        }
        private void AuthOKClick(object sender, RoutedEventArgs e) {
            app.GAuthStep2(GAccessCode.Text);
            try {
                DataPicker tDataPicker = new DataPicker(documentName);
                tDataPicker.Show();
            }
            catch (System.ApplicationException) {
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Question;
                string caption = "Ошибка!";
                string messageBoxText = "Ошибка обращения к Google документу. Нажмите Yes чтобы проигнорировать и попробовать еще раз, No чтобы закрыть приложение.";
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);
                if (result == MessageBoxResult.No) app.GracefulShutdown();
            }
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