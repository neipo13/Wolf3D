using Microsoft.Xna.Framework.Audio;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Entities;
using Wolf3D.Weapons;

namespace Wolf3D.Components
{
    public enum PickupType
    {
        Ammo,
        Health,
        Weapon
    }
    public class PickupTrigger : Component, ITriggerListener
    {
        PickupType pickupType;
        AmmoType weaponType;
        int pickupValue = 0;
        SoundEffect soundEffect;

        public PickupTrigger(PickupType type, int value, SoundEffect soundEffect, AmmoType weaponType = AmmoType.Pistol)
        {
            this.pickupType = type;
            this.pickupValue = value;
            this.soundEffect = soundEffect;
            this.weaponType = weaponType;
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            soundEffect.Play(0.4f * NezGame.gameSettings.sfxVolumeMultiplier, 0f, 0f);
            switch (pickupType)
            {
                case PickupType.Health:
                    ShootableEntity e = other.Entity as ShootableEntity;
                    if (e == null) return;
                    e.Hit(-pickupValue);
                    break;
                case PickupType.Ammo:
                    PlayerState state = other.Entity.GetComponent<PlayerState>();
                    if (state == null) return;
                    state.SetAmmo(weaponType, state.GetAmmo(weaponType) + pickupValue);
                    break;
                case PickupType.Weapon:
                    break;
            }
            local.Enabled = false;
            local.Entity.Enabled = false;
            local.Entity?.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }

        public void onTriggerStay(Collider other, Collider local) { }
    }
}
