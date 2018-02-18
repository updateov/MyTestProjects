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
using Perigen.Patterns.NnetControls.Screens;

namespace Perigen.Patterns.NnetControls.Controls
{
    /// <summary>
    /// Interaction logic for NavigationPanelUserControl.xaml
    /// </summary>
    public partial class NavigationPanelUserControl : UserControl
    {
        private PatternsUIManager UiManager { get; set; }

        public string PanelTooltip
        {
            get { return (string)GetValue(PanelTooltipProperty); }
            set { SetValue(PanelTooltipProperty, value); }
        }
        public static readonly DependencyProperty PanelTooltipProperty =
            DependencyProperty.Register("PanelTooltip", typeof(string), typeof(NavigationPanelUserControl), new UIPropertyMetadata(null));

        
        public NavigationPanelUserControl(PatternsUIManager uiManager)
        {
            UiManager = uiManager;

            InitializeComponent();
            UiManager.UpdateDataEvent += evUpdateDataEvent;

            DrawChunks();
        }
      
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawChunks();
        }

        void evUpdateDataEvent(object sender, UpdateDataEventArgs e)
        {
            RedrawChunks();
        }

        public void UpdateChunks()
        {
            canvasPanel.Children.Clear();
            borderPanel.InvalidateVisual();
            DrawChunks();
        }

        private void RedrawChunks()
        {
            canvasPanel.Children.Clear();
            DrawChunks();
        }

        private void DrawChunks()
        {
            try
            {
                foreach (IPatternsExportableChunk chunk in UiManager.ExportableChunks)
                {
                    ChunkControl chunkCtrl = new ChunkControl(UiManager,
                                                              chunk.getStartTime(),
                                                              chunk.getTimeRange(),
                                                              chunk.getIsExported(),
                                                              chunk.getIntervalID());

                    chunkCtrl.Height = this.Height - 4;
                    chunkCtrl.Width = chunk.getX2() - chunk.getX1();

                    Canvas.SetLeft(chunkCtrl, chunk.getX1());
                    Canvas.SetTop(chunkCtrl, 0);
                    canvasPanel.Children.Add(chunkCtrl);
                }
            }
            catch { }
        }

        public void SetSelectedChunk(DateTime startTime, bool isSelect)
        {
            foreach (ChunkControl chunk in canvasPanel.Children)
            {
                if (chunk.StartTime == startTime)
                {
                    chunk.SetSelected(isSelect);
                    break;
                }
            }
        }
        public void ShowChunkExported(DateTime startTime)
        {
            foreach (ChunkControl chunk in canvasPanel.Children)
            {
                if (chunk.StartTime == startTime)
                {
                    chunk.IsExported = true;
                    break;
                }
            }
        }
        public ChunkControl GetChunk(DateTime startTime)
        {
            foreach (ChunkControl chunk in canvasPanel.Children)
            {
                if (chunk.StartTime == startTime)
                {
                    return chunk;
                }
            }
            return null;
        }

        public void ReleaseResources()
        {
            UiManager.UpdateDataEvent -= evUpdateDataEvent;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ReleaseResources();
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            if (UiManager.PanelTooltip == String.Empty)
            {
                this.PanelTooltip = null;
            }
            else
            {
                Point p = Mouse.GetPosition(sender as Border);
                double pointInMinutes = (this.Width - p.X) / UiManager.PixelsInMinute;

                DateTime currentTime = UiManager.StripStartTime.AddMinutes(pointInMinutes * -1);

                if (currentTime < UiManager.End36Weeks)
                {
                    this.PanelTooltip = UiManager.PanelTooltip;
                }
                else
                {
                    this.PanelTooltip = null;
                }
            }
        }
    }
}
