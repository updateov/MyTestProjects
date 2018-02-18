using Microsoft.Win32;
using ReviewManager;
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

namespace Review
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML documents |*.xml|In Files |*.in|V01 Files |*.V01"; // Filter files by extension
            bool? bRes = dlg.ShowDialog();

            if (bRes.HasValue && bRes.Value)
                EpisodesManager.Instance.ProcessRetrospective(dlg.FileName);
        }
    }
}
