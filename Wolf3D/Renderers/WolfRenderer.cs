using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Tweens;
using System;
using System.Collections.Generic;
using Wolf3D.Util;

namespace Wolf3D.Renderers
{

    public class WolfRenderer : Renderer
    {
        public Color[,] colorBuffer;
        public float[,] zBuffer;
        public WolfSprite[] sprites;
        SpriteBatch spriteBatch;

        Dictionary<string, Color[]> spriteColors;

        public float verticalOffset;
        public float moveBounceOffset;
        public Vector2 screenShakeOffset;


        //screenshakestuffs
        float shakeIntensity = 0f;
        float shakeDegredation = 0.95f;
        Vector2 shakeDirection;
        Vector2 shakeOffset; //temp holder
        ITween<Vector2> returnToCenterTween;
        bool shaking = false;

        //target holder
        RenderTarget2D renderTarget;

        public float totalVerticalOffset => verticalOffset + moveBounceOffset + screenShakeOffset.Y;

        //public int UiSize = NezGame.uiSize;
        public WolfRenderer(int renderOrder) : base(renderOrder)
        {
            spriteBatch = new SpriteBatch(Core.GraphicsDevice);
            spriteColors = new Dictionary<string, Color[]>();
            renderTarget = new RenderTarget2D(Core.GraphicsDevice, NezGame.designWidth, NezGame.designHeight);
        }

        public void SetCameraShake(float intensity, float degredation, Vector2 direction)
        {
            if (shakeIntensity < intensity)
            {
                shakeDirection = direction;
                shakeIntensity = intensity;
                if (degredation < 0f || degredation >= 1f)
                    degredation = 0.95f;

                shakeDegredation = degredation;
            }
            shaking = true;
        }
        public void Update()
        {
            //camera shake update
            if(shaking && Math.Abs(shakeIntensity) > 0f)
            {
                shakeOffset = shakeDirection;
                if (shakeOffset.X != 0f || shakeOffset.Y != 0f)
                {
                    shakeOffset.Normalize();
                }
                else
                {
                    shakeOffset.X = shakeOffset.X + Nez.Random.NextFloat() - 0.5f;
                    shakeOffset.Y = shakeOffset.Y + Nez.Random.NextFloat() - 0.5f;
                }

                // TODO: this needs to be multiplied by camera zoom so that less shake gets applied when zoomed in
                shakeOffset *= shakeIntensity;
                shakeIntensity *= -shakeDegredation;
                screenShakeOffset = shakeOffset;
                if (Math.Abs(shakeIntensity) <= 0.1f)
                {
                    shakeIntensity = 0f;
                    shaking = false;
                    returnToCenterTween?.Stop();
                    returnToCenterTween = this.Tween("screenShakeOffset", Vector2.Zero, 0.2f);
                    returnToCenterTween.Start();
                }
            }

            //slap in sprites
            Array.Sort(sprites); //sorting far to near using CompareTo
            for (int i = 0; i < sprites.Length; i++)
            {
                var sprite = sprites[i];
                if (sprite.SpriteColors == null || !sprite.Enabled || sprite.Entity == null) continue;
                var transY = sprite.transformY;
                if(transY <= 0f) continue; // transY is how far in front 0/negative means its not drawn anyway, skip all the other calc at this point
                var transX = sprite.transformX;
                var spriteToScreenX = (int)((NezGame.designWidth / 2) * (1 + transX / transY));
                var spriteWidth = sprite.spriteWidth;
                var spriteHeight = sprite.spriteHeight;
                if (spriteHeight > NezGame.h || spriteHeight < 0f) spriteHeight = NezGame.h;
                var spriteSourceWidth = (int)sprite.SpriteWidth();
                int moveVertical = (int)(totalVerticalOffset / transY);
                int startAdj = (int)(sprite.drawStartY + moveVertical);
                if (startAdj < 0) startAdj = 0;
                int endAdj = (int)(sprite.drawEndY + moveVertical);
                if (endAdj > NezGame.h) endAdj = NezGame.h - 1;
                int xOffset = (int)(screenShakeOffset.X / transX);
                int startAdjX = (int)(sprite.drawStartX - xOffset);
                int endAdjX = (int)(sprite.drawEndX - xOffset);
                //loop through every vertical stripe of the sprite on screen
                for (int stripe = startAdjX; stripe < endAdjX; stripe++)
                {
                    if (stripe <= 0 || stripe >= NezGame.designWidth) continue;
                    int texX = (int)(256 * (stripe + xOffset - (-spriteWidth / 2 + spriteToScreenX)) * spriteSourceWidth / spriteWidth) / 256;
                    if (sprite.flipX) texX = spriteSourceWidth - texX - 1;
                    for (int y = startAdj; y < endAdj; y++)
                    {
                        //the conditions in the if are:
                        //1) it's in front of camera plane so you don't see things behind you
                        //2) it's on the screen (left)
                        //3) it's on the screen (right)
                        //4) ZBuffer, with perpendicular distance
                        if (transY < zBuffer[y, stripe])//4
                        {
                            int d = ((int)(y - (int)moveVertical) * 256) - (NezGame.h * 128) + (spriteHeight * 128);  //256 and 128 factors to avoid floats
                            int texY = ((int)(d * spriteSourceWidth) / spriteHeight) / 256;
                            if (texY < 0) texY = 0;
                            else if (texY >= spriteSourceWidth) texY = spriteSourceWidth - 1;
                            var color = sprite.SpriteColors[spriteSourceWidth * texY + texX];
                            if (color.A != Color.Transparent.A)
                            {
                                if (sprite.flashing)
                                {
                                    colorBuffer[y, stripe] = sprite.flashColor;
                                }
                                else
                                {
                                    colorBuffer[y, stripe] = color;
                                }
                            }
                        }
                    }
                }
            }
        }


        public override void Render(Scene scene)
        {            
            //draw from the buffer
            spriteBatch.Begin();
            var joinedBuffer = new Color[colorBuffer.Length];
            var xLen = colorBuffer.GetLength(1);
            var yLen = colorBuffer.GetLength(0);
            for(int x = 0; x < xLen; x++)
            {
                for(int y = 0; y < yLen; y++)
                {
                    joinedBuffer[x + (y * NezGame.designWidth)] = colorBuffer[y, x];
                }
            }
            renderTarget.SetData<Color>(joinedBuffer, 0, joinedBuffer.Length);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

    }
}
