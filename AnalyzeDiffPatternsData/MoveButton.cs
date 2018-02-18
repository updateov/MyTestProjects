using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace CALMDevTool
{
    public partial class MoveButton : Button
    {
        public enum MoveDirection
        {
            MoveNext,
            MovePrev,
            MoveUp,
            MoveDown
        }

        public MoveButton()
        {
            InitializeComponent();
            m_curSize = Size;
            Text = "";
            m_ButtonDirection = MoveDirection.MoveNext;
        }


        private Size m_curSize;
        private MoveDirection m_ButtonDirection;
        public MoveDirection ButtonDirection
        {
            get { return m_ButtonDirection; }
            set
            {
                m_ButtonDirection = value;
                Refresh();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Size.Width != m_curSize.Width)
            {
                m_curSize.Height = Size.Width;
                m_curSize.Width = Size.Width;
            }
            else
            {
                m_curSize.Width = Size.Height;
                m_curSize.Height = Size.Height;
            }
            Size = m_curSize;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Text = "";
            base.OnPaint(pevent);
            switch (m_ButtonDirection)
            {
                case MoveDirection.MoveNext:
                    PaintNext(pevent.Graphics);
                    break;
                case MoveDirection.MovePrev:
                    PaintPrev(pevent.Graphics);
                    break;
                case MoveDirection.MoveUp:
                    PaintUp(pevent.Graphics);
                    break;
                case MoveDirection.MoveDown:
                    PaintDown(pevent.Graphics);
                    break;
                default:
                    break;
            }
        }

        private void PaintNext(Graphics graphics)
        {
            Point upperLeft = new Point(ClientRectangle.X + ClientRectangle.Width / 3, 
                                        ClientRectangle.Y + ClientRectangle.Height / 3);
            Point bottomLeft = new Point(ClientRectangle.X + ClientRectangle.Width / 3, 
                                    ClientRectangle.Y + ClientRectangle.Height - ClientRectangle.Height / 3);
            Point Right = new Point(ClientRectangle.X + ClientRectangle.Width - ClientRectangle.Width / 3,
                                    ClientRectangle.Y + ClientRectangle.Height / 2);

            graphics.FillPolygon(new LinearGradientBrush(upperLeft, bottomLeft,
                            //new Point(ClientRectangle.X + ClientRectangle.Width - ClientRectangle.Width / 3,
                            //        ClientRectangle.Y + ClientRectangle.Height - ClientRectangle.Height / 3),
                            Color.White, Enabled ? Color.Blue : Color.DarkGray),
                            new Point[] { upperLeft, Right, bottomLeft });
            graphics.DrawPolygon(new Pen(Enabled ? Color.Black : Color.DarkGray), 
                                new Point[] { upperLeft, Right, bottomLeft });
        }

        private void PaintPrev(Graphics graphics)
        {
            Point upperRight = new Point(ClientRectangle.X + ClientRectangle.Width - ClientRectangle.Width / 3,
                                        ClientRectangle.Y + ClientRectangle.Height / 3);
            Point bottomRight = new Point(ClientRectangle.X + ClientRectangle.Width - ClientRectangle.Width / 3,
                                    ClientRectangle.Y + ClientRectangle.Height - ClientRectangle.Height / 3);
            Point Left = new Point(ClientRectangle.X + ClientRectangle.Width / 3,
                                    ClientRectangle.Y + ClientRectangle.Height / 2);

            graphics.FillPolygon(new LinearGradientBrush(upperRight,
                            new Point(ClientRectangle.X + ClientRectangle.Width - ClientRectangle.Width / 3,
                                    ClientRectangle.Y + ClientRectangle.Height - ClientRectangle.Height / 3),
                                    Color.White, Enabled ? Color.Blue : Color.DarkGray),
                            new Point[] { upperRight, bottomRight, Left });
            graphics.DrawPolygon(new Pen(Enabled ? Color.Black : Color.DarkGray), 
                                new Point[] { upperRight, bottomRight, Left });
        }

        private void PaintUp(Graphics graphics)
        {
            Point bottomRight = new Point(ClientRectangle.X + ClientRectangle.Width / 3,
                                        ClientRectangle.Y + ClientRectangle.Height - ClientRectangle.Height / 3);
            Point bottomLeft = new Point(ClientRectangle.X + ClientRectangle.Width - ClientRectangle.Width / 3,
                                    ClientRectangle.Y + ClientRectangle.Height - ClientRectangle.Height / 3);
            Point Upper = new Point(ClientRectangle.X + ClientRectangle.Width / 2,
                                    ClientRectangle.Y + ClientRectangle.Height / 3);

            graphics.FillPolygon(new LinearGradientBrush(Upper,
                            new Point(ClientRectangle.X + ClientRectangle.Width - ClientRectangle.Width / 3,
                                    ClientRectangle.Y + ClientRectangle.Height - ClientRectangle.Height / 3),
                                    Color.White, Enabled ? Color.Blue : Color.DarkGray),
                            new Point[] { Upper, bottomRight, bottomLeft });
            graphics.DrawPolygon(new Pen(Enabled ? Color.Black : Color.DarkGray),
                                new Point[] { Upper, bottomRight, bottomLeft });
        }

        private void PaintDown(Graphics graphics)
        {
            Point upperLeft = new Point(ClientRectangle.X + ClientRectangle.Width / 3,
                                        ClientRectangle.Y + ClientRectangle.Height / 3);
            Point upperRight = new Point(ClientRectangle.X + ClientRectangle.Width - ClientRectangle.Width / 3,
                                    ClientRectangle.Y + ClientRectangle.Height / 3);
            Point Bottom = new Point(ClientRectangle.X + ClientRectangle.Width / 2,
                                    ClientRectangle.Y + ClientRectangle.Width - ClientRectangle.Height / 3);

            graphics.FillPolygon(new LinearGradientBrush(upperLeft,
                            new Point(ClientRectangle.X + ClientRectangle.Width - ClientRectangle.Width / 3,
                                    ClientRectangle.Y + ClientRectangle.Height - ClientRectangle.Height / 3),
                                    Color.White, Enabled ? Color.Blue : Color.DarkGray),
                            new Point[] { upperLeft, upperRight, Bottom });
            graphics.DrawPolygon(new Pen(Enabled ? Color.Black : Color.DarkGray),
                                new Point[] { upperLeft, upperRight, Bottom });
        }

    }
}
