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

namespace MediaFrame
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
        /*
        public GUIPictureBox PicBox() // for interface
        {
            return pictureBox1;
        }

        public MainWindow()
        {
            InitializeComponent();

            pictureBox1.Init(); // blank image (color of background, fills the PictureBox). It does react to all the mouse operations. Alternative: no image at start, but add null conditions to all interface event handlers.

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

        // stats for reference / debugging. No mouse coords - shown when mouse is outside picture box
        public void ShowStats()
        {
            //toolStrip1.tstbStatus.Text = string.Format("center: {0}, scale: {1}", RoundPoint(pictureBox1.state.center).ToString(), Math.Round(pictureBox1.state.scale, 2));
            if (pictureBox1.Image == null || !pictureBox1.isImageLoaded)
                statusTextBox1.Text = "no image";
            else
                statusTextBox1.Text = string.Format("center: {0}, scale: {1}", RoundPoint(pictureBox1.state.center).ToString(), Math.Round(pictureBox1.state.scale, 3));
        }

        private void Form1_MouseStatsUpdate(object sender, MouseEventArgs e)
        {
            ShowStats(e.Location);
        }

        // stats with added mouse coords (relative to image's position and scale) - replace the former when mouse is within PictureBox
        private void ShowStats(PointF mouse)
        {
            Point relCoords = RoundPoint(pictureBox1.MouseLocationOnImage(mouse));
            if (pictureBox1.Image == null || !pictureBox1.isImageLoaded)
            {
                statusTextBox1.Text = "no image";
            }
            else
            {
                statusTextBox1.Text = string.Format("center: {0}, scale: {1}, mouse coords: {2}", RoundPoint(pictureBox1.state.center).ToString(), Math.Round(pictureBox1.state.scale, 3), RoundPoint(pictureBox1.MouseLocationOnImage(mouse)));
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
        */
    }
}
