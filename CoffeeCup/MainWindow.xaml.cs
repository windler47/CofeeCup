using System;
using System.Windows;

namespace CoffeeCup {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        CoffeeCup.App app = (CoffeeCup.App)CoffeeCup.App.Current;
        public MainWindow() {
            InitializeComponent();
        }
        public MainWindow(string docUri, string docPath) {
            InitializeComponent();
            DocUri.Text = docUri;
            FolderPath.Text = docPath;
        }
        private void FolderBrowserButtonClick(object sender, RoutedEventArgs e) {
            //Open Folder Broser Dialog
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".xml";
            dialog.Filter = "XML Files (*.xml)|*.xml";
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true) {
                FolderPath.Text = dialog.FileName;
            }
        }
        private void MainOKClick(object sender, RoutedEventArgs e) {
            bool allOk;
            if (app.InitializexmlDoc(FolderPath.Text)) {
                MessageBox.Show("Произошла ошибка при открытии файла с данными. Проверьте правильность пути к файлу.");
            }
            else {
                if (!app.LoadGRefreshToken()) {
                    try {
                        DataPicker tdataPicker = new DataPicker(DocUri.Text);
                        tdataPicker.Show();
                        allOk = true;
                    }
                    catch (System.ApplicationException) {
                        MessageBoxButton button = MessageBoxButton.YesNo;
                        MessageBoxImage icon = MessageBoxImage.Question;
                        string caption = "Ошибка!";
                        string messageBoxText = "Возникла ошибка при обращении к Google документу. Одна из вероятных причин возникновения этой ошибки - просроченные данные авторизации. Нажмите Yes чтобы сбросить данные и авторизвать приложение повторно, No чтобы попробовать еще раз с текущими данными.";
                        MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);
                        if (result == MessageBoxResult.Yes) app.RemoveSavedRefreshToken();
                        allOk = false;
                    }
                }
                else {
                    AuthWindow tAuthWindow = new AuthWindow(DocUri.Text);
                    tAuthWindow.Show();
                    allOk = true;
                }
                if (allOk) {
                    this.Close();
                }
            }
        }
        private void AppExit(object sender, RoutedEventArgs e) {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
