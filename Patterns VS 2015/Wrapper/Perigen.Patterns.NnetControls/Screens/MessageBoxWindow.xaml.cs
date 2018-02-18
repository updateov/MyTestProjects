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
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace Perigen.Patterns.NnetControls.Screens
{
    /// <summary>
    /// Interaction logic for MessageBoxWindow.xaml
    /// </summary>
    public partial class MessageBoxWindow : Window
    {
        private Storyboard m_gridFadeInStoryBoard;

        public MessageBoxWindow()
        {
            InitializeComponent();

            this.KeyUp += MessageBoxWindow_KeyUp;
            uiGridMain.MouseLeftButtonDown += delegate { this.DragMove(); };
        }

        void MessageBoxWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                this.Close();
            }
        }

        private void messageBoxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("gridFadeInMessage");
            m_gridFadeInStoryBoard.Begin();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
