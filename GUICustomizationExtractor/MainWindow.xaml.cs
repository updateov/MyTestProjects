using System;
using System.IO;
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
using Microsoft.Win32;

namespace GUICustomizationExtractor
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

        private void m_buttonBrowseSourceFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_textInBoxPath.Text = dlg.SelectedPath;
            }
        }

        private void m_comboBoxDialogName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableExtractButton();
        }

        private void m_buttonBrowseDestinationFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.SaveFileDialog();
            dlg.Filter = "XML files (*.xml)|*.xml";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_textBoxOutPath.Text = dlg.FileName;
            }
        }

        private void m_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableExtractButton();
        }

        private void EnableExtractButton()
        {
            m_buttonExtract.IsEnabled = m_comboBoxDialogName.SelectedIndex > -1 && !m_textInBoxPath.Text.Equals(String.Empty) && !m_textBoxOutPath.Text.Equals(String.Empty);
        }

        private void m_buttonExtract_Click(object sender, RoutedEventArgs e)
        {
            int ind = m_comboBoxDialogName.SelectedIndex;
            var item = m_comboBoxDialogName.Items[ind] as ComboBoxItem;
            var srcFile = m_textInBoxPath.Text + "\\" + item.Name + ".cs";
            var extractor = new Extractor();
            m_buttonExtract.IsEnabled = false;
            bool bSuccs = extractor.Process(srcFile, m_textBoxOutPath.Text, item.Name, item.Content);
            if (bSuccs)
                Close();
            else
                m_buttonExtract.IsEnabled = true;
        }
    }
}
