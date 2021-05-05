using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
    public class SpikerEnemyEntity : ShootableEntity
    {
        public AnimatedWolfSprite sprite;
        BoxCollider collider;
        PlayerState playerState;
        List<Sprite> gibSprites;
        SpikeSoldierController controller;
        SoundEffect[] hitEffects;

        bool dead;
        float gibChance = 0.2f;

        public SpikerEnemyEntity(PlayerState playerState, List<Sprite> Sprites, int[,] map, List<Sprite> gibSprites, List<SoundEffect> hitEffects) : base("solider-enemy")
        {
            this.playerState = playerState;
            this.gibSprites = gibSprites;
            dead = false;
            this.hitEffects = hitEffects.ToArray();

            collider = new BoxCollider(10f, 10f);
            Flags.SetFlagExclusive(ref collider.PhysicsLayer, (int)Constants.PhysicsLayer.Enemy);

            Flags.SetFlagExclusive(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Wall);
            Flags.SetFlag(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.PlayerShot);
            Flags.SetFlag(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.SemiSolidWall);
            Flags.SetFlag(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Player);

            //Console.WriteLine($"ENEMY: {Nez.Flags.BinaryStringRepresentation(collider.PhysicsLayer)}");

            sprite = new AnimatedWolfSprite(playerState);
            sprite.AddAnimation(SoldierSpriteState.Idle.ToString(), new SpriteAnimation(new Sprite[] { Sprites[0] }, 4));
            var walkAnim = new SpriteAnimation(Sprites.Take(2).ToArray(), 12);
            sprite.AddAnimation(SoldierSpriteState.Walk.ToString(), walkAnim);


            var hitAnim = new SpriteAnimation(Sprites.Skip(2).Take(1).ToArray(), 12);
            sprite.AddAnimation(SoldierSpriteState.Hit.ToString(), hitAnim);

            var deadAnim = new SpriteAnimation(Sprites.Skip(2).Take(6).ToArray(), 12);
            sprite.AddAnimation(SoldierSpriteState.Dead.ToString(), deadAnim);

            var gibAnim = new SpriteAnimation(Sprites.Skip(8).Take(7).ToArray(), 12);
            sprite.AddAnimation(SoldierSpriteState.Gib.ToString(), gibAnim);

            var shootAnim = new SpriteAnimation(Sprites.Skip(15).Take(4).ToArray(), 12);
            sprite.AddAnimation(SoldierSpriteState.Shoot.ToString(), shootAnim);

            sprite.Play(SoldierSpriteState.Walk.ToString());

            var state = new PlayerState();

            var mover = new Mover();

            var pathfinder = new ChaseLogic(map, playerState);

            controller = new SpikeSoldierController();


            AddComponent(collider);
            AddComponent(sprite);
            AddComponent(mover);
            AddComponent(state);
            AddComponent(pathfinder);
            AddComponent(controller);

            hp = 5;
            HitEffectType = HitEffectType.blood1;
        }

        public override void Hit(int dmg)
        {
            sprite.flashing = true;
            sprite.Play(SoldierSpriteState.Hit.ToString(), AnimatedWolfSprite.LoopMode.ClampForever);
            controller.Enabled = false;
            Core.Schedule(0.1f, (t) =>
            {
                sprite.flashing = false;
            });
            Core.Schedule(0.3f, (t) =>
            {
                if (!dead)
                {
                    controller.Enabled = true;
                    controller.AfterHit();
                }
            });
            base.Hit(dmg);
            if (!dead)
            {
                hitEffects[0].Play(0.4f * NezGame.gameSettings.sfxVolumeMultiplier, 0f, 0f);
            }
        }

        public override void OnDeath()
        {
            dead = true;
            hitEffects[1].Play(0.4f * NezGame.gameSettings.sfxVolumeMultiplier, 0f, 0f);
            // for now just shut stuff off no need to delete it
            sprite.Play(SoldierSpriteState.Dead.ToString(), AnimatedWolfSprite.LoopMode.ClampForever);
            collider.Enabled = false;
            controller.Enabled = false;
            RemoveComponent(collider);
            RemoveComponent(controller);


            var wolfRenderer = ((Scenes.WolfScene)(this.Scene)).wolfRenderer;

            //sometimes spawn gibs
            if (Nez.Random.NextFloat() < gibChance)
            {
                hitEffects[2].Play(0.4f * NezGame.gameSettings.sfxVolumeMultiplier, 0f, 0f);
                sprite.Play(SoldierSpriteState.Gib.ToString(), AnimatedWolfSprite.LoopMode.ClampForever);
                int gibNum = Nez.Random.Range(3, 7);
                var currentSprites = wolfRenderer.sprites;
                var newSpriteLength = currentSprites.Length + gibNum;
                wolfRenderer.sprites = new WolfSprite[newSpriteLength];
                int index = 0;
                while (index < currentSprites.Length)
                {
                    wolfRenderer.sprites[index] = currentSprites[index];
                    index++;
                }
                for (int i = 0; i < gibNum; i++)
                {
                    GibType type = GibType.solider_head;
                    if (i > 0) type = (GibType)Nez.Random.Range(0, 1);
                    var x = Nez.Random.Range(-1f, 1f);
                    var y = Nez.Random.Range(-1f, 1f);
                    var gib = new GibEntity(playerState, gibSprites, type, new Vector2(x, y));
                    gib.Position = this.Position;
                    wolfRenderer.sprites[index] = gib.GetComponent<WolfSprite>();
                    index++;
                    this.Scene.AddEntity(gib);
                }
            }
            //this.Destroy();
        }
    }
}
