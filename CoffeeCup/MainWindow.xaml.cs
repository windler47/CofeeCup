using System;
using System.Windows;

namespace CoffeeCup
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CoffeeCup.App app = (CoffeeCup.App)CoffeeCup.App.Current;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FolderBrowserButtonClick(object sender, RoutedEventArgs e)
        {
            //Open Folder Broser Dialog
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".xml";
            dialog.Filter = "XML Files (*.xml)|*.xml";
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true) 
            {
                FolderPath.Text = dialog.FileName;
            } 
        }
        private void MainOKClick(object sender, RoutedEventArgs e)
        {
            app.DocUri = DocUri.Text;
            app.docPath = FolderPath.Text;
            if (app.LoadGRefreshToken()) {
                AuthWindow tAuthWindow = new AuthWindow();
                tAuthWindow.Show();
            }
            else {
                DataPicker tdataPicker = new DataPicker();
                tdataPicker.Show();
            }
            this.Close();
        }
        private void AppExit(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
