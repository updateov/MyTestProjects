
﻿// Houssem Dellai 
// houssem.dellai@ieee.org 
// +216 95 325 964 
// Studying Software Engineering 
// in the National Engineering School of Sfax (ENIS) 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Metro_Style_Web_Browser
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        double buttonWidth = 93d;
        double margin = 5d;

        public MainPage()
        {
            this.InitializeComponent();
        }
        
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {   
            
        }

        private void pointerMovedEvent(object sender, PointerRoutedEventArgs e)
        {
            Button b = (Button)sender;

            if (b.Width == buttonWidth)
            {
                b.Margin = new Thickness(b.Margin.Left - margin, b.Margin.Top - margin / 2, b.Margin.Right, b.Margin.Bottom);
                b.Height += margin;
                b.Width += 2 * margin;
                b.FontSize += margin;
            }
        }

        private void pointerExitedEvent(object sender, PointerRoutedEventArgs e)
        {
            Button b = (Button)sender;

            if (b.Width == buttonWidth + 2 * margin)
            {
                b.Margin = new Thickness(b.Margin.Left + margin, b.Margin.Top + margin / 2, b.Margin.Right, b.Margin.Bottom);
                b.Height -= margin;
                b.Width -= 2 * margin;
                b.FontSize -= margin;
            }
        }

        private void newB_Click(object sender, RoutedEventArgs e)
        {
            tb.Text = "";
        }

        private void goB_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("http://" + tb.Text);
            try
            {
                wv1.Navigate(uri);
            }
            catch (Exception ex)
            {
                tb.Text = "invalid adress !";
            }
        }

        private void exitB_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void helpB_Click(object sender, RoutedEventArgs e)
        {
            tb.Text = "http://code.msdn.microsoft.com/site/search?f%5B0%5D.Type=User&f%5B0%5D.Value=Houssem%20Dellai";
            wv1.Navigate(new Uri("http://code.msdn.microsoft.com/site/search?f%5B0%5D.Type=User&f%5B0%5D.Value=Houssem%20Dellai"));
        }
    }
}
