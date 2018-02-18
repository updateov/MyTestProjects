using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChartControl1
{
    public partial class ChartCtrl : UserControl
    {
        private const int UPPER_FHR = 40;
        private const int FHR_VERT_SPACE = 7;
        private const int LOWER_FHR = 187;
        private const float FHR_RATIO = 0.7f;

        private const int UPPER_UP = 210;
        private const int UP_VERT_SPACE = 9;
        private const int LOWER_UP = 300;


        public DateTime AbsoluteStartTime { get; set; }
        public long AbsoluteStart { get; set; }
        private Font m_font = new Font("Microsoft Sans Serif", 7.5f);

        private List<byte> FHRToDraw { get; set; }
        public List<byte> UPToDraw { get; set; }

        private bool m_bDraggingMainStrip = false;

        public ChartCtrl()
        {
            FHRToDraw = new List<byte>();
            UPToDraw = new List<byte>();
            
            InitializeComponent();

            DateTime now = DateTime.Now;
            AbsoluteStartTime = now;
            AbsoluteStart = now.ToEpoch();
        }

        public void AddDataToStrip(IEnumerable<byte> fhr, IEnumerable<byte> up)
        {
            FHRToDraw.AddRange(fhr);
            Refresh();
        }

        private void ChartCtrl_Paint(object sender, PaintEventArgs e)
        {
            DateTime now = DateTime.Now;
            var width = this.Width;
            var height = this.Height;

            DrawMainGrid(e.Graphics, now);

            DrawFHR(e.Graphics, now);

            DrawGridData(e.Graphics, now);
        }

        private void DrawMainGrid(Graphics gc, DateTime now)
        {

            DrawHorizontals(gc);

            int secs = now.Second % 10;
            var delta = Width / 180f;
            var partDelta = delta / 10;
            if (FHRToDraw.Count <= 0)
                DrawVerticalInitial(gc, secs, delta, partDelta);
            else
            {
                DrawVerticalLive(gc, secs, delta, partDelta, now);
            }
        }

        private void DrawHorizontals(Graphics gc)
        {
            for (int y = UPPER_FHR; y <= LOWER_FHR; y += FHR_VERT_SPACE)
            {
                if ((y - UPPER_FHR) % (6 * FHR_VERT_SPACE) == 0 || y == LOWER_FHR)
                    gc.DrawLine(new Pen(Color.FromArgb(128, 10, 10, 10), 0.15f), new Point(0, y), new Point(this.Width, y));
                else
                    gc.DrawLine(new Pen(Color.FromArgb(90, 90, 90, 90), 0.15f), new Point(0, y), new Point(this.Width, y));
            }

            for (int y = UPPER_UP; y <= LOWER_UP; y += UP_VERT_SPACE)
            {
                if ((y - UPPER_UP) % (5 * UP_VERT_SPACE) == 0)
                    gc.DrawLine(new Pen(Color.FromArgb(128, 10, 10, 10), 0.15f), new Point(0, y), new Point(this.Width, y));
                else
                    gc.DrawLine(new Pen(Color.FromArgb(90, 90, 90, 90), 0.15f), new Point(0, y), new Point(this.Width, y));
            }

        }

        private void DrawVerticalInitial(Graphics gc, int secs, float delta, float partDelta)
        {
            for (float x = 0; x < Width; x += delta)
            {
                float xPoint = x + (delta - (partDelta * secs));
                if (xPoint > Width)
                    break;

                gc.DrawLine(new Pen(Color.FromArgb(90, 90, 90, 90), 0.15f), new PointF(xPoint, UPPER_FHR), new PointF(xPoint, LOWER_FHR));
                gc.DrawLine(new Pen(Color.FromArgb(90, 90, 90, 90), 0.15f), new PointF(xPoint, UPPER_UP), new PointF(xPoint, LOWER_UP));
            }
        }

        private void DrawVerticalLive(Graphics gc, int secs, float delta, float partDelta, DateTime now)
        {
            int nextRound = 0;
            var leftBound = now.AddSeconds(-1800);
            int nextMin = leftBound.Minute;
            if (secs % 10 == 0)
            {
                nextRound = leftBound.Second + 10;
            }
            else
            {
                nextRound = leftBound.Second + (10 - secs);
            }

            if (nextRound % 60 == 0)
                ++nextMin;

            for (float x = 0; x < Width; x += delta)
            {
                float xPoint = x + (delta - (partDelta * secs));
                if (xPoint > Width)
                    break;

                if (nextRound % 60 == 0)
                {
                    gc.DrawLine(new Pen(Color.FromArgb(128, 10, 10, 10), 0.15f), new PointF(xPoint, UPPER_FHR), new PointF(xPoint, LOWER_FHR));
                    gc.DrawLine(new Pen(Color.FromArgb(128, 10, 10, 10), 0.15f), new PointF(xPoint, UPPER_UP), new PointF(xPoint, LOWER_UP));
                    if (nextMin % 3 == 0)
                    {
                        int val = 240;
                        for (float i = UPPER_FHR - 5f; i < LOWER_FHR; i += (3f * FHR_VERT_SPACE))
                        {
                            gc.FillRectangle(new SolidBrush(Color.White), val >= 100 ? xPoint - 25 : xPoint - 20, i, val >= 100 ? 21f : 16f, 11f);
                            gc.DrawString(val.ToString(), m_font, new SolidBrush(Color.FromArgb(170, 10, 10, 10)), val >= 100 ? xPoint - 25 : xPoint - 20, i - 1);
                            val -= 30;
                        }

                        val = 100;
                        for (float i = UPPER_UP - 5f; i < LOWER_UP; i += (2.5f * UP_VERT_SPACE))
                        {
                            if (val == 0)
                            {
                                gc.FillRectangle(new SolidBrush(Color.White), xPoint - 15, i, 11f, 11f);
                                gc.DrawString(val.ToString(), m_font, new SolidBrush(Color.FromArgb(170, 10, 10, 10)), xPoint - 14, i - 1);
                            }
                            else
                            {
                                gc.FillRectangle(new SolidBrush(Color.White), val >= 100 ? xPoint - 25 : xPoint - 20, i, val >= 100 ? 21f : 16f, 11f);
                                gc.DrawString(val.ToString(), m_font, new SolidBrush(Color.FromArgb(170, 10, 10, 10)), val >= 100 ? xPoint - 25 : xPoint - 20, i - 1);
                            }

                            val -= 25;
                        }
                    }
                }
                else
                {
                    gc.DrawLine(new Pen(Color.FromArgb(90, 90, 90, 90), 0.15f), new PointF(xPoint, UPPER_FHR), new PointF(xPoint, LOWER_FHR));
                    gc.DrawLine(new Pen(Color.FromArgb(90, 90, 90, 90), 0.15f), new PointF(xPoint, UPPER_UP), new PointF(xPoint, LOWER_UP));
                }

                //if (nextRound % 60 == 0 && nextMin % 3 == 0)
                //{
                //}

                nextRound += 10;
                nextRound %= 60;
                if (nextRound == 0)
                {
                    ++nextMin;
                    nextMin %= 60;
                }
            }
        }

        private void DrawFHR(Graphics gc, DateTime now)
        {
            if (FHRToDraw == null || FHRToDraw.Count <= 0)
                return;

            //DateTime rightBoundTime = AbsoluteStartTime.AddSeconds(FHRToDraw.Count / 4);
            DateTime rightBoundTime = now;
            DateTime leftBoundTime = rightBoundTime.AddSeconds(-1800);

            var delta = Width / 180f;
            var partDelta = delta / 10;

            int drawStartIndex = 0;
            float drawStartPoint = 0f;
            if (FHRToDraw.Count > 7200)
            {
                drawStartIndex = FHRToDraw.Count - 7200;
            }
            else
            {
                drawStartPoint = GetDrawingStartPoint(FHRToDraw.Count, 4);
            }

            float step = Width / 7200f;
            for (int i = drawStartIndex; i < FHRToDraw.Count; i++)
            {
                float y = LOWER_FHR - (FHRToDraw[i] - UPPER_FHR) * FHR_RATIO;
                //gc.FillEllipse(new SolidBrush(Color.FromArgb(10, 5, 5, 5)), drawStartPoint, 60f, step, step);
                if (y >= UPPER_FHR && y <= LOWER_FHR)
                    gc.FillEllipse(new SolidBrush(Color.Black), drawStartPoint, y, 1.5f, 1.5f);

                drawStartPoint += step;
            }
        }

        private void DrawUP(Graphics gc)
        {

        }

        private void DrawGridData(Graphics gc, DateTime now)
        {
            if (now.Minute % 3 == 0 && now.Second == 0)
            {

            }
        }

        private float GetDrawingStartPoint(int stripSize, int signalFrequency)
        {
            if (stripSize >= 1800 * signalFrequency)
                return 0f;

            int shift = (1800 * signalFrequency) - stripSize;
            float startPoint = (Width / (1800f * signalFrequency)) * shift;
            return startPoint;
        }

        private void ChartCtrl_Resize(object sender, EventArgs e)
        {
            Refresh();
        }

        #region Drag strip

        private int m_prevX = 0;

        private void ChartCtrl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Y <= 250)
                m_bDraggingMainStrip = true;

            m_prevX = e.X;
        }

        private void ChartCtrl_MouseUp(object sender, MouseEventArgs e)
        {
            m_bDraggingMainStrip = false;
        }

        private void ChartCtrl_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bDraggingMainStrip)
            {
                
            }
        }

        #endregion
    }
}
