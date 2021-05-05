using Microsoft.Xna.Framework;
using Nez.Textures;

namespace Wolf3D.Util
{
    public struct VerticalSlice
    {
        public Color color;
        public float top;
        public float bottom;
        public Sprite Sprite;
        public int textureOffset;
        public float ZBuffer;

        public Rectangle sliceSourceRect
        {
            get
            {
                if (Sprite == null) return new Rectangle(0, 0, 1, 1);
                var orig = Sprite.SourceRect;
                orig.X += textureOffset;
                orig.Width = 1;
                if(orig.X > Sprite.SourceRect.Right)
                {
                    orig.Width = 0;
                }
                return orig;
            }
        }
    }
}
