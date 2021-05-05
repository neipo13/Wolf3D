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
    public class Melee : IWeapon
    {
        public AmmoType ammoType => AmmoType.Melee;

        public string idleHands => Constants.HandStates.PISTOL_IDLE;

        public string shootHands => Constants.HandStates.MELEE_SHOOT;

        public int ammo
        {
            get
            {
                return 8;
            }
            set
            {
                
            }
        }
        public SpriteAnimator handsSprite { get; private set; }

        public PlayerState playerState { get; private set; }

        public SoundEffect shotSound { get; private set; }
        
        public bool shooting = false;

        public float reloadTime = 0.4f;

        public TextComponent ammoCountTextComponent { get; private set; }

        public Melee(SpriteAnimator sprite, PlayerState state, TextComponent ammoText, SoundEffect shotSound)
        {
            handsSprite = sprite;
            playerState = state;
            ammoCountTextComponent = ammoText;
            //ammo = 40;
            this.shotSound = shotSound;
        }

        public void Shoot()
        {
            if (shooting) return;
            shooting = true;
            Core.Schedule(reloadTime, (t) => shooting = false);
            handsSprite.Play(shootHands, SpriteAnimator.LoopMode.ClampForever);

            HudFaceAnimator faceAnimator = (this.playerState.Entity as PlayerEntity)?.faceAnimator;
            faceAnimator?.MeleeAttack();
            var thetas = new float[3];
            thetas[0] = Mathf.Atan2(playerState.Direction.Y, playerState.Direction.X);
            thetas[1] = thetas[0] + -Mathf.PI / 8f;
            thetas[2] = thetas[0] + Mathf.PI / 8f;
            var endPosition = playerState.Entity.Position + Mathf.AngleToVector(thetas[0], 15f);
            var endPostion2 = playerState.Entity.Position + Mathf.AngleToVector(thetas[1], 15f);
            var endPostion3 = playerState.Entity.Position + Mathf.AngleToVector(thetas[2], 15f);

            int flag = -1;
            Nez.Flags.SetFlagExclusive(ref flag, (int)Constants.PhysicsLayer.Enemy);
            var ray = Nez.Physics.Linecast(playerState.Entity.Position, endPosition, flag);
            var ray2 = Nez.Physics.Linecast(playerState.Entity.Position, endPostion2, flag);
            var ray3 = Nez.Physics.Linecast(playerState.Entity.Position, endPostion3, flag);

            shotSound.Play(0.2f * NezGame.gameSettings.sfxVolumeMultiplier, 0f, 0f);

            if (ray.Collider != null) TryShot(ray);
            else if (ray2.Collider != null) TryShot(ray2);
            else if (ray3.Collider != null) TryShot(ray3);
        }

        public void TryShot(RaycastHit ray)
        {
            ShootableEntity shootable = ray.Collider.Entity as ShootableEntity;
            if (shootable != null)
            {
                var towardPlayer = playerState.Entity.Position - ray.Point;
                Vector2Ext.Normalize(ref towardPlayer);
                towardPlayer *= 0.1f;
                var effectposition = ray.Point + towardPlayer;
                shootable.Hit(2, effectposition, playerState);
            }
        }

        public void ShotReleased() { }
    }
}
