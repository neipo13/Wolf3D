using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Components;
using Wolf3D.Entities;
using Wolf3D.Util;

namespace Wolf3D.Weapons
{
    /*
     * https://www.denofgeek.com/games/how-dooms-shotgun-leveled-a-punishing-playing-field/
     * The shotgun’s pellets do an estimated 5-15 damage based on accuracy, 
     * which is the same damage as your pistol’s bullets. However, as the DOOM 
     * shotgun fires seven pellets at once in a spread formation, the actual total damage output is 35-105 points.
     * */
    public class Shotgun : IWeapon
    {
        public AmmoType ammoType => AmmoType.Shotgun;

        public string idleHands => Constants.HandStates.SHOTGUN_IDLE;

        public string shootHands => Constants.HandStates.SHOTGUN_SHOOT;

        public int ammo
        {
            get
            {
                return playerState.GetAmmo(ammoType);
            }
            set
            {
                playerState.SetAmmo(ammoType, value);
            }
        }
        public SpriteAnimator handsSprite { get; private set; }

        public PlayerState playerState { get; private set; }

        private bool shotHeld = false;
        public bool shooting = false;
        public float reloadTime = 0.5f;

        int pelletsPerShot = 5;

        public TextComponent ammoCountTextComponent { get; private set; }
        public SoundEffect shotSound { get; private set; }

        public Shotgun(SpriteAnimator sprite, PlayerState state, TextComponent ammoText, SoundEffect shotSound)
        {
            handsSprite = sprite;
            playerState = state;
            ammoCountTextComponent = ammoText;
            //ammo = 15;
            this.shotSound = shotSound;
        }

        public void Shoot()
        {
            if (shooting || shotHeld || ammo < 1) return;
            shotHeld = true;
            shooting = true;
            Core.Schedule(reloadTime, (t) => shooting = false);
            handsSprite.Play(shootHands, SpriteAnimator.LoopMode.ClampForever);
            ammo--;
            ammoCountTextComponent.SetText(ammo.ToString());
            shotSound.Play(0.2f * NezGame.gameSettings.sfxVolumeMultiplier, 0f, 0f);

            //fire 7 bullets but do them in order so that dead enemies do not block shots
            for (int i = 0; i < pelletsPerShot; i++)
            {
                SingleShot();
            }
        }

        public void SingleShot()
        {
            var randomness = Nez.Random.Range(-Mathf.PI / 20f, Mathf.PI / 20f);
            var theta = Mathf.Atan2(playerState.Direction.Y, playerState.Direction.X) + randomness;
            var endPosition = playerState.Entity.Position + Mathf.AngleToVector(theta, 1000f);

            int flag = -1;
            Nez.Flags.SetFlagExclusive(ref flag, (int)Constants.PhysicsLayer.Enemy);
            Flags.SetFlag(ref flag, (int)Constants.PhysicsLayer.Wall);
            var ray = Nez.Physics.Linecast(playerState.Entity.Position, endPosition, flag);


            if (ray.Collider != null)
            {
                ShootableEntity shootable = ray.Collider.Entity as ShootableEntity;
                if (shootable != null)
                {
                    var towardPlayer = playerState.Entity.Position - ray.Point;
                    Vector2Ext.Normalize(ref towardPlayer);
                    towardPlayer *= 0.1f;
                    var effectposition = ray.Point + towardPlayer;
                    shootable.Hit(1, effectposition, playerState);
                }
                else
                {
                    var effect = HitEffectPool.obtain(playerState.Entity.Scene, playerState);
                    var towardPlayer = playerState.Entity.Position - ray.Point;
                    Vector2Ext.Normalize(ref towardPlayer);
                    towardPlayer *= 0.1f;
                    effect.Position = ray.Point + towardPlayer;
                    effect.PlayEffect(HitEffectType.spark);
                }
            }
        }

        public void ShotReleased()
        {
            shotHeld = false;
        }
    }
}
