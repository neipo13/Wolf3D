using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Components;
using Wolf3D.Util;

namespace Wolf3D.Entities
{
    public enum HitEffectType
    {
        spark,
        blood1
    }
    public class HitEffect : Entity, IPoolable
    {
        AnimatedWolfSprite sprite;

        public HitEffect(Scene scene, PlayerState playerState)
        {
            var texture = scene.Content.Load<Texture2D>("img/effects");
            var Sprites = Sprite.SpritesFromAtlas(texture, 48, 48).ToArray();
            sprite = new AnimatedWolfSprite(playerState);
            this.AddComponent(sprite);
            sprite.AddAnimation(HitEffectType.spark.ToString(), new Nez.Sprites.SpriteAnimation(Sprites.Skip(0).Take(3).ToArray(), 12));
            sprite.AddAnimation(HitEffectType.blood1.ToString(), new Nez.Sprites.SpriteAnimation(Sprites.Skip(3).Take(3).ToArray(), 12));
            sprite.Play(HitEffectType.spark.ToString(), AnimatedWolfSprite.LoopMode.ClampForever);
            sprite.OnAnimationCompletedEvent += OnAnimCompleted;
        }

        private void OnAnimCompleted(string obj)
        {
            HitEffectPool.free(this);
        }
        public override void OnRemovedFromScene()
        {
            sprite.OnAnimationCompletedEvent -= OnAnimCompleted;
            base.OnRemovedFromScene();
        }

        public void Reset()
        {
            // just move offscreen for now
            Position = new Microsoft.Xna.Framework.Vector2(-100, -100);
            sprite.Enabled = false;
        }

        public void PlayEffect(HitEffectType type)
        {
            sprite.Enabled = true;
            sprite.Play(type.ToString(), AnimatedWolfSprite.LoopMode.ClampForever);
        }
    }
}
