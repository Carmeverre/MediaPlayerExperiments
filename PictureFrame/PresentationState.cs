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
        public PointF center { get; set; } // center of the image. Slightly simpler scaled image calculations than keeping top left corner.
        public float scale { get; set; } // zoom proportion
        
        // todo: make any other relevant functions, like compare

        public PresentationState(PointF center, float scale)
        {
            this.center = center;
            this.scale = scale;
        }
    }
}