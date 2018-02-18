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

namespace WpfApplicationRecursive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //var model = new List<ModelItems>()
            //{
            //    new ModelItems()
            //    {
            //        Text = "Level 1V",
            //        Items = new List<ModelItems>()
            //        {
            //            new ModelItems()
            //            {
            //                Text = "Level 2,1V",
            //                Items = new List<ModelItems>()
            //                {
            //                    new ModelItems()
            //                    {
            //                        Text = "Level 3,1,1H",
            //                        Items = new List<ModelItems>()
            //                        {
            //                            new ModelItems() { Text = "Level 4,1,1,1" },
            //                            new ModelItems() { Text = "Level 4,1,1,2" },
            //                            new ModelItems() { Text = "Level 4,1,1,3" }
            //                        }
            //                    },
            //                    new ModelItems()
            //                    {
            //                        Text = "Level 3,1,2H",
            //                        Items = new List<ModelItems>()
            //                        {
            //                            new ModelItems() { Text = "Level 4,1,2,1" },
            //                            new ModelItems() { Text = "Level 4,1,2,2" },
            //                            new ModelItems() { Text = "Level 4,1,2,3" },
            //                            new ModelItems() { Text = "Level 4,1,2,4" }
            //                        }
            //                    },
            //                    new ModelItems()
            //                    {
            //                        Text = "Level 3,1,3H",
            //                        Items = new List<ModelItems>()
            //                        {
            //                            new ModelItems() { Text = "Level 4,1,3,1" },
            //                            new ModelItems() { Text = "Level 4,1,3,2" }
            //                        }
            //                    }
            //                }
            //            },
            //            new ModelItems()
            //            {
            //                Text = "Level 2,2V",
            //                Items = new List<ModelItems>()
            //                {
            //                    new ModelItems()
            //                    {
            //                        Text = "Level 3,2,1H",
            //                        Items = new List<ModelItems>()
            //                        {
            //                            new ModelItems() { Text = "Level 4,2,1,1" },
            //                            new ModelItems() { Text = "Level 4,2,1,2" },
            //                            new ModelItems() { Text = "Level 4,2,1,3" }
            //                        }
            //                    },
            //                    new ModelItems()
            //                    {
            //                        Text = "Level 3,2,2H",
            //                        Items = new List<ModelItems>()
            //                        {
            //                            new ModelItems() { Text = "Level 4,2,2,1" },
            //                            new ModelItems() { Text = "Level 4,2,2,2" },
            //                            new ModelItems() { Text = "Level 4,2,2,3" }
            //                        }
            //                    },
            //                    new ModelItems()
            //                    {
            //                        Text = "Level 3,2,3H",
            //                        Items = new List<ModelItems>()
            //                        {
            //                            new ModelItems() { Text = "Level 4,2,3,1" },
            //                            new ModelItems() { Text = "Level 4,2,3,2" },
            //                            new ModelItems() { Text = "Level 4,2,3,3" }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    },
            //    new ModelItems() { Text = "Level 2V" },
            //    new ModelItems() { Text = "Level 3V" }
            //};

            //DataContext = model;

        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            m_timer.Tick += m_timer_Tick;
            m_timer.Interval = new TimeSpan(0,0,10);
            m_timer.Start();            
        }

        void m_timer_Tick(object sender, EventArgs e)
        {
            Navigate();
            if (++m_cnt > 1000)
                m_timer.Stop();
        }

        System.Windows.Threading.DispatcherTimer m_timer = new System.Windows.Threading.DispatcherTimer();
        private int m_cnt = 0;

        private void Navigate()
        {
                youtubeBrowser.Navigate("https://www.youtube.com/watch?v=KuFRYDjhGF4&feature=youtu.be");
        }
    }
}
