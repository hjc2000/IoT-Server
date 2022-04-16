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

namespace WpfApp2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        RenameFilesAndFolders _renameFilesAndFolders = new RenameFilesAndFolders();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _renameFilesAndFolders;
        }

        private void ruleInput_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                _renameFilesAndFolders.Rule = ruleInput.Text;
                ruleInput.Text = "";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _renameFilesAndFolders.HandleClick();
        }

        private void pathInput_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Enter)
            {
                _renameFilesAndFolders.UserPath = pathInput.Text;
            }
        }
    }
}
