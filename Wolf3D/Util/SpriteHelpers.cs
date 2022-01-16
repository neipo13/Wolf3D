using Microsoft.Xna.Framework;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wolf3D.Util
{
    public static class SpriteHelpers
    {
        public static Color GetColorAt(Sprite sprite, int pixelIndex)
        {
            Color[] colors1D = new Color[sprite.Texture2D.Width * sprite.Texture2D.Height];
            sprite.Texture2D.GetData<Color>(colors1D);
            var indexInTexture = pixelIndex + (sprite.SourceRect.X + (sprite.SourceRect.Width * sprite.SourceRect.Y));
            return colors1D[indexInTexture - 1];
        }

        public static Color[] GetTextureColors(Sprite sprite)
        {
            Color[] colors1D = new Color[sprite.Texture2D.Width * sprite.Texture2D.Height];
            sprite.Texture2D.GetData<Color>(colors1D);
            return colors1D;
        }

        public static Color[] GetColors(Sprite sprite)
        {
            Color[] colors1D = new Color[sprite.Texture2D.Width * sprite.Texture2D.Height];
            sprite.Texture2D.GetData<Color>(colors1D);
            Color[] colorStore = new Color[sprite.SourceRect.Width * sprite.SourceRect.Height];
            for (int x = 0; x < sprite.SourceRect.Width; x++)
            {
                for (int y = 0; y < sprite.SourceRect.Height; y++)
                {
                    colorStore[x + (y * sprite.SourceRect.Width)] = colors1D[sprite.SourceRect.X + x + ((y + sprite.SourceRect.Y) * sprite.Texture2D.Width)];
                }
            }
            return colorStore;
        }

        public static Color[] GetColors(Sprite sprite, Color[] textureColors)
        {
            Color[] colorStore = new Color[sprite.SourceRect.Width * sprite.SourceRect.Height];
            for (int x = 0; x < sprite.SourceRect.Width; x++)
            {
                for (int y = 0; y < sprite.SourceRect.Height; y++)
                {
                    colorStore[x + (y * sprite.SourceRect.Width)] = textureColors[sprite.SourceRect.X + x + ((y + sprite.SourceRect.Y) * sprite.Texture2D.Width)];
                }
            }
            return colorStore;
        }
    }
}
