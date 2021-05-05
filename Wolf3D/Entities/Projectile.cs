using Microsoft.Xna.Framework;
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
using Wolf3D.Constants;
using Wolf3D.Util;

namespace Wolf3D.Entities
{
    public enum ParticleType
    {
        RedFlash,
        Spike
    }
    public class Projectile : Entity
    {
        AnimatedWolfSprite sprite;
        public Projectile(Scene scene, ParticleType type, Vector2 direction, int physicsLayer, int collidesWithLayers, PlayerState playerState, int damage = 1, float speed = 40f, float maxLifespan = 2f)
        {
            var texture = scene.Content.Load<Texture2D>("img/effects");
            var Sprites = Sprite.SpritesFromAtlas(texture, 48, 48).ToArray();
            //sprite
            sprite = new AnimatedWolfSprite(playerState);
            sprite.AddAnimation(ParticleType.RedFlash.ToString(), new SpriteAnimation(Sprites.Skip(6).Take(2).ToArray(), 12));
            var spikeAnim = new SpriteAnimation(Sprites.Skip(8).Take(7).ToArray(), 12);
            sprite.AddAnimation(ParticleType.Spike.ToString(), spikeAnim);
            var loopMode = AnimatedWolfSprite.LoopMode.ClampForever;
            if(type == ParticleType.RedFlash)
            {
                loopMode = AnimatedWolfSprite.LoopMode.Loop;
            }
            sprite.Play(type.ToString(), loopMode);
            AddComponent(sprite);

            sprite.OnAnimationCompletedEvent += Sprite_onAnimationCompletedEvent;

            //collider
            BoxCollider collider = new BoxCollider(2f, 2f);
            collider.PhysicsLayer = physicsLayer;
            collider.CollidesWithLayers = collidesWithLayers;
            AddComponent(collider);

            //mover
            Mover mover = new Mover();
            AddComponent(mover);

            //controller
            Components.ProjectileMover controller = new Components.ProjectileMover(direction, speed, damage);
            AddComponent(controller);

            Core.Schedule(maxLifespan, (t) =>
            {
                if (this.Enabled) this.Destroy();
            });
        }

        private void Sprite_onAnimationCompletedEvent(string type)
        { 
            if(type == ParticleType.Spike.ToString())
            {
                if (this.Enabled) this.Destroy();
            }
        }

        public override void OnRemovedFromScene()
        {
            sprite.OnAnimationCompletedEvent -= Sprite_onAnimationCompletedEvent;
            base.OnRemovedFromScene();
        }

        public void reset()
        {

        }
    }
}
