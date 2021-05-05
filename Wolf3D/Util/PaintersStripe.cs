using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolf3D.Util
{
    public class PaintersStripe
    {
        public int textureMapValue { get; set; }
        public float sideDistX { get; set; }
        public float sideDistY { get; set; }
        public int mapX { get; set; }
        public int mapY { get; set; }
        public int side { get; set; }
        public bool isSolid { get; set; }
        public bool isDoor { get; set; }
        public bool isHalfWall { get; set; }
    }
}
