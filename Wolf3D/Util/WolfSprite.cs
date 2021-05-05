using Microsoft.Xna.Framework;
using Nez;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Components;

namespace Wolf3D.Util
{
    public class WolfSprite : Component, IComparable<WolfSprite>
    {
        public Vector2 pos => this.Entity == null ? Vector2.Zero : (this.Entity.Position / 10f);

        /// <summary>
        /// distance without sqrt because exact distance is not needed for sorting
        /// </summary>
        public float distanceForSort
        {
            get
            {
                if (this.Entity == null) return float.MaxValue;
                return ((playerState.RayCasterPosition.X - pos.X) * (playerState.RayCasterPosition.X - pos.X) + (playerState.RayCasterPosition.Y - pos.Y) * (playerState.RayCasterPosition.Y - pos.Y));
            }
        }

        private int h = NezGame.h;

        protected Sprite Sprite;
        public PlayerState playerState;
        public Color[] SpriteColors;

        public bool flashing = false;
        public Color flashColor = Color.White;

        public bool flipX = false;

        public WolfSprite(PlayerState playerState) : base()
        {
            this.playerState = playerState;
        }

        public virtual void SetSprite(Sprite Sprite)
        {
            this.Sprite = Sprite;
            SpriteColors = SpriteHelpers.GetColors(Sprite);
        }

        public float SpriteWidth()
        {
            if (Sprite == null) return 1;
            return Sprite.SourceRect.Width;
        }

        public Rectangle rectStripe(int textureOffset)
        {
            if (Sprite == null) return new Rectangle(0, 0, 1, 1);
            var orig = Sprite.SourceRect;
            orig.X += textureOffset;
            orig.Width = 1;
            //orig.Y += 1;
            //orig.Height -= 1;
            return orig;
        }

        public int CompareTo(WolfSprite spr)
        {
            return spr.distanceForSort.CompareTo(this.distanceForSort);
        }

        public float spriteX
        {
            get
            {
                if (this.Entity == null) return 0f;
                return (this.Entity.Position.X / 10f) - playerState.RayCasterPosition.X;
            }
        }
        public float spriteY
        {
            get
            {
                if (this.Entity == null) return 0f;
                return (this.Entity.Position.Y / 10f) - playerState.RayCasterPosition.Y;
            }
        }

        public float transformX
        {
            get
            {
                if (this.Entity == null) return 0f;
                return playerState.invDet * (playerState.Direction.Y * spriteX - playerState.Direction.X * spriteY);
            }
        }

        public float transformY
        {
            get
            {
                if (this.Entity == null) return 0f;
                return playerState.invDet * (-playerState.Plane.Y * spriteX + playerState.Plane.X * spriteY);

            }
        }


        public int spriteScreenX
        {
            get
            {
                if (this.Entity == null) return 0;
                return (int)(NezGame.designWidth / 2 * (1 + transformX / transformY));
            }
        }
        public int spriteHeight
        {
            get
            {
                if (this.Entity == null) return 0;
                return Math.Abs((int)(NezGame.h / transformY));
            }
        }

        public int spriteWidth
        {
            get
            {
                if (this.Entity == null) return 0;
                var w = Math.Abs((int)(NezGame.h / transformY));
                if (w > NezGame.h)
                {
                    return 0;
                }
                return w;
            }
        }

        public int drawStartY
        {
            get
            {
                if (this.Entity == null) return 0;
                var y = -spriteHeight / 2 + NezGame.h / 2;
                if (y < 0) y = 0;
                return y;
            }
        }
        public int drawEndY
        {
            get
            {
                if (this.Entity == null) return 0;
                var y = spriteHeight / 2 + h / 2;
                if (y >= h) y = h - 1;
                return y;
            }
        }
        public int drawStartX
        {
            get
            {
                if (this.Entity == null) return 0;
                var x = -spriteWidth / 2 + spriteScreenX;
                if (x < 0) x = 0;
                return x;
            }
        }
        public int drawEndX
        {
            get
            {
                if (this.Entity == null) return 0;
                var x = spriteWidth / 2 + spriteScreenX;
                if (x >= NezGame.designWidth) x = NezGame.designWidth - 1;
                return x;
            }
        }
    }
}
