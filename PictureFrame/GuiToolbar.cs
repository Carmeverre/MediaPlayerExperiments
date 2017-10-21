using System;
using System.Windows.Forms;
using System.Drawing;


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

        public GUIToolbar()
            : base()
        {
            tsbNewImg = new ButtonNewImage();
            tsbZoomTo1 = new ButtonZoom1();
            tsbZoomToFit = new ButtonZoomFit();
            tsbGrid = new ButtonGridToggle();

            // NOTE: changing order in the following line is enough to reorder the toolbar
            Items.AddRange(new ToolStripItem[] { tsbNewImg, tsbZoomToFit, tsbZoomTo1, tsbGrid});
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
}
