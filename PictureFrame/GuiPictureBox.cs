using System;
using System.Drawing;
using System.Windows.Forms;

namespace PictureFrame
{
    public delegate PointF CoordTransform(PointF p);

    public class GUIPictureBox : PictureBox
    {
        public bool isImageLoaded { get; set; }

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


        public GUIPictureBox()
        {
            Image = null;
            isImageLoaded = false;
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
            PaintPic(e);
        }
        
        public void PaintPic(PaintEventArgs e)
        {
            base.OnPaint(e); // used to have stackoverflow exception...dunno if this is needed, but MSDN says derived classes should call it...
            e.Graphics.Clear(this.BackColor); // deafult form color. Parameter can be replaced by Color.White etc.
            DrawImgScaled(e);
            if (DoDrawGrid && state.scale >= 1)
                DrawGrid(e.Graphics);
        }
        
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
    }
}
