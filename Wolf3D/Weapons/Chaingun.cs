using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Sprites;
using Wolf3D.Components;
using Wolf3D.Entities;
using Wolf3D.Util;

namespace Wolf3D.Weapons
{
    public class Chaingun : IWeapon
    {
        public AmmoType ammoType => AmmoType.MachineGun;

        public string idleHands => Constants.HandStates.CHAINGUN_IDLE;

        public string shootHands => Constants.HandStates.CHAINGUN_SHOOT;

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

        public bool shooting = false;
        public float reloadTime = 0.15f;
        public TextComponent ammoCountTextComponent { get; private set; }
        public SoundEffect shotSound { get; private set; }

        public Chaingun(SpriteAnimator sprite, PlayerState state, TextComponent ammoText, SoundEffect shotSound)
        {
            handsSprite = sprite;
            playerState = state;
            ammoCountTextComponent = ammoText;
            //ammo = 50;
            this.shotSound = shotSound;
        }

        public void Shoot()
        {
            if (shooting || ammo < 1) return;
            shooting = true;
            Core.Schedule(reloadTime, (t) => shooting = false);
            if (handsSprite.CurrentAnimationName != shootHands)
            {
                handsSprite.Play(shootHands);
            }
            
            ammo--;
            ammoCountTextComponent.SetText(ammo.ToString());

            var theta = Mathf.Atan2(playerState.Direction.Y, playerState.Direction.X);
            var endPosition = playerState.Entity.Position + Mathf.AngleToVector(theta, 1000f);

            int flag = -1;
            Nez.Flags.SetFlagExclusive(ref flag, (int)Constants.PhysicsLayer.Enemy);
            Flags.SetFlag(ref flag, (int)Constants.PhysicsLayer.Wall);
            var ray = Nez.Physics.Linecast(playerState.Entity.Position, endPosition, flag);

            shotSound.Play(0.2f * NezGame.gameSettings.sfxVolumeMultiplier, 0f, 0f);

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
                    effect.Position = ray.Point + towardPlayer;
                    effect.PlayEffect(HitEffectType.spark);
                }
            }
        }

        public void ShotReleased()
        {
            handsSprite.Play(idleHands, SpriteAnimator.LoopMode.ClampForever);
        }
    }
}
