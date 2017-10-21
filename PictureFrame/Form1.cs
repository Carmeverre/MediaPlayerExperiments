using System;
using System.Drawing;
using System.Windows.Forms;

namespace PictureFrame
{
    public partial class Form1 : Form, GUIForm
    {


        public GUIPictureBox PicBox() // for interface
        {
            return pictureBox1;
        }

        public Form1()
        {
            InitializeComponent();

            pictureBox1.Init(); // blank image (color of background, fills the PictureBox). It does react to all the mouse operations. Alternative: no image at start, but add null conditions to all interface event handlers.
                                //toolStrip1.tsb1.ClickMe(); // alternative where user must pick image at start. Not great, selecting nothing may cause a null exception.            

            // stats update event handlers
            pictureBox1.UpdateStats += new EventHandler(this.Form1_StatsUpdate);
            pictureBox1.UpdateMouseStats += new MouseEventHandler(this.Form1_MouseStatsUpdate);
            ShowStats();
            //this.Invalidate(); // needed? definitely not for the blank image...
        }

        private void Form1_StatsUpdate(object sender, EventArgs e)
        {
            ShowStats();
        }

        // TODO: uncomment and reenable with different brightness reading?
        //// stats for reference / debugging. No mouse coords - shown when mouse is outside picture box
        //public void ShowStats()
        //{
        //    //toolStrip1.tstbStatus.Text = string.Format("center: {0}, scale: {1}", RoundPoint(pictureBox1.state.center).ToString(), Math.Round(pictureBox1.state.scale, 2));
        //    if (pictureBox1.imgArr == null)
        //        statusTextBox1.Text = "no image";
        //    else
        //        statusTextBox1.Text = string.Format("center: {0}, scale: {1}", RoundPoint(pictureBox1.state.center).ToString(), Math.Round(pictureBox1.state.scale, 3));
        //}

        // temporary replacement
        public void ShowStats()
        {
            if (pictureBox1.Image == null)
            {
                statusTextBox1.Text = "no image";
            }
            else
            {
                statusTextBox1.Text = "";
            }
        }

        private void Form1_MouseStatsUpdate(object sender, MouseEventArgs e)
        {
            ShowStats(e.Location);
        }

        // TODO: uncomment and reenable with different brightness reading?
        //// stats with added mouse coords (relative to image's position and scale) - replace the former when mouse is within PictureBox
        //private void ShowStats(PointF mouse)
        //{
        //    //toolStrip1.tstbStatus.Text = string.Format("center: {0}, scale: {1}, mouse coords: {2}", RoundPoint(pictureBox1.state.center).ToString(), Math.Round(pictureBox1.state.scale, 2), RoundPoint(pictureBox1.MouseLocationOnImage(mouse)));
        //    Point relCoords = RoundPoint(pictureBox1.MouseLocationOnImage(mouse));
        //    if (pictureBox1.imgArr == null)
        //    {
        //        statusTextBox1.Text = "no image";
        //    }
        //    else
        //    {
        //        short ptBright = pictureBox1.imgArr.Point(relCoords.X, relCoords.Y);
        //        if (ptBright == -1)
        //            statusTextBox1.Text = string.Format("center: {0}, scale: {1}, mouse coords: {2}", RoundPoint(pictureBox1.state.center).ToString(), Math.Round(pictureBox1.state.scale, 3), relCoords);
        //        else
        //            statusTextBox1.Text = string.Format("center: {0}, scale: {1}, mouse coords: {2}, brightness: {3}", RoundPoint(pictureBox1.state.center).ToString(), Math.Round(pictureBox1.state.scale, 3), relCoords, ptBright);
        //    }
        //}

        // temporary replacement
        private void ShowStats(PointF mouse)
        {
            if (pictureBox1.Image == null)
            {
                statusTextBox1.Text = "no image";
            }
            else
            {
                statusTextBox1.Text = "";
            }
        }

        // should maybe move this to a static utils sort of class
        private static Point RoundPoint(PointF p)
        {
            return new Point((int)Math.Round(p.X), (int)Math.Round(p.Y));
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (pictureBox1 == null)
                return;
            if (pictureBox1.TryDrag(pictureBox1.state.center))
            {
                pictureBox1.Invalidate(); // prevents inconsistent border and image jumps after resize
            }
            ShowStats();
        }
    }
}
