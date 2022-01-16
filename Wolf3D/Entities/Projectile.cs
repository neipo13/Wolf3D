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
        Spike,
        Grenade,
        Disc
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
            var nadeAnim = new SpriteAnimation(Sprites.Skip(15).Take(10).ToArray(), 12);
            sprite.AddAnimation(ParticleType.Grenade.ToString(), nadeAnim);
            var discAnim = new SpriteAnimation(Sprites.Skip(6).Take(2).ToArray(), 12);
            sprite.AddAnimation(ParticleType.Disc.ToString(), discAnim);
            var loopMode = AnimatedWolfSprite.LoopMode.ClampForever;
            if(type == ParticleType.RedFlash || type == ParticleType.Disc)
            {
                loopMode = AnimatedWolfSprite.LoopMode.Loop;
            }
            sprite.Play(type.ToString(), loopMode);
            AddComponent(sprite);

            sprite.OnAnimationCompletedEvent += Sprite_onAnimationCompletedEvent;

            //collider
            BoxCollider collider = new BoxCollider(2f, 2f);
            collider.IsTrigger = true;
            collider.PhysicsLayer = physicsLayer;
            collider.CollidesWithLayers = collidesWithLayers;
            AddComponent(collider);

            //mover
            Mover mover = new Mover();
            AddComponent(mover);

            //controller
            Components.ProjectileMover controller = new Components.ProjectileMover(
                direction, 
                speed, 
                damage, 
                explodes:type == ParticleType.Grenade, 
                bounces: type == ParticleType.Disc
                );
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
            else if (type == ParticleType.Grenade.ToString())
            {
                if (this.Enabled)
                {
                    this.Enabled = false;
                    //spawn explosion
                    var physicsLayer = -1;
                    Nez.Flags.SetFlagExclusive(ref physicsLayer, (int)Constants.PhysicsLayer.PlayerShot);
                    var explosion = new Explosion(Scene, Position, physicsLayer, sprite.playerState, 50, 10f);
                    explosion.Position = this.Position;
                    Scene.AddEntity(explosion);
                    this.Destroy();
                }
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
