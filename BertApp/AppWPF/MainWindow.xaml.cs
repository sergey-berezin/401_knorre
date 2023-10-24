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
using ViewModel;

namespace AppWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IUIServices
    {
        public MainWindow()
        {
            InitializeComponent();
            MainViewModel viewModel = new MainViewModel(this);
            DataContext = viewModel;
        }
        public string? ChooseFileToOpen()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                FileName = "Hobbit", 
                DefaultExt = ".txt"
            };
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                return openFileDialog.FileName;
            }
            return null;
        }
    }
}
