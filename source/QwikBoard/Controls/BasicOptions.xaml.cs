using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Korkboard.Controls
{
    /// <summary>
    /// Interaction logic for BasicOptions.xaml
    /// </summary>
    public partial class BasicOptions : UserControl
    {
        public BasicOptions()
        {
            InitializeComponent();
        }

        private void OpenDataFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var runExplorer = new System.Diagnostics.ProcessStartInfo();

            runExplorer.FileName = "explorer.exe";
            runExplorer.Arguments = MainWindow.AppFolder;

            System.Diagnostics.Process.Start(runExplorer);
        }
    }
}
