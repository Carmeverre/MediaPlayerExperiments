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
    /// Interaction logic for GuiPictureBox.xaml
    /// </summary>
    public partial class GuiPictureBox : UserControl
    {
        public Image Image;//TODO

        public bool isImageLoaded { get; set; }

        // image to be drawn and manipulated is found/set at picture box's Image property.

        // todo: some things here are public only for debugging buttons, and should be made private / properties with special getter/setters later

        public PresentationState state { get; set; } // contains center, mouseLastP, scale. getters / setters needed here? Maybe move mouseLastP out? Or move something else in?
        Point mouseLastP { get; set; } // last location of mouse as calculated by processed MouseMove event
        MouseButton mouseButtonPressed; // to differentiate between drag and contrast/brightness
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

        Pen pen = new Pen(Brushes.Black,1); // black lines of width 1pt. Might want to init this for the whole form, not every time the grid is drawn.

        public event EventHandler UpdateStats; // custom event handler, event raised when Stats are updated
        public event MouseEventHandler UpdateMouseStats; // custom event handler, event raised when Stats are updated
        EventArgs ea = EventArgs.Empty;


        public GuiPictureBox()
        {
            Image = null;
            isImageLoaded = false;
        }

        public void Init()
        {
            //Init(new Bitmap(ClientSize.Width, ClientSize.Height));//TODO
        }

        public void Init(Image img)
        {
            Image = img;
            state = new PresentationState(new PointD(Image.Width / 2, Image.Height / 2), 1);
            mouseLastP = new Point(); // just in case
            //DoubleBuffered = true;//TODO
            //SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);//TODO
            DoDrawGrid = false;
            //Image = img; // Picture box is automatically invalidated and redrawn when image changes
            //this.Invalidate(); // not sure if necessary
        }

        // resets zoom to 1 and moves image so its top-left corner is at top-left corner of the PictureBox
        public void Reset()
        {
            state.center = new PointD(Image.Width / 2, Image.Height / 2);
            //DoDrawGrid = false; // text of a ToolStripButton depends on this property, shouldn't be reset separately.
            ResetZoom();
        }

        public void ResetZoom()
        {
            state.scale = 1;
            scaleTimes = 0;
            //Invalidate();//TODO
        }

        public void ZoomToFit()
        {
            state.center = new PointD(ActualWidth / 2, ActualHeight / 2); // center of the PictureBox
            // fit width
            if ((float)ActualWidth / Image.Width < (float)ActualHeight / Image.Height)
            {
                state.scale = (float)ActualWidth / Image.Width;
            }
            else // fit height
            {
                state.scale = (float)ActualHeight / Image.Height;
            }
            scaleTimes = (int)Math.Round(Math.Log(state.scale, SCALE_FACTOR));
            // above formula based on the line: float factor = (float)Math.Pow(SCALE_FACTOR, scaleTimes - prevScaleTimes);
            //Invalidate();//TODO
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
            PointD newCenter = new PointD(state.center.X + e.GetPosition(this).X - mouseLastP.X, state.center.Y + e.GetPosition(this).Y - mouseLastP.Y);
            mouseLastP = e.GetPosition(this);//.Location;

            if (TryDrag(newCenter))
            {
                //Invalidate();//TODO
            }
        }

        // use separately to prevent clipping on one side while there's a border on the other
        public bool TryDrag(PointD newCenter)
        {
            newCenter = DragLimit(newCenter);
            if (!state.center.Equals(newCenter)) // prevent unnecessary redrawing on failed drag, ioncluding redrawing grid and any other details // note: changed to Equals for new type, it's a value type (struct) so should be value comparison
            {
                state.center = newCenter; // image actually moves
                //this.Invalidate();
                return true;
            }
            return false;
        }

        // If image is large and attempts to move so a blank area is shown, it is held back. Applicable to drag and resize.
        // long function, but in most cases all but one conditions will be skipped, so it shouldn't take long
        public PointD DragLimit(PointD newCenter)
        {
            double scale = state.scale;
            // prevent from moving left or right too far
            if (Image.Width * scale > ActualWidth) // wide image compared to window
            {
                // going too far to the left
                if (newCenter.X < ActualWidth - (Image.Width * scale / 2))
                {
                    newCenter.X = ActualWidth - (Image.Width * scale / 2);
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
                else if (newCenter.X > ActualWidth - (Image.Width * scale / 2))
                {
                    newCenter.X = ActualWidth - (Image.Width * scale / 2);
                }
            }

            // prevent from moving up or down too far
            // tall image compared to window
            if (Image.Height * scale > ActualHeight)
            {
                // going too far up
                if (newCenter.Y < ActualHeight - (Image.Height * scale / 2))
                {
                    newCenter.Y = ActualHeight - (Image.Height * scale / 2);
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
                else if (newCenter.Y > ActualHeight - (Image.Height * scale / 2))
                {
                    newCenter.Y = ActualHeight - (Image.Height * scale / 2);
                }
            }
            return newCenter;
        }

        // coordinates of mouse relative to the (top left corner of) image (taking into account scale)
        public PointD MouseLocationOnImage(PointD p)
        {
            return new PointD(((p.X - state.center.X) / state.scale) + Image.Width / 2, ((p.Y - state.center.Y) / state.scale) + Image.Height / 2);
        }

        // location of Image's top left corner on form
        private PointD ImageOffset()
        {
            double x = state.center.X - (Image.Width / 2) * state.scale;
            double y = state.center.Y - (Image.Height / 2) * state.scale;
            return new PointD(x, y);
        }

        // given coordinates relative to top left corner of Image, in pixels, at scale = 1, return location on form where defect should be drawn
        // (works for single point. If center of defect is given, additional parameters and math may be needed)
        // almost the inverse of MouseLocationOnImage, but works with top left corner instead of center.
        public PointD ImageToWindow(PointD p)
        {
            PointD offset = ImageOffset();
            double x = (p.X * state.scale) + offset.X;
            double y = (p.Y * state.scale) + offset.Y;
            return new PointD(x, y);
        }

        protected override void OnRender(DrawingContext drawingContext) // replaces OnPaint
        {
            base.OnRender(drawingContext);
            //PaintEventArgs e // (original handler input)
            if (Image == null/* || imgArr == null*/)
            { // todo: maybe have a flag in rawarr when it's not null to say whether an image is loaded or not...
                //e.Graphics.Clear(this.BackColor); // otherwise everything is black before an image is loaded //TODO
                return;
            }
            PaintPic(drawingContext);
        }

        public void PaintPic(DrawingContext drawingContext)
        {
            //PaintEventArgs e // (original handler input)
            base.OnRender(drawingContext);//base.OnPaint(e); // used to have stackoverflow exception...dunno if this is needed, but MSDN says derived classes should call it...
            //e.Graphics.Clear(this.BackColor); // deafult form color. Parameter can be replaced by Color.White etc.//TODO
            DrawImgScaled(drawingContext);
            if (DoDrawGrid && state.scale >= 1)
            {
                //DrawGrid(e.Graphics);//TODO
            }
        }

        private void DrawImgScaled(DrawingContext drawingContext)
        {
            //PaintEventArgs e // (original handler input)
            //e.Graphics.Clip = new Region(ClientRectangle); // This prevents image sliding under toolbar. So long as the ClientSize is kept updated.//TODO
            //System.Drawing.Drawing2D.InterpolationMode mode = e.Graphics.InterpolationMode; // bilinear
            if (state.scale >= 1) // when grid is visible
            {
                //e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;//TODO
            }
            else
            {
                //e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear; // this is default, but just in case. Nearest neighbor at small scale looks weird.//TODO
            }
            //e.Graphics.DrawImage(Image, GetScaledFrameCentered());//TODO
        }

        private PointD[] GetScaledFrameCentered()
        {
            PointD center = state.center;
            double scale = state.scale;
            PointD TL = new PointD(center.X - Image.Width * scale / 2, center.Y - Image.Height * scale / 2);
            PointD TR = new PointD(center.X + Image.Width * scale / 2, center.Y - Image.Height * scale / 2);
            PointD BL = new PointD(center.X - Image.Width * scale / 2, center.Y + Image.Height * scale / 2);
            PointD[] frame = { TL, TR, BL };
            return frame;
        }

        /* //TODO
        private void DrawGrid(Graphics g)
        {
            // pen now tied to class
            double dist = BASE_DIST * state.scale; // scales correctly
            // todo: for greatly magnified image, maybe increase line density (smaller dist). For scale under 1, maybe do draw grid with reduced line density (bigger dist).
            double leftX = state.center.X - Image.Width * state.scale / 2;
            double rightX = state.center.X + Image.Width * state.scale / 2;
            double topY = state.center.Y - Image.Height * state.scale / 2;
            double bottomY = Math.Min(state.center.Y + Image.Height * state.scale / 2, ClientRectangle.Bottom); // Min prevents gridlines being drawn over the toolbar

            double shift = 0.5f * state.scale;

            //todo-minor: maybe combine pairs of for loops, since borders are symmetric around center.
            // draw vertical lines left of center
            for (double x = state.center.X; x >= leftX; x -= dist)
            {
                g.DrawLine(pen, new PointD(x - shift, topY - shift), new PointD(x - shift, bottomY + shift));
            }

            // draw vertical lines right of center
            for (double x = state.center.X; x <= rightX; x += dist)
            {
                g.DrawLine(pen, new PointD(x - shift, topY - shift), new PointD(x - shift, bottomY + shift));
            }

            // draw horizontal lines above center
            for (double y = state.center.Y; y >= topY; y -= dist)
            {
                g.DrawLine(pen, new PointD(leftX - shift, y - shift), new PointD(rightX + shift, y - shift));
            }

            // draw horizontal lines below center
            for (double y = state.center.Y; y <= bottomY; y += dist)
            {
                g.DrawLine(pen, new PointD(leftX - shift, y - shift), new PointD(rightX + shift, y - shift));
            }
        }
        */

        #region Mouse Interactions
        // Down, Move and Up deal with mouse drag beginning inside the picture box
        // Wheel reacts to moving mouse wheel anywhere when window is selected 
        // Enter and Leave raise the right status update events, so form's status shows mouse coordinaes when and only when they are relevant, and enable reaction to wheel.

        private void GuiPictureBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (mouseInPic) // now wheel won't scale when mouse is outside picture box, but picture box may still have focus.
            {
                int prevScaleTimes = scaleTimes;
                //scaleTimes += e.Delta / SystemInformation.MouseWheelScrollDelta;//TODO
                // limit the scaling...beyond a certain point it is useless, and beyond a further point it can cause overflow exception
                if (scaleTimes > MAX_WHEELS_UP)
                    scaleTimes = MAX_WHEELS_UP;
                if (scaleTimes < MAX_WHEELS_DOWN)
                    scaleTimes = MAX_WHEELS_DOWN;
                float factor = (float)Math.Pow(SCALE_FACTOR, scaleTimes - prevScaleTimes);
                state.scale *= factor;
                double distX = state.center.X - e.GetPosition(this).X;
                double distY = state.center.Y - e.GetPosition(this).Y;
                state.center = new PointD(e.GetPosition(this).X + (distX * factor), e.GetPosition(this).Y + (distY * factor)); // this causes the zoom to be around the mouse's location, so relative mouse coordinates usually won't change on zoom
                TryDrag(state.center); // prevents inconsistent image borders after zoom and jumping on next move. This is why mouse coordinates sometimes change on zoom.
                SendStatsEvent(e);
                //this.Invalidate();//TODO
            }
        }
        
        private void GuiPictureBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // to prevent confusing scenarios of switching mouse buttons while one is pressed. The one you start with is the only one working.
            // also ignores any mouse buttons other that left and right
            if (!mouseIsDown && (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right))
            {
                // works on anything within the bounds of the PictureBox (not just the image itself), in case someone shrinks the image to much smaller than the form and wants to drag it around. Plus it's easier to check.
                mouseIsDown = true;
                mouseLastP = e.GetPosition(this); //.Location; // is that the right replacement? relative to "this"?
                mouseButtonPressed = e.ChangedButton;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mouseIsDown)
            {
                if (mouseButtonPressed == MouseButton.Left)
                    Drag(e);
                //else if (mouseButtonPressed == MouseButtons.Right)
                //    ChangeContrastBrightness(e);
                // others might be center mouse button or others, which shouldn't interact with the image, I guess..?
            }
            SendStatsEvent(e);
        }

        private void GuiPictureBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseIsDown)
            {
                if (e.ChangedButton == mouseButtonPressed) // accidentally clicking another mouse button and releasing it won't stop the first one working
                {
                    mouseIsDown = false;
                    //Invalidate();//TODO
                }
            }
        }

        private void GuiPictureBox_MouseEnter(object sender, MouseEventArgs e)
        {
            mouseInPic = true; // not precise. If borders are shown around the image, they're still part of the control
            this.Focus(); // crude but works (to make picture box react to scrolling). Whether this is right depends on GUI requirements.
        }

        private void GuiPictureBox_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseInPic = false;
            UpdateStats(this, ea);
            // FindForm().Focus(); // the goggles they do nothing. Guess the form can't get focus.
            // Tabbing does make picture box lose focus, though. Added condition in OnMouseWheel so wheel doesn't change zoom when mouse is outside picture box
        }


        #endregion
    }

    public class PresentationState
    {
        public PointD center { get; set; } // center of the image. Slightly simpler scaled image calculations than keeping top left corner.
        public double scale { get; set; } // zoom proportion

        public PresentationState(PointD center, float scale)
        {
            this.center = center;
            this.scale = scale;
        }
    }

    public struct PointD
    {
        public double X;
        public double Y;

        public PointD(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public PointD(Point p)
        {
            this.X = p.X;
            this.Y = p.Y;
        }
    }
}
