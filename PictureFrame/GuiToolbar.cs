using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
//using WIA;


namespace PictureFrame
{
    public interface GUIForm
    {
        GUIPictureBox PicBox();
        void ShowStats();
    }

    // right now this is a ToolStrip using a specific order of specific ToolStripButtons, and some of the button's properties are not set in its Init.
    public class GUIToolbar : ToolStrip
    {
        public ButtonNewImage tsbNewImg;
        public ButtonZoom1 tsbZoomTo1;
        public ButtonZoomFit tsbZoomToFit;
        public ButtonGridToggle tsbGrid;
        //public ToolStripTextBox tstbStatus;
        //public ButtonScannerInterface tsbScanner;
        //public ButtonResetCB tsbResetCB;
        //public ButtonAnalyze tsbAnalyze;

        public GUIToolbar()
            : base()
        {
            tsbNewImg = new ButtonNewImage();
            tsbZoomTo1 = new ButtonZoom1();
            tsbZoomToFit = new ButtonZoomFit();
            tsbGrid = new ButtonGridToggle();
            //tsbScanner = new ButtonScannerInterface();
            //tsbResetCB = new ButtonResetCB();
            //tsbAnalyze = new ButtonAnalyze();
            // NOTE: changing order in the following line is enough to reorder the toolbar
            Items.AddRange(new ToolStripItem[] { tsbNewImg, /*tsbScanner,*/ /*tsbResetCB,*/ tsbZoomToFit, tsbZoomTo1, tsbGrid, /*tsbAnalyze*/ }); // todo: order them more sensibly...
            this.Size = new Size(400, 64); // don't think it worked...
            this.ImageScalingSize = new Size(48, 48); // a bit too big? // note: original icon size 64x64 // note: this is the property that actually affects image size
            InitButtons();
            // note: button images used based on http://adamwhitcroft.com/batch/
        }

        private void InitButtons()
        {
            System.Drawing.Size buttonSize = new System.Drawing.Size(64, 64); // changing numbers doesn't seem to change button sizes, even when i adjust toolbar size...

            // open new image file
            tsbNewImg.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            //tsbNewImg.ImageTransparentColor = System.Drawing.Color.Magenta; // todo: should probably remove these...
            tsbNewImg.Name = "toolStripButton1"; // needed? Numbers no longer correspond to order, anyway.
            tsbNewImg.Size = buttonSize;
            tsbNewImg.Text = "Open Image File";
            tsbNewImg.Image = Properties.Resources.tsb_uni_img_from_file;
            tsbNewImg.AutoSize = false;

            // set zoom to 1
            tsbZoomTo1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            //tsbZoomTo1.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbZoomTo1.Name = "toolStripButton2";
            tsbZoomTo1.Size = buttonSize;
            tsbZoomTo1.Text = "Reset Zoom"; // option: zoom to 1? Zoom to scale?
            tsbZoomTo1.Image = Properties.Resources.tsb_uni_zoom_to_1;
            tsbZoomTo1.AutoSize = false;

            // zoom to fit
            tsbZoomToFit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            //tsbZoomToFit.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbZoomToFit.Name = "toolStripButton3";
            tsbZoomToFit.Size = buttonSize;
            tsbZoomToFit.Text = "Zoom to Fit";
            tsbZoomToFit.Image = Properties.Resources.tsb_uni_zoom_to_fit;
            tsbZoomToFit.AutoSize = false;

            // show/hide grid
            tsbGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            // option: replace display style with ImageAndText and the hover text will always be visible. But with narrow window button will be invisible sooner and clicking it in dropdown does nothing (fix?)
            //tsbGrid.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbGrid.Name = "toolStripButton4";
            tsbGrid.Size = buttonSize;
            tsbGrid.Text = "Show Grid";
            tsbGrid.Image = Properties.Resources.tsb_uni_grid_show; // this one is switched on click, so it's like a toggle
            tsbGrid.AutoSize = false;

            //// todo: don't know if any other properties need be set for the new buttons. They may be unnecessary for other buttons as well
            //// scan image
            //tsbScanner.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            //tsbScanner.Size = buttonSize;
            //tsbScanner.Text = "Scan Image";
            //tsbScanner.Image = Properties.Resources.tsb_uni_img_from_scan;
            //tsbScanner.AutoSize = false;

            //// reset contrast/brightness
            //tsbResetCB.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            //tsbResetCB.Size = buttonSize;
            //tsbResetCB.Text = "Reset Contrast/Brightness";
            //tsbResetCB.Image = Properties.Resources.tsb_uni_contrast_brightness_reset;
            //tsbResetCB.AutoSize = false;

            //// status text box
            //tstbStatus.Name = "toolStripTextBox1";
            //tstbStatus.ReadOnly = true;
            //tstbStatus.Size = new System.Drawing.Size(380, 32);

            //// analyze button. todo: image and such
            //tsbAnalyze.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            //tsbAnalyze.Size = buttonSize;
            //tsbAnalyze.Text = "Analyze Defects"; // option: just 'Analyze'? 'Find Defects'?
            //tsbAnalyze.Image = Properties.Resources.tsb_uni_analyze;
            //tsbAnalyze.AutoSize = false;
        }
    }

    // image browser button. Also resets zoom and image center.
    public class ButtonNewImage : ToolStripButton
    {
        protected override void OnClick(EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image files (*.bmp, *.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.bmp; *.jpg; *.jpeg; *.jpe; *.jfif; *.png"; // text shown as filter and actual filter
            openFileDialog1.Title = "Select an image to open"; // title of browsing window opened

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Form form = this.GetCurrentParent().FindForm();
                if (form is GUIForm)
                {
                    (form as GUIForm).PicBox().Image = new Bitmap(openFileDialog1.FileName); // todo: any way to do this more generally than bitmap? Any need to?
                    //(form as GUIForm).PicBox().ArrFromImage(new Bitmap(openFileDialog1.FileName)); // note: replaced with image version since I'm not using arr right now
                    (form as GUIForm).PicBox().Reset();
                }
                if (this.GetCurrentParent() is GUIToolbar)
                {
                    (this.GetCurrentParent() as GUIToolbar).tsbZoomToFit.ClickMe();
                }
            }
        }

        public void ClickMe()
        {
            OnClick(new EventArgs());
        }
    }

    // zoom reset button. Does not affect window size or image centering
    public class ButtonZoom1 : ToolStripButton
    {
        protected override void OnClick(EventArgs e)
        {
            Form form = this.GetCurrentParent().FindForm();
            if (form is GUIForm)
            {
                (form as GUIForm).PicBox().ResetZoom();
                (form as GUIForm).PicBox().TryDrag((form as GUIForm).PicBox().state.center);
                (form as GUIForm).ShowStats();
                form.Invalidate();
            }
        }
    }

    // zoom-to-fit button. Centers image and adjusts scale so the entire image is visible in the window and at least one dimension fills the PictureBox
    public class ButtonZoomFit : ToolStripButton
    {
        protected override void OnClick(EventArgs e)
        {
            Form form = this.GetCurrentParent().FindForm();
            if (form is GUIForm)
            {
                (form as GUIForm).PicBox().ZoomToFit();
                (form as GUIForm).ShowStats();
                this.Invalidate();
            }
        }

        public void ClickMe()
        {
            OnClick(new EventArgs());
        }
    }

    // button replacing checkbox - show / hide grid
    public class ButtonGridToggle : ToolStripButton
    {
        protected override void OnClick(EventArgs e)
        {
            Form form = this.GetCurrentParent().FindForm();
            if (form is GUIForm)
            {
                //bool check = (sender as ToolStripButton).Checked;
                bool check = !(form as GUIForm).PicBox().DoDrawGrid;
                (form as GUIForm).PicBox().DoDrawGrid = check;
                if (check)
                {
                    if ((form as GUIForm).PicBox().state.scale >= 1) // should be drawn only when scale >= 1, or make line density (relative to pixels) dependant on scale.
                        //Invalidate();
                        (form as GUIForm).PicBox().Invalidate();
                    this.Text = "Hide Grid";
                    this.Image = Properties.Resources.tsb_uni_grid_hide;
                }
                else
                {
                    (form as GUIForm).PicBox().Invalidate(); // redraw image without gridlines
                    this.Text = "Show Grid";
                    this.Image = Properties.Resources.tsb_uni_grid_show;
                }
            }
        }
    }

    //// opens scanner-choosing dialog, scans image, if allowed, and loads it into the picture box.
    //public class ButtonScannerInterface : ToolStripButton
    //{
    //    protected override void OnClick(EventArgs e)
    //    {
    //        Form form = this.GetCurrentParent().FindForm();
    //        if (form is GUIForm)
    //        {
    //            DeviceManager manager = new DeviceManager();
    //            WIA.CommonDialog dialog = new WIA.CommonDialog(); // create a new WIA common dialog box for the user to select a device from
    //            ImageFile scannedImage = null;
    //            scannedImage = dialog.ShowAcquireImage(
    //                        // advanced: try to change the dialog so non-grayscale options are not shown? Here's what dialog can do: https://msdn.microsoft.com/en-us/library/ms630492(v=vs.85).aspx
    //                        WiaDeviceType.ScannerDeviceType,
    //                        WiaImageIntent.GrayscaleIntent, // changed from UnspecifiedIntent. Don't know if it matters, the user can choose once the dialog opens...
    //                        WiaImageBias.MaximizeQuality,
    //                        FormatID.wiaFormatTIFF, // todo: needed? Not really using the format's full potential, since it's converted to bmp later...but other formats default to low bpi
    //                        true, true, false);
    //            if (scannedImage != null)
    //            {
    //                //scannedImage.SaveFile("scannedImage.png"); // This is how to save it as a new file, if needed
    //                (form as GUIForm).PicBox().ArrFromScannedImage(scannedImage); // fff...I completely forgot to write this line and been wondering what the hell was missing...
    //            }
    //        }
    //    }
    //}

    //// resets contrast and brightness to their default values for the current Image.
    //public class ButtonResetCB : ToolStripButton
    //{
    //    protected override void OnClick(EventArgs e)
    //    {
    //        Form form = this.GetCurrentParent().FindForm();
    //        if (form is GUIForm)
    //        {
    //            (form as GUIForm).PicBox().ResetCB();
    //        }
    //    }
    //}

    //public class ButtonAnalyze : ToolStripButton
    //{
    //    protected override void OnClick(EventArgs e)
    //    {
    //        Form form = this.GetCurrentParent().FindForm();
    //        if (form is GUIForm)
    //        {
    //            (form as GUIForm).PicBox().imgArr.Analyze(); // todo: if image hasn't been loaded yet, an error message should appear instead of the program crashing...
    //            //(form as GUIForm).PicBox().ImageFromArr(); // update image // note: no longer relevant, working with unrotated image so nothing to update. Unless there's some sort of error checking?
    //            (form as GUIForm).PicBox().imgArr.Analyze2(); // function split so we can see the rotated image before the rest of the calculations go through. Can be combined again easily enough.
    //            (form as GUIForm).PicBox().ChangedDefects = true; // on PicBox's next OnPaint, discovered defects will be picked from imgArr and drawn on top of the picture.
    //            (form as GUIForm).PicBox().Invalidate(); // ...right?
    //        }
    //    }
    //}
}
