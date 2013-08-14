using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Forms;

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
            FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
            { 
                FolderPath.Text = dialog.SelectedPath;
            } 
        }
        private void MainOKClick(object sender, RoutedEventArgs e)
        {
            app.DocUri = DocUri.Text;
            //app.wsID = WSID.Text;
            app.docPath = FolderPath.Text;
            //AuthWindow tAuthWindow = new AuthWindow();
            //tAuthWindow.Show();
            DataPicker tdataPicker = new DataPicker();
            tdataPicker.Show();
            this.Close();
        }
        private void AppExit(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
