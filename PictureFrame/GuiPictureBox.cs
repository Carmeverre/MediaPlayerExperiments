using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
//using WIA;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PictureFrame
{
    public delegate PointF CoordTransform(PointF p);

    public class GUIPictureBox : PictureBox
    {
        // image to be drawn and manipulated is found/set at picture box's Image property.

        // todo: some things here are public only for debugging buttons, and should be made private / properties with special getter/setters later

        public PresentationState state { get; set; } // contains center, mouseLastP, scale. getters / setters needed here? Maybe move mouseLastP out? Or move something else in?
        Point mouseLastP { get; set; } // last location of mouse as calculated by processed MouseMove event
        MouseButtons mouseButtonPressed; // to differentiate between drag and contrast/brightness
        int scaleTimes = 0; // number of times mouse wheel moved away from scale = 1. Scale down counts as negative. To make limiting with MAX_WHEELS_UP and MAX_WHEELS_DOWN easier.
        // must be updated when state.scale is updated, otherwise an overflow exception is possible.

        public bool DoDrawGrid { get; set; }

        const int BASE_DIST = 5; // distance in image pixels between grid lines. The lines are 1 pixel thick. Other values tried: 1 - solid black at scale = 1. 2 - a curious mosaic forms. with 5 looks like a proper grid, like the image is cut up into squares.
        const float SCALE_FACTOR = 1.2F; // how much scale is multiplied / divided by every time mouse is scrolled. 1.2 looks good to me.
        const int MAX_WHEELS_UP = 30;
        const int MAX_WHEELS_DOWN = -25;
        // last 2 are maximum number of times image can be scaled up or down relative to scale 1. Should probably depend on SCALE_FACTOR.
        // These are just my estimates of the limits of an image viewer's usefulness, overflow exception is caused a ways beyond that.

        bool mouseIsDown = false; // only if mouse went down within the PictureBox
        bool mouseInPic = false;

        Pen pen = new Pen(Color.Black); // black lines of width 1pt. Might want to init this for the whole form, not every time the grid is drawn.

        public event EventHandler UpdateStats; // custom event handler, event raised when Stats are updated
        public event MouseEventHandler UpdateMouseStats; // custom event handler, event raised when Stats are updated
        EventArgs ea = EventArgs.Empty;

        //// todo: different source for defects, check compatibility. Class of defect (currently DefectRing) may also be changed so they're drawn differently
        ////IEnumerable<IDefect> _defects = new List<DefectCircle> { new DefectCircle(100, 100, 58), new DefectCircle(300, 200, 58) }; // test list of defects
        //// empty lists of defects for compiling. Add/initialize defects here as in the example above and they will be drawn if DoDrawDefects == true
        //IEnumerable<IDefect> _defects1 = new List<DefectCircle>(); // type 1 defects
        //IEnumerable<IDefect> _defects2 = new List<DefectCircle>(); // type 2 defects
        //IEnumerable<IDefect> _defects3 = new List<DefectCircle>(); // type 3 defects
        //bool DoDrawDefects = true; // change by some means later, like a toggle button / checkbox as with grid.
        //public bool ChangedDefects = false;

        //ConstRawImageArr crawarr = new ConstRawImageArr(); // rawr! testing with constant arr
        // these are no longer constants, they depend on minimum and maximum values available in an image...
        //int MIN_LEFTLIM = -1000;
        //int MAX_RIGHTLIM = 30000;
        //int MIN_LIM_DIST = 100; // minimum distance between left and right limits of slope
        //bool ChangedContrastBrightness = false;
        //RawImageArr rawarr; // replace with the following:
        //public CImageData imgArr; // any way to avoid making this public..?


        public GUIPictureBox()
        {
            Image = null;
        }

        public void Init()
        {
            Init(new Bitmap(ClientSize.Width, ClientSize.Height));
        }

        public void Init(Image img)
        {
            Image = img;
            state = new PresentationState(new PointF(Image.Width / 2, Image.Height / 2), 1);
            mouseLastP = new Point(); // just in case
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
            DoDrawGrid = false;
            //Image = img; // Picture box is automatically invalidated and redrawn when image changes
            //this.Invalidate(); // not sure if necessary
        }

        // resets zoom to 1 and moves image so its top-left corner is at top-left corner of the PictureBox
        public void Reset()
        {
            state.center = new PointF(Image.Width / 2, Image.Height / 2);
            //DoDrawGrid = false; // text of a ToolStripButton depends on this property, shouldn't be reset separately.
            ResetZoom();
            //LimitContrastBrightness();
        }

        public void ResetZoom()
        {
            state.scale = 1;
            scaleTimes = 0;
            Invalidate();
        }

        public void ZoomToFit()
        {
            state.center = new PointF(ClientSize.Width / 2, ClientSize.Height / 2); // center of the PictureBox
            // fit width
            if ((float)ClientSize.Width / Image.Width < (float)ClientSize.Height / Image.Height)
            {
                state.scale = (float)ClientSize.Width / Image.Width;
            }
            else // fit height
            {
                state.scale = (float)ClientSize.Height / Image.Height;
            }
            scaleTimes = (int)Math.Round(Math.Log(state.scale, SCALE_FACTOR));
            // above formula based on the line: float factor = (float)Math.Pow(SCALE_FACTOR, scaleTimes - prevScaleTimes);
            Invalidate();
        }

        private void SendStatsEvent(MouseEventArgs e)
        {
            if (mouseInPic)
            {
                UpdateMouseStats(this, e);
            }
            else
            {
                UpdateStats(this, ea);
            }
        }

        public void Drag(MouseEventArgs e)
        {
            PointF newCenter = new PointF(state.center.X + e.X - mouseLastP.X, state.center.Y + e.Y - mouseLastP.Y);
            mouseLastP = e.Location;

            if (TryDrag(newCenter))
            {
                Invalidate();
            }
        }

        // use separately to prevent clipping on one side while there's a border on the other
        public bool TryDrag(PointF newCenter)
        {
            newCenter = DragLimit(newCenter);
            if (state.center != newCenter) // prevent unnecessary redrawing on failed drag, ioncluding redrawing grid and any other details
            {
                state.center = newCenter; // image actually moves
                //this.Invalidate();
                return true;
            }
            return false;
        }

        // If image is large and attempts to move so a blank area is shown, it is held back. Applicable to drag and resize.
        // long function, but in most cases all but one conditions will be skipped, so it shouldn't take long
        public PointF DragLimit(PointF newCenter)
        {
            float scale = state.scale;
            // prevent from moving left or right too far
            if (Image.Width * scale > ClientSize.Width) // wide image compared to window
            {
                // going too far to the left
                if (newCenter.X < ClientSize.Width - (Image.Width * scale / 2))
                {
                    newCenter.X = ClientSize.Width - (Image.Width * scale / 2);
                }

                // going too far to the right
                else if (newCenter.X > Image.Width * scale / 2)
                {
                    newCenter.X = Image.Width * scale / 2;
                }
            }
            else // narrow image
            {
                // too far left
                if (newCenter.X < Image.Width * scale / 2)
                {
                    newCenter.X = Image.Width * scale / 2;
                }
                // too far right
                else if (newCenter.X > ClientSize.Width - (Image.Width * scale / 2))
                {
                    newCenter.X = ClientSize.Width - (Image.Width * scale / 2);
                }
            }

            // prevent from moving up or down too far
            // tall image compared to window
            if (Image.Height * scale > ClientSize.Height)
            {
                // going too far up
                if (newCenter.Y < ClientSize.Height - (Image.Height * scale / 2))
                {
                    newCenter.Y = ClientSize.Height - (Image.Height * scale / 2);
                }

                // going too far down
                else if (newCenter.Y > Image.Height * scale / 2)
                {
                    newCenter.Y = Image.Height * scale / 2;
                }
            }
            else // vertically challenged image :P
            {
                // too far up
                if (newCenter.Y < Image.Height * scale / 2)
                {
                    newCenter.Y = Image.Height * scale / 2;
                }
                // too far down
                else if (newCenter.Y > ClientSize.Height - (Image.Height * scale / 2))
                {
                    newCenter.Y = ClientSize.Height - (Image.Height * scale / 2);
                }
            }
            return newCenter;
        }

        //// changes brightness and contrast proportionally to mouse movement and PictureBox size
        //public void ChangeContrastBrightness(MouseEventArgs e)
        //{
        //    // note: can add number to proportion calculation to change sensitivity by a constant. Increase numerator to increase sensitivity, increase denominator to reduce sensitivity.
        //    // option: change proportion to use constant number of pixels or screen points rather than ClientSize?
        //    // todo: try and change something about the math so it's not so easy to get seemingly stuck with a black screen..?

        //    Point dist = new Point(e.X - mouseLastP.X, e.Y - mouseLastP.Y);
        //    if (dist.X == 0 && dist.Y == 0) // small optimization in case contrast and brightness need not be changed...
        //        return;
        //    mouseLastP = e.Location; // alternative - only recalculate on mouse up. Not as smooth, but no delay. No longer sifnificant, I moved the update to the right place and it's quick enough now.

        //    // set new brightness and limit values:
        //    state.leftLim -= (dist.X * (state.rightLim - state.leftLim)) / (ClientSize.Width); // movement across possible brightness line proportional to mouse's movement along PictureBox's width
        //    if (state.leftLim < MIN_LEFTLIM)
        //        state.leftLim = MIN_LEFTLIM;
        //    else if (state.leftLim > MAX_RIGHTLIM - MIN_LIM_DIST)
        //        state.leftLim = MAX_RIGHTLIM - MIN_LIM_DIST;

        //    // set new contrast and limit values:
        //    state.rightLim += (dist.Y * (state.rightLim - state.leftLim)) / (ClientSize.Height); // movement across possible contrast line proportional to mouse's movement along PictureBox's height
        //    // possible contrast here is between rightmost limit and (updated) brightness
        //    if (state.rightLim < state.leftLim + MIN_LIM_DIST)
        //        state.leftLim = state.leftLim + MIN_LIM_DIST;
        //    else if (state.rightLim > MAX_RIGHTLIM)
        //        state.rightLim = MAX_RIGHTLIM;

        //    ChangedContrastBrightness = true;
        //    Invalidate(); // actual recalculate happens in OnPaint, brightness/contrast are updated much more responsively
        //}

        // coordinates of mouse relative to the (top left corner of) image (taking into account scale)
        public PointF MouseLocationOnImage(PointF p)
        {
            return new PointF(((p.X - state.center.X) / state.scale) + Image.Width / 2, ((p.Y - state.center.Y) / state.scale) + Image.Height / 2);
        }

        // location of Image's top left corner on form
        private PointF ImageOffset()
        {
            float x = state.center.X - (Image.Width / 2) * state.scale;
            float y = state.center.Y - (Image.Height / 2) * state.scale;
            return new PointF(x, y);
        }

        // given coordinates relative to top left corner of Image, in pixels, at scale = 1, return location on form where defect should be drawn
        // (works for single point. If center of defect is given, additional parameters and math may be needed)
        // almost the inverse of MouseLocationOnImage, but works with top left corner instead of center.
        public PointF ImageToWindow(PointF p)
        {
            PointF offset = ImageOffset();
            float x = (p.X * state.scale) + offset.X;
            float y = (p.Y * state.scale) + offset.Y;
            return new PointF(x, y);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Image == null/* || imgArr == null*/)
            { // todo: maybe have a flag in rawarr when it's not null to say whether an image is loaded or not...
                e.Graphics.Clear(this.BackColor); // otherwise everything is black before an image is loaded
                return;
            }
            //if (ChangedContrastBrightness)
            //{
            //    Image = CRawImageArr.DisplayableImage(imgArr, state.leftLim, state.rightLim);
            //    ChangedContrastBrightness = false;
            //}
            PaintPic(e);
            //if (DoDrawDefects)
            //{
            //    if (ChangedDefects)
            //    {
            //        ListDefects();
            //        ChangedDefects = false;
            //    }
            //    PaintDefects(e);
            //}
        }

        //protected void ListDefects()
        //{
        //    // TODO: empty list before adding all the vector's members...unless analyze is only ever meant to be called once?
        //    int x, y;
        //    for (int i = 0; i < imgArr.DefectNum(1); i++)
        //    {
        //        x = imgArr.DefectCoord(1, i, true);
        //        y = imgArr.DefectCoord(1, i, false);
        //        DefectCircle defect = new DefectCircle(x, y, 29 - 7); // todo: get diameter/wall thickness from data
        //        (_defects1 as List<DefectCircle>).Add(defect);
        //    }
        //    for (int i = 0; i < imgArr.DefectNum(2); i++)
        //    {
        //        x = imgArr.DefectCoord(2, i, true);
        //        y = imgArr.DefectCoord(2, i, false);
        //        DefectCircle defect = new DefectCircle(x, y, 29 - 7);
        //        (_defects2 as List<DefectCircle>).Add(defect);
        //    }
        //    for (int i = 0; i < imgArr.DefectNum(3); i++)
        //    {
        //        x = imgArr.DefectCoord(3, i, true);
        //        y = imgArr.DefectCoord(3, i, false);
        //        DefectCircle defect = new DefectCircle(x, y, 9); // note: different size. If it's a circle rather than an ellipse, let's have it centered at wall's center and no wider than the wall...
        //        (_defects3 as List<DefectCircle>).Add(defect);
        //    }
        //}

        public void PaintPic(PaintEventArgs e)
        {
            base.OnPaint(e); // used to have stackoverflow exception...dunno if this is needed, but MSDN says derived classes should call it...
            e.Graphics.Clear(this.BackColor); // deafult form color. Parameter can be replaced by Color.White etc.
            DrawImgScaled(e);
            if (DoDrawGrid && state.scale >= 1)
                DrawGrid(e.Graphics);
        }

        //public void PaintDefects(PaintEventArgs e)
        //{
        //    CoordTransform coordTransform = p => ImageToWindow(p);
        //    Pen pen = new Pen(Color.Red); // red for spot defect
        //    foreach (IDefect defect in _defects1)
        //        defect.Draw(e.Graphics, pen, coordTransform, state.scale); // also pass function for transforming coordinates
        //    pen.Color = Color.Yellow; // yellow for brightness!
        //    foreach (IDefect defect in _defects2)
        //        defect.Draw(e.Graphics, pen, coordTransform, state.scale);
        //    pen.Color = Color.Turquoise; // blue for highly shifted wall
        //    foreach (IDefect defect in _defects3)
        //        defect.Draw(e.Graphics, pen, coordTransform, state.scale);
        //}

        private void DrawImgScaled(PaintEventArgs e)
        {
            e.Graphics.Clip = new Region(ClientRectangle); // This prevents image sliding under toolbar. So long as the ClientSize is kept updated.
            //System.Drawing.Drawing2D.InterpolationMode mode = e.Graphics.InterpolationMode; // bilinear
            if (state.scale >= 1) // when grid is visible
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            else
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear; // this is default, but just in case. Nearest neighbor at small scale looks weird.
            e.Graphics.DrawImage(Image, GetScaledFrameCentered());
        }

        private PointF[] GetScaledFrameCentered()
        {
            PointF center = state.center;
            float scale = state.scale;
            PointF TL = new PointF(center.X - Image.Width * scale / 2, center.Y - Image.Height * scale / 2);
            PointF TR = new PointF(center.X + Image.Width * scale / 2, center.Y - Image.Height * scale / 2);
            PointF BL = new PointF(center.X - Image.Width * scale / 2, center.Y + Image.Height * scale / 2);
            PointF[] frame = { TL, TR, BL };
            return frame;
        }

        private void DrawGrid(Graphics g)
        {
            // pen now tied to class
            float dist = BASE_DIST * state.scale; // scales correctly
            // todo: for greatly magnified image, maybe increase line density (smaller dist). For scale under 1, maybe do draw grid with reduced line density (bigger dist).
            float leftX = state.center.X - Image.Width * state.scale / 2;
            float rightX = state.center.X + Image.Width * state.scale / 2;
            float topY = state.center.Y - Image.Height * state.scale / 2;
            float bottomY = Math.Min(state.center.Y + Image.Height * state.scale / 2, ClientRectangle.Bottom); // Min prevents gridlines being drawn over the toolbar

            float shift = 0.5f * state.scale;

            //todo-minor: maybe combine pairs of for loops, since borders are symmetric around center.
            // draw vertical lines left of center
            for (float x = state.center.X; x >= leftX; x -= dist)
            {
                g.DrawLine(pen, new PointF(x - shift, topY - shift), new PointF(x - shift, bottomY + shift));
            }

            // draw vertical lines right of center
            for (float x = state.center.X; x <= rightX; x += dist)
            {
                g.DrawLine(pen, new PointF(x - shift, topY - shift), new PointF(x - shift, bottomY + shift));
            }

            // draw horizontal lines above center
            for (float y = state.center.Y; y >= topY; y -= dist)
            {
                g.DrawLine(pen, new PointF(leftX - shift, y - shift), new PointF(rightX + shift, y - shift));
            }

            // draw horizontal lines below center
            for (float y = state.center.Y; y <= bottomY; y += dist)
            {
                g.DrawLine(pen, new PointF(leftX - shift, y - shift), new PointF(rightX + shift, y - shift));
            }
        }

        #region Mouse Interactions
        // Down, Move and Up deal with mouse drag beginning inside the picture box
        // Wheel reacts to moving mouse wheel anywhere when window is selected 
        // Enter and Leave raise the right status update events, so form's status shows mouse coordinaes when and only when they are relevant, and enable reaction to wheel.

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (mouseInPic) // now wheel won't scale when mouse is outside picture box, but picture box may still have focus.
            {
                int prevScaleTimes = scaleTimes;
                scaleTimes += e.Delta / SystemInformation.MouseWheelScrollDelta;
                // limit the scaling...beyond a certain point it is useless, and beyond a further point it can cause overflow exception
                if (scaleTimes > MAX_WHEELS_UP)
                    scaleTimes = MAX_WHEELS_UP;
                if (scaleTimes < MAX_WHEELS_DOWN)
                    scaleTimes = MAX_WHEELS_DOWN;
                float factor = (float)Math.Pow(SCALE_FACTOR, scaleTimes - prevScaleTimes);
                state.scale *= factor;
                float distX = state.center.X - e.X;
                float distY = state.center.Y - e.Y;
                state.center = new PointF(e.X + (distX * factor), e.Y + (distY * factor)); // this causes the zoom to be around the mouse's location, so relative mouse coordinates usually won't change on zoom
                TryDrag(state.center); // prevents inconsistent image borders after zoom and jumping on next move. This is why mouse coordinates sometimes change on zoom.
                SendStatsEvent(e);
                this.Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // to prevent confusing scenarios of switching mouse buttons while one is pressed. The one you start with is the only one working.
            // also ignores any mouse buttons other that left and right
            if (!mouseIsDown && (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right))
            {
                // works on anything within the bounds of the PictureBox (not just the image itself), in case someone shrinks the image to much smaller than the form and wants to drag it around. Plus it's easier to check.
                mouseIsDown = true;
                mouseLastP = e.Location;
                mouseButtonPressed = e.Button;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mouseIsDown)
            {
                if (mouseButtonPressed == MouseButtons.Left)
                    Drag(e);
                //else if (mouseButtonPressed == MouseButtons.Right)
                //    ChangeContrastBrightness(e);
                // others might be center mouse button or others, which shouldn't interact with the image, I guess..?
            }
            SendStatsEvent(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (mouseIsDown)
            {
                if (e.Button == mouseButtonPressed) // accidentally clicking another mouse button and releasing it won't stop the first one working
                {
                    mouseIsDown = false;
                    Invalidate();
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            mouseInPic = true; // not precise. If borders are shown around the image, they're still part of the control
            this.Focus(); // crude but works (to make picture box react to scrolling). Whether this is right depends on GUI requirements.
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            mouseInPic = false;
            UpdateStats(this, ea);
            // FindForm().Focus(); // the goggles they do nothing. Guess the form can't get focus.
            // Tabbing does make picture box lose focus, though. Added condition in OnMouseWheel so wheel doesn't change zoom when mouse is outside picture box
        }
        #endregion        


        #region image acquisition
        // stuff to do with loading images from a file (given as Image) or a scanned image (ImageFile)

        //// loads info from file into 2D arr that can be used for contrast/brightness change recalculations etc.
        //// should be called by new file opening button's click
        //// current implementation is slow (takes a couple of seconds), but fine if people don't switch images too often
        //public void ArrFromImage(Image img)
        //{
        //    // initialize array dimensions
        //    //imgArr = new CImageData(img.Width, img.Height); // replaces next line
        //    //rawarr = new RawImageArr(img.Width, img.Height); // todo: add special case for empty array (if someone tries to open an empty image). Could be an exception otherwise.

        //    // fill the array using image from file...
        //    if (img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format16bppGrayScale)
        //    {
        //        Bitmap clone = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale); // not many functions besides bitmap ones...
        //        using (Graphics g = Graphics.FromImage(clone))
        //        {
        //            g.DrawImage(img, new Rectangle(0, 0, clone.Width, clone.Height)); // drawing seems to copy the image...
        //        }
        //        ArrFromGrayscaleImage(clone);
        //    }
        //    else // any color or nonstandard pixel format
        //    {
        //        Bitmap clone = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format48bppRgb); // not many functions besides bitmap ones...
        //        using (Graphics g = Graphics.FromImage(clone))
        //        {
        //            g.DrawImage(img, new Rectangle(0, 0, clone.Width, clone.Height)); // drawing seems to copy the image...
        //        }
        //        // seems to work okay... todo: check whether this works for opening an image from file that has alpha.
        //        ArrFromColorImage(clone);
        //    }
        //    //ChangedContrastBrightness = true; // this ensures that image is loaded from array on paint, so colored images are shown as grayscale from the start
        //    Init(img); // hmm? well...it shows the image once loaded, but doesn't turn colord into grayscale
        //}

        //// should fill arr from pre-selected or at that time selected scanner        
        //public void ArrFromScannedImage(ImageFile imgF)
        //{
        //    var imageBytes = (byte[])imgF.FileData.get_BinaryData();
        //    using (var ms = new MemoryStream(imageBytes))
        //    {
        //        var img = Image.FromStream(ms);
        //        // init and fill the array using scannedImage ImageFile...
        //        ArrFromImage(img);
        //    }
        //}

        //// from PixelFormat enum, the only known grayscale format is 16bpp grayscale. Assumes arr dimensions are correctly initialized.
        //public void ArrFromGrayscaleImage(Bitmap bmp)
        //{
        //    //for (int y = 0; y < bmp.Height; y++)
        //    //{
        //    //    for (int x = 0; x < bmp.Width; x++)
        //    //    {
        //    //        Color c = bmp.GetPixel(x,y);
        //    //        rawarr.arr[x, y] = c.R; // red component. Assuming all components equal. This is wrong. Color components here are bytes. There must be more depth available...
        //    //    }
        //    //}

        //    BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppGrayScale); // read-only this time...and changed the pixel format to what should be provided...
        //    int stride = data.Stride; // presumably directly proportional to bpp and number of color components
        //    int colorSum; // or average...

        //    unsafe
        //    {
        //        Int16* ptr = (Int16*)data.Scan0; // the problem is likely with byte. Int16 perhaps?
        //        //// Go through the draw area and set the pixels as they should be
        //        //for (int y = 0, ny = bmp.Height, nx = bmp.Width; y < ny; y++)
        //        //{
        //        //    for (int x = 0; x < nx; x++)
        //        //    {
        //        //        // todo: did this work? I don't know, it never actually got turned into this format...
        //        //        colorSum = ptr[(x * 3) + y * stride / 2];
        //        //        rawarr.arr[y, x] = colorSum;
        //        //    }
        //        //}

        //        // C++ CImageData implementation (untested): [note ptr from previous implementation is still needed]
        //        short* c_ptr;
        //        for (int y = 0, ny = bmp.Height, nx = bmp.Width; y < ny; y++)
        //        {
        //            c_ptr = imgArr.Line(y);
        //            for (int x = 0; x < nx; x++)
        //            {
        //                colorSum = ptr[(x * 3) + y * stride / 2];
        //                c_ptr[x] = (short)colorSum; // thereabouts...?
        //            }
        //        }
        //    }
        //    bmp.UnlockBits(data);
        //}

        //// assume pre-converted to a particular format..? Format currently used: Format48bppRgb
        //// todo: probably merge this with the grayscale. Once they're in the same format, the averaging will work for greyscale as well as color and save a bunch of duplication.
        //// Only loss is in converting grayscale to color. Even the stride length should be the same when locking bits!
        //public void ArrFromColorImage(Bitmap bmp)
        //{
        //    //// changed this to use lock bits after testing
        //    //for (int y = 0; y < bmp.Height; y++)
        //    //{
        //    //    for (int x = 0; x < bmp.Width; x++)
        //    //    {
        //    //        Color c = bmp.GetPixel(x, y);
        //    //        rawarr.arr[y, x] = (c.R + c.G + c.B); // needs /3? nah, let's test with different ranges. This is wrong. Color components here are bytes. There must be more depth available...
        //    //    }
        //    //}

        //    BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format48bppRgb); // read-only this time...and changed the pixel format to what should be provided...
        //    int stride = data.Stride; // width of a single line of pixels (oh...must be in bytes! so...divide by 2 for int16 pointer? Yes! Without that, got access violation)
        //    int colorSum; // or average...

        //    unsafe
        //    {
        //        short* ptr = (short*)data.Scan0; // with byte, the image just looks wrong (of course...doesn't fit PixelFormat), but with Int16/short I get AccessViolation
        //        //// Go through the draw area and set the pixels as they should be
        //        //// yuss! Extremely quick loading now!
        //        //for (int y = 0, ny = bmp.Height, nx = bmp.Width; y < ny; y++)
        //        //{
        //        //    for (int x = 0; x < nx; x++)
        //        //    {
        //        //        colorSum = ptr[(x * 3) + y * stride / 2]; // red component
        //        //        colorSum += ptr[(x * 3) + y * stride / 2 + 1]; // green component
        //        //        colorSum += ptr[(x * 3) + y * stride / 2 + 2]; // blue component
        //        //        rawarr.arr[y, x] = colorSum;
        //        //    }
        //        //}

        //        // C++ CImageData implementation (untested): [note ptr from previous implementation is still needed]
        //        short* c_ptr;
        //        for (int y = 0, ny = bmp.Height, nx = bmp.Width; y < ny; y++)
        //        {
        //            c_ptr = imgArr.Line(y);
        //            for (int x = 0; x < nx; x++)
        //            {
        //                colorSum = ptr[(x * 3) + y * stride / 2]; // red component
        //                colorSum += ptr[(x * 3) + y * stride / 2 + 1]; // green component
        //                colorSum += ptr[(x * 3) + y * stride / 2 + 2]; // blue component
        //                c_ptr[x] = (short)colorSum; // thereabouts...?
        //            }
        //        }
        //    }
        //    bmp.UnlockBits(data);
        //}

        //// TODO
        //public void ImageFromArr()
        //{
        //    //Bitmap img = new Bitmap(Image.Width, Image.Height, PixelFormat.Format16bppGrayScale);
        //    //BitmapData data = img.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppGrayScale);
        //    //int stride = data.Stride; // width of a single line of pixels (in bytes, so divide by 2 to get short pointer)

        //    //unsafe
        //    //{
        //    //    short* ptr = (short*)data.Scan0; // with byte, the image just looks wrong (of course...doesn't fit PixelFormat), but with Int16/short I get AccessViolation
        //    //    short* c_ptr;
        //    //    for (int y = 0, ny = Image.Height, nx = Image.Width; y < ny; y++)
        //    //    {
        //    //        c_ptr = imgArr.LineRotated(y);
        //    //        for (int x = 0; x < nx; x++)
        //    //        {
        //    //            ptr[x + y * stride / 2] = c_ptr[x]; // pixel format is grayscale, this should do..?
        //    //        }
        //    //    }
        //    //}
        //    //img.UnlockBits(data);
        //    //Image = img;
        //    //ChangedContrastBrightness = true;
        //    Invalidate(); // (?) // ALTERNATIVE: comment out everything above that's uncommented, set ChangedContrastBrightness = true, and make it so rotated image is put in m_pImage and not separate thing.
        //    // as it is, drawing sends argument exception at OnPaint or something...
        //}

        #endregion

        //// finds minimum and maximum values in loaded rawarr and defines MIN_LEFTLIM, MAX_RIGHTLIM, and MIN_LIM_DIST accordingly, and initializes brightness and contrast to the limits.
        //public void LimitContrastBrightness()
        //{
        //    int min = int.MaxValue;
        //    int max = int.MinValue;
        //    int temp;
        //    unsafe
        //    {
        //        short* c_ptr;
        //        for (int y = 0; y < Image.Height; y++)
        //        {
        //            c_ptr = imgArr.Line(y);
        //            for (int x = 0; x < Image.Width; x++)
        //            {
        //                temp = c_ptr[x];
        //                if (min > temp)
        //                    min = temp;
        //                if (max < temp)
        //                    max = temp;
        //            }
        //        }
        //    }
        //    MIN_LEFTLIM = min;
        //    MAX_RIGHTLIM = max;
        //    MIN_LIM_DIST = (max - min) / 100; // dunno if that's a good minimum distance
        //    state.leftLim = MIN_LEFTLIM;
        //    state.rightLim = MAX_RIGHTLIM; // assuming the min/max were not outliers, this should do
        //}

        //public void ResetCB()
        //{
        //    state.leftLim = MIN_LEFTLIM;
        //    state.rightLim = MAX_RIGHTLIM; // assuming the min/max were not outliers, this should do
        //    ChangedContrastBrightness = true;
        //    Invalidate();
        //}
    }
}
