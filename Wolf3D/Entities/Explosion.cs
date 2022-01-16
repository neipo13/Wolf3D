using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
    public class Explosion : Entity
    {
        AnimatedWolfSprite sprite;
        public Explosion(Scene scene, Vector2 pos, int physicsLayer, PlayerState playerState, int damage = 1, float radius = 50f)
        {
            var texture = scene.Content.Load<Texture2D>("img/explosion");
            var Sprites = Sprite.SpritesFromAtlas(texture, 48, 48).ToArray();
            var sound = scene.Content.Load<SoundEffect>("sfx/barrel_explode");

            //sprite
            sprite = new AnimatedWolfSprite(playerState);
            //sprite.Scale = new Vector2(0.5f, 0.5f);
            sprite.AddAnimation("basic_boom", new SpriteAnimation(Sprites, 12));
            var loopMode = AnimatedWolfSprite.LoopMode.Once;
            sprite.Play("basic_boom", loopMode);
            AddComponent(sprite);

            var wolfRenderer = ((Scenes.WolfScene)(scene)).wolfRenderer;
            var currentSprites = wolfRenderer.sprites; 
            var newSpriteLength = currentSprites.Length + 1;
            wolfRenderer.sprites = new WolfSprite[newSpriteLength];
            int index = 0;
            while (index < currentSprites.Length)
            {
                wolfRenderer.sprites[index] = currentSprites[index];
                index++;
            }
            wolfRenderer.sprites[index] = sprite;

            sprite.OnAnimationCompletedEvent += Sprite_onAnimationCompletedEvent;

            //check collisions
            Core.Schedule((0.1f), t => CheckCollisions(pos, radius, damage, physicsLayer));

            //sfx
            sound.Play(0.2f * NezGame.gameSettings.sfxVolumeMultiplier, 0f, 0f);

        }

        public void CheckCollisions(Vector2 position, float radius, int damage, int physicsLayer = -1)
        {
            Collider[] collisions = new Collider[25]; // max number of collisions
            var numCols = Nez.Physics.OverlapCircleAll(position, radius, collisions, physicsLayer);
            for (int i = 0; i < numCols; i++)
            {
                var col = collisions[i];
                var shootable = col.Entity as ShootableEntity;
                if (shootable != null)
                {
                    //cast a ray to ensure we dont do damage through walls or other colliders

                    shootable.Hit(damage);
                }
            }
        }

        private void Sprite_onAnimationCompletedEvent(string type)
        {
            if (this.Enabled) this.Destroy();
        }

        public override void OnRemovedFromScene()
        {
            sprite.OnAnimationCompletedEvent -= Sprite_onAnimationCompletedEvent;
            base.OnRemovedFromScene();
        }
    }
}
