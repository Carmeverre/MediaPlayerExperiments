using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using WIA;
using System.IO;

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

        #region testing

        //// create a new WIA common dialog box for the user to select a device from
        //WIA.CommonDialog dialog = new WIA.CommonDialog();

        //// debugging button
        //// previously: testing contrast/brightness
        //// currently: testing scanner dialog
        //// select scanner and print some stats
        //private void button1_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        // show user the WIA device dialog
        //        d = dialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, true, false);

        //        // check if a device was selected
        //        if (d != null)
        //        {
        //            label1.Text = "selection: success!";
        //            // Print camera properties
        //            textBox1.AppendText("\n\n Print properties:\n");
        //            foreach (Property p in d.Properties)
        //            {
        //                textBox1.AppendText(p.Name + ": " + p.get_Value() + "  (" + p.PropertyID + ":" + p.IsReadOnly + ") \n");

        //                //// Update UI
        //                //if (p.PropertyID == 3) _label = (String)p.get_Value();
        //                //if (p.PropertyID == 4) _label = _label + " - " + p.get_Value();
        //                //this.label1.Text = _label;
        //            }

        //            // Print commands
        //            textBox1.AppendText("\n\n Print commands:\n");
        //            foreach (DeviceCommand dvc in d.Commands)
        //            {
        //                textBox1.AppendText(dvc.Name + ": " + dvc.Description + "  (" + dvc.CommandID + ") \n");
        //            }

        //            // Print events
        //            textBox1.AppendText("\n\n Print events:\n");
        //            foreach (DeviceEvent dve in d.Events)
        //            {
        //                textBox1.AppendText(dve.Name + ": " + dve.Description + "  (" + dve.Type + ") \n");
        //            }

        //            // Print item properties
        //            textBox1.AppendText("\n\n Print item properties:\n");
        //            foreach (Property item in d.Items[1].Properties)
        //            {
        //                textBox1.AppendText(item.IsReadOnly + ": " + item.Name + "  (" + item.PropertyID + ") \n");
        //            }                    
        //        }
        //        else // d == null
        //        {
        //            textBox1.Text = "";
        //            label1.Text = "selection: none";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "selection: WIA Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }

        //}

        // get image from scanner (ability to order it to scan what it has at the time, but also possible to wait or cancel)
        // todo: move this to toolbar?
        //private void button2_Click(object sender, EventArgs e)
        //{
        //    //if (d == null) // ShowAcquireImage seems to work fine without pre-selection of scanner. That whole section seems only good for the user viewing scanner properties.
        //    //{
        //    //    textBox1.Text = "select a scanner first";
        //    //    return;
        //    //}
        //    DeviceManager manager = new DeviceManager();            
        //    ImageFile scannedImage = null;
        //    scannedImage = dialog.ShowAcquireImage(
        //        // advanced: try to change the dialog so non-grayscale options are not shown? Here's what dialog can do: https://msdn.microsoft.com/en-us/library/ms630492(v=vs.85).aspx
        //                WiaDeviceType.ScannerDeviceType,
        //                WiaImageIntent.GrayscaleIntent, // changed from UnspecifiedIntent. Don't know if it matters, the user can choose once the dialog opens...
        //                WiaImageBias.MaximizeQuality,
        //                FormatID.wiaFormatTIFF, // todo: needed? Not really using the format's full potential, since it's converted to bmp later...but other formats default to low bpi
        //                true, true, false);
        //    if (scannedImage != null)
        //    {
        //        //scannedImage.SaveFile("scannedImage.png"); // This is how to save it as a new file, if needed
        //        pictureBox1.ArrFromScannedImage(scannedImage); // fff...I completely forgot to write this line and been wondering what the hell was missing...
        //    }
        //}
        #endregion

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    base.OnClosing(e);

        //    CheckTime.Dump();
        //}
    }
}
