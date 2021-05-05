using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wolf3D.Components;
using Nez.Sprites;
using Nez.Textures;
using Wolf3D.Renderers;
using Nez.Tweens;
using Microsoft.Xna.Framework.Audio;
using Wolf3D.Scenes;

namespace Wolf3D.Entities
{
    public class PlayerEntity : ShootableEntity
    {
        public PlayerController controller;
        TextComponent healthLabel;
        TextComponent ammoLabel;
        SpriteRenderer hitEffectSprite;
        ITween<Color> tween;
        WolfRenderer wolfRenderer;
        SoundEffect[] soundEffects;
        SpriteAnimator hudSprite;
        SpriteAnimator handsSprite;
        public HudFaceAnimator faceAnimator;

        public override void Hit(int dmg)
        {
            if (hp <= 0) return;
            base.Hit(dmg);
            if (hp <= 0) return;
            healthLabel.SetText(hp.ToString());
            if(dmg > 0)
            {
                faceAnimator.TakeDamage();
                soundEffects[0].Play(0.4f * NezGame.gameSettings.sfxVolumeMultiplier, 0f, 0f);
                hitEffectSprite.Enabled = true;
                hitEffectSprite.Color = Color.White;
                tween?.Stop(false);
                tween = hitEffectSprite.TweenColorTo(Color.Transparent, 0.05f).SetCompletionHandler((c) => hitEffectSprite.Enabled = false);
                tween.Start();
                wolfRenderer.SetCameraShake(3f, 0.85f, default(Vector2));
            }
        }

        public PlayerEntity(int[,] worldMap, SpriteAnimator hudSprite, List<Sprite> faceSprites, TextComponent healthLabel, TextComponent ammoLabel, WolfRenderer wolfRenderer, SpriteRenderer hitEffectSprite, SoundEffect[] soundEffects): base("player")
        {
            this.healthLabel = healthLabel;
            this.ammoLabel = ammoLabel;
            this.hitEffectSprite = hitEffectSprite;
            this.wolfRenderer = wolfRenderer;
            this.soundEffects = soundEffects;
            this.hudSprite = hudSprite;

            //Transform.ShouldRoundPosition = false;
            var moveCollider = new BoxCollider(6f, 6f);

            Flags.SetFlagExclusive(ref moveCollider.PhysicsLayer, (int)Constants.PhysicsLayer.Player);
            Flags.SetFlagExclusive(ref moveCollider.CollidesWithLayers, (int)Constants.PhysicsLayer.Wall);
            Flags.SetFlag(ref moveCollider.CollidesWithLayers, (int)Constants.PhysicsLayer.SemiSolidWall);
            Flags.SetFlag(ref moveCollider.CollidesWithLayers, (int)Constants.PhysicsLayer.EnemyShot);
            Flags.SetFlag(ref moveCollider.CollidesWithLayers, (int)Constants.PhysicsLayer.Enemy);
            Flags.SetFlag(ref moveCollider.CollidesWithLayers, (int)Constants.PhysicsLayer.Pickup);
            Flags.SetFlag(ref moveCollider.CollidesWithLayers, (int)Constants.PhysicsLayer.DoorTrigger);

            AddComponent(moveCollider);
            AddComponent(new PlayerState());
            AddComponent(new Mover());
            controller = AddComponent(new PlayerController(worldMap, ammoLabel, wolfRenderer));

            faceAnimator = new HudFaceAnimator(faceSprites, hudSprite);
            AddComponent(faceAnimator); 

            hp = 100;
            healthLabel.SetText(hp.ToString());
        }

        public void SetHandsSprite(SpriteAnimator handSprite)
        {
            this.handsSprite = handSprite;
            controller.SetHandsSprite(handSprite);
        }

        public override void OnDeath()
        {
            hp = 0;
            healthLabel.SetText(hp.ToString());
            controller.Enabled = false;
            hudSprite.Enabled = false;
            handsSprite.Enabled = false;
            var wolfScene = this.Scene as WolfScene;
            wolfScene?.PlayerDied();
            tween?.Stop(false);
        }

        //public override void debugRender(Graphics graphics)
        //{
        //    base.debugRender(graphics);
        //    graphics.batcher.drawHollowRect(bounds, Debug.Colors.ColliderBounds, Debug.Size.lineSizeMultiplier);
        //    graphics.batcher.drawPolygon(shape.Position, poly.Points, Debug.Colors.ColliderEdge, true, Debug.Size.lineSizeMultiplier);
        //    graphics.batcher.drawPixel(Entity.transform.Position, Debug.Colors.ColliderPosition, 4 * Debug.Size.lineSizeMultiplier);
        //    graphics.batcher.drawPixel(Entity.transform.Position + shape.center, Debug.Colors.ColliderCenter, 2 * Debug.Size.lineSizeMultiplier);
        //}
    }
}
