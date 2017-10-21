using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PictureFrame
{
    public class PresentationState
    {
        // default values for contrast and brightness. todo: adjust these according to specifications. Contrast should be greater than brightness
        const int DEF_BRIGHTNESS = 0;
        const int DEF_CONTRAST = 1024;

        // todo: move any other relevant fields here
        public PointF center { get; set; } // center of the image. Slightly simpler scaled image calculations than keeping top left corner.
        public float scale { get; set; } // zoom proportion
        // fields defining contrast and brightness. For now, correspond directly to leftlim and rightlim of the inclined z-graph's slope
        public int leftLim { get; set; } // defines starting point of slope on transformation graph
        public int rightLim { get; set; } // defines end point of slope on transformation graph. Must be greater than brightness


        // todo: make any other relevant functions, like compare

        public PresentationState(PointF center, float scale)
        {
            this.center = center;
            // this.mouseLastP = mouseLastP; // not relevant here
            this.scale = scale;
            this.leftLim = DEF_BRIGHTNESS;
            this.rightLim = DEF_CONTRAST;
        }
    }
}