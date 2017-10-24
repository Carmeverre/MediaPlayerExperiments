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

            ////TODO
            //pictureBox.Init(); // blank image (color of background, fills the PictureBox). It does react to all the mouse operations. Alternative: no image at start, but add null conditions to all interface event handlers.

            //// stats update event handlers
            //pictureBox.UpdateStats += new EventHandler(this.Form1_StatsUpdate);
            //pictureBox.UpdateMouseStats += new MouseEventHandler(this.Form1_MouseStatsUpdate);
            //ShowStats();
            //this.Invalidate(); // needed? definitely not for the blank image...
    }

    public GuiPictureBox PicBox() // for interface
        {
            return pictureBox;
        }

        

        private void Form1_StatsUpdate(object sender, EventArgs e)
        {
            ShowStats();
        }

        // stats for reference / debugging. No mouse coords - shown when mouse is outside picture box
        public void ShowStats()
        {
            //toolStrip.tstbStatus.Text = string.Format("center: {0}, scale: {1}", RoundPoint(pictureBox.state.center).ToString(), Math.Round(pictureBox1.state.scale, 2));
            if (pictureBox.Image == null || !pictureBox.isImageLoaded)
                status.Text = "no image";
            else
                status.Text = string.Format("center: {0}, scale: {1}", RoundPoint(pictureBox.state.center).ToString(), Math.Round(pictureBox.state.scale, 3));
        }

        private void Form1_MouseStatsUpdate(object sender, MouseEventArgs e)
        {
            ShowStats(new PointD(e.GetPosition(this)));
        }

        // stats with added mouse coords (relative to image's position and scale) - replace the former when mouse is within PictureBox
        private void ShowStats(PointD mouseLoc)
        {
            Point relCoords = RoundPoint(pictureBox.MouseLocationOnImage(mouseLoc));
            if (pictureBox.Image == null || !pictureBox.isImageLoaded)
            {
                status.Text = "no image";
            }
            else
            {
                status.Text = string.Format("center: {0}, scale: {1}, mouse coords: {2}",
                    RoundPoint(pictureBox.state.center).ToString(),
                    Math.Round(pictureBox.state.scale, 3),
                    RoundPoint(pictureBox.MouseLocationOnImage(mouseLoc)));
            }
        }

        // should maybe move this to a static utils sort of class
        private static Point RoundPoint(PointD p)
        {
            return new Point((int)Math.Round(p.X), (int)Math.Round(p.Y));
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (pictureBox == null)
                return;
            if (pictureBox.TryDrag(pictureBox.state.center))
            {
                //pictureBox.Invalidate(); // prevents inconsistent border and image jumps after resize//TODO
            }
            ShowStats();
        }
    }
}
