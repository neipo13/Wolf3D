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
    public class Toaster : IWeapon
    {
        public AmmoType ammoType => AmmoType.Disc;

        public string idleHands => Constants.HandStates.PISTOL_IDLE;

        public string shootHands => Constants.HandStates.PISTOL_SHOOT;

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

        public SoundEffect shotSound { get; private set; }

        public bool shotHeld = false;
        public bool shooting = false;

        public float reloadTime = 0.2f;

        public TextComponent ammoCountTextComponent { get; private set; }

        public Toaster(SpriteAnimator sprite, PlayerState state, TextComponent ammoText, SoundEffect shotSound)
        {
            handsSprite = sprite;
            playerState = state;
            ammoCountTextComponent = ammoText;
            //ammo = 40;
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

            var theta = Mathf.Atan2(playerState.Direction.Y, playerState.Direction.X);
            var dir = Mathf.AngleToVector(theta, 1f);

            int collidesWith = -1;
            Nez.Flags.SetFlagExclusive(ref collidesWith, (int)Constants.PhysicsLayer.Enemy);
            Flags.SetFlag(ref collidesWith, (int)Constants.PhysicsLayer.Wall);
            int physicsLayer = -1;
            Nez.Flags.SetFlagExclusive(ref physicsLayer, (int)Constants.PhysicsLayer.PlayerShot);

            shotSound.Play(0.2f * NezGame.gameSettings.sfxVolumeMultiplier, 0f, 0f);

            var wolfRenderer = ((Scenes.WolfScene)(playerState.Entity.Scene)).wolfRenderer;
            var proj = new Projectile(playerState.Entity.Scene, ParticleType.Disc, dir, physicsLayer, collidesWith, playerState, 10, 150f, 10f);
            proj.Position = playerState.Entity.Position;
            wolfRenderer.AddWolfSprite(proj.GetComponent<WolfSprite>());
            playerState.Entity.Scene.AddEntity(proj);
        }

        public void ShotReleased()
        {
            shotHeld = false;
        }
    }
}
