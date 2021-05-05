using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;
using Nez.Textures;

namespace Wolf3D.Scenes
{
    public enum SplashType
    {
        AllieCatGames,
        Neipo
    }
    public class SplashScreenScene : Scene
    {
        Entity Entity;
        readonly Color hidden = new Color(0, 0, 0, 0);
        readonly Color visible = new Color(255, 255, 255, 255);
        bool transitioning = false;
        SplashType type;
        bool tweenedIn = false;

        public SplashScreenScene(SplashType type)
        {
            this.type = type;
        }

        public override void Initialize()
        {
            base.Initialize();
            ClearColor = Color.Black;
            AddRenderer(new DefaultRenderer());
        }

        public override void OnStart()
        {
            base.OnStart();

            Entity = AddEntity(new Entity());
            Entity.Position = new Vector2(NezGame.designWidth / 2f, NezGame.designHeight / 2f);

            var logoPath = "";
            switch (type)
            {
                case SplashType.AllieCatGames:
                    logoPath = "korbis2AClogo";
                    break;
                case SplashType.Neipo:
                    logoPath = "korbisNeipoLogo";
                    break;
            }
            var logoTex = Content.Load<Texture2D>($"img/{logoPath}");
            var logoSprite = new SpriteRenderer(logoTex);
            logoSprite.Color = hidden;
            Entity.AddComponent(logoSprite);

            var t = 1.5f;
            logoSprite
                .TweenColorTo(visible, t)
                .SetCompletionHandler(c =>
                {
                    logoSprite
                    .TweenColorTo(hidden, t)
                    .SetDelay(t)
                    .SetCompletionHandler(StartNewScene)
                    .Start();
                    tweenedIn = true;
                })
                .Start();
            transitioning = false;
        }

        public override void Update()
        {
            base.Update();
            if (Nez.Input.LeftMouseButtonPressed && tweenedIn)
            {
                StartNewScene(null);
            }
        }

        public void StartNewScene(Nez.Tweens.ITween<Color> c)
        {
            if (!transitioning)
            {
                transitioning = true;
                Scene nextScene = null;
                switch (type)
                {
                    case SplashType.AllieCatGames:
                        nextScene = new Scenes.SplashScreenScene(SplashType.Neipo);
                        break;
                    case SplashType.Neipo:
                        nextScene = new Scenes.MainMenuScene();
                        break;
                }
                Core.StartSceneTransition(new FadeTransition(() => nextScene));
            }
        }

        public override void Unload()
        {
            Entity = null;
            base.Unload();
        }
    }
}
