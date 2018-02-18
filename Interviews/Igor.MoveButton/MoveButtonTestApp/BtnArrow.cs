using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace MoveButtonTestApp
{
    public enum BtnDirection
    {
        Left,
        Up,
        Right,
        Down
    }

    public partial class BtnArrow : UserControl
    {
        public BtnDirection Direction { get; set; }

        public BtnArrow()
        {
            InitializeComponent();    
        }

        public void DrawArrow(BtnDirection direction)
        {
            Direction = direction;

            drawUpTriangle();
        }

        private void drawUpTriangle()
        {
            Graphics surface;
            surface = this.CreateGraphics();

            Point[] points = GetPoints();
            PathGradientBrush gradientBrush = new PathGradientBrush(points);
            Color[] colors = { Color.Red, Color.Green, Color.Blue };
            gradientBrush.CenterColor = Color.Black;
            gradientBrush.SurroundColors = colors;

            surface.FillPolygon(gradientBrush, points);
        }

        private void drawDownTriangle()
        {
            Graphics surface;
            surface = this.CreateGraphics();

            Point[] points = GetPoints();
            PathGradientBrush gradientBrush = new PathGradientBrush(points);
            Color[] colors = {  Color.Green, Color.Blue, Color.Red};
            gradientBrush.CenterColor = Color.Black;
            gradientBrush.SurroundColors = colors;

            surface.FillPolygon(gradientBrush, points);
        } 

        private Point[] GetPoints()
        {
            if(Direction == BtnDirection.Left)
            {
                return new Point[] { new Point(45, 5), new Point(5, 25), new Point(45, 45) };
            }
            else if(Direction == BtnDirection.Up)
            {
                return new Point[] { new Point(5, 45), new Point(25, 5), new Point(45, 45) };
            }
            else if (Direction == BtnDirection.Right)
            {
                return new Point[] { new Point(5, 5), new Point(5, 45), new Point(45, 25) };
            }          
            return new Point[] { new Point(5, 5), new Point(45, 5), new Point(25, 45) };
        }

        private void BtnArrow_Load(object sender, EventArgs e)
        {

        }

        private void BtnArrow_Click(object sender, EventArgs e)
        {
  
        }

        private void BtnArrow_MouseDown(object sender, MouseEventArgs e)
        {
            drawDownTriangle();
        }

        private void BtnArrow_MouseUp(object sender, MouseEventArgs e)
        {
            drawUpTriangle();
        }
    }
}
