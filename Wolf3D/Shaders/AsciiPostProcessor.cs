using Microsoft.Xna.Framework.Graphics;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolf3D.Shaders
{
    public class AsciiPostProcessor : PostProcessor
    {
        EffectParameter characterTexture;
        EffectParameter tilesX;
        EffectParameter tilesY;
        EffectParameter characterWidth;
        EffectParameter characterHeight;
        EffectParameter characterCount;
        EffectParameter brightness;
        EffectParameter screenHeight;
        EffectParameter screenWidth;

        float charSize = 10f;

        public AsciiPostProcessor(int exectionOrder) : base(exectionOrder, null) { }

        public override void OnAddedToScene(Scene scene)
        {
            base.OnAddedToScene(scene);
            Effect = scene.Content.Load<Effect>("shaders/AsciiRenderer");
            characterTexture = Effect.Parameters["_CharTex"];
            tilesX = Effect.Parameters["_tilesX"];
            tilesY = Effect.Parameters["_tilesY"];
            characterWidth = Effect.Parameters["_tilesW"];
            characterHeight = Effect.Parameters["_tilesH"];
            characterCount = Effect.Parameters["_charCount"];
            brightness = Effect.Parameters["_brightness"];
            screenHeight = Effect.Parameters["screenHeight"];
            screenWidth = Effect.Parameters["screenWidth"];

            var charTex = scene.Content.Load<Texture2D>("img/Charmap");
            characterTexture.SetValue(charTex);
            tilesX.SetValue(NezGame.designWidth / charSize);
            tilesY.SetValue(NezGame.designHeight / charSize);
            characterWidth.SetValue(charSize);
            characterHeight.SetValue(charSize);
            characterCount.SetValue(8);
            brightness.SetValue(0f);
            screenWidth.SetValue(NezGame.designWidth);
            screenHeight.SetValue(NezGame.designHeight);


        }
    }
}
