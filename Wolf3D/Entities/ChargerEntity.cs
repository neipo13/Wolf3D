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
    public enum ChargerState
    {
        Idle,
        Alert,
        Charge,
        Swing,
        Gib,
        Hit,
        Dead,
        Wait
    }

    public class ChargerEntity : ShootableEntity
    {
        public AnimatedWolfSprite sprite;
        BoxCollider collider;
        PlayerState playerState;
        List<Sprite> gibSprites;
        ChargerController controller;
        SoundEffect[] hitEffects;

        bool dead;
        float gibChance = 0.2f;

        public ChargerEntity(PlayerState playerState, List<Sprite> Sprites, int[,] map, List<Sprite> gibSprites, List<SoundEffect> hitEffects) : base("solider-enemy")
        {
            this.playerState = playerState;
            this.gibSprites = gibSprites;
            dead = false;
            this.hitEffects = hitEffects.ToArray();

            collider = new BoxCollider(6, 6);
            Flags.SetFlagExclusive(ref collider.PhysicsLayer, (int)Constants.PhysicsLayer.Enemy);

            Flags.SetFlagExclusive(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Wall);
            Flags.SetFlag(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.PlayerShot);
            Flags.SetFlag(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.SemiSolidWall);
            Flags.SetFlag(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Player);

            Console.WriteLine($"ENEMY: {Nez.Flags.BinaryStringRepresentation(collider.PhysicsLayer)}");

            sprite = new AnimatedWolfSprite(playerState);
            sprite.ShowClose = true;
            sprite.AddAnimation(ChargerState.Idle.ToString(), new SpriteAnimation(new Sprite[] { Sprites[7] }, 12));


            var hitAnim = new SpriteAnimation(Sprites.Skip(14).Take(1).ToArray(), 12);
            sprite.AddAnimation(ChargerState.Hit.ToString(), hitAnim);

            var walkAnim = new SpriteAnimation(Sprites.Take(6).ToArray(), 24);
            sprite.AddAnimation(ChargerState.Charge.ToString(), walkAnim);

            var shootAnim = new SpriteAnimation(Sprites.Skip(9).Take(5).ToArray(), 12);
            sprite.AddAnimation(ChargerState.Alert.ToString(), shootAnim);

            sprite.Play(ChargerState.Idle.ToString());

            var state = new PlayerState();

            var mover = new Mover();

            controller = new ChargerController();


            AddComponent(collider);
            AddComponent(sprite);
            AddComponent(mover);
            AddComponent(state);
            AddComponent(controller);

            hp = 6;
            HitEffectType = HitEffectType.blood1;
        }

        public override void Hit(int dmg)
        {
            var oldAnim = sprite.CurrentAnimation;
            sprite.flashing = true;
            sprite.Play(ChargerState.Hit.ToString(), AnimatedWolfSprite.LoopMode.ClampForever);
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
            sprite.ShowClose = false;
            hitEffects[1].Play(0.4f * NezGame.gameSettings.sfxVolumeMultiplier, 0f, 0f);
            // for now just shut stuff off no need to delete it
            //sprite.Play(ChargerState.Dead.ToString(), AnimatedWolfSprite.LoopMode.ClampForever);
            sprite.Enabled = false;
            collider.Enabled = false;
            controller.Enabled = false;
            RemoveComponent(collider);
            RemoveComponent(controller);


            var wolfRenderer = ((Scenes.WolfScene)(this.Scene)).wolfRenderer;

            //sometimes spawn gibs
            hitEffects[2].Play(0.4f * NezGame.gameSettings.sfxVolumeMultiplier, 0f, 0f);
            //sprite.Play(ChargerState.Gib.ToString(), AnimatedWolfSprite.LoopMode.ClampForever);
            int gibNum = Nez.Random.Range(5, 10);
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
                GibType type = GibType.eyeball;
                var x = Nez.Random.Range(-1f, 1f);
                var y = Nez.Random.Range(-1f, 1f);
                var gib = new GibEntity(playerState, gibSprites, type, new Vector2(x, y));
                gib.Position = this.Position;
                wolfRenderer.sprites[index] = gib.GetComponent<WolfSprite>();
                index++;
                this.Scene.AddEntity(gib);
            }
            //this.Destroy();
        }
    }
}
