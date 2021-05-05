using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolf3D.Weapons
{
    public enum AmmoType
    {
        Pistol,
        Shotgun,
        MachineGun,
        Explosive,
        Melee
    }

    //public enum HandState
    //{
    //    PistolIdle,
    //    PistolShoot,
    //    MachineGunIdle,
    //    MachineGunShoot,
    //    ShotgunIdle,
    //    ShotgunShoot,
    //    RocketLauncherIdle,
    //    RocketLauncherShoot,
    //    GattlingIdle,
    //    GattlingShoot,
    //    MeleeSwing
    //}

    public interface IWeapon
    {
        AmmoType ammoType { get; }
        string idleHands { get; }
        string shootHands { get; }
        int ammo { get; set; }
        TextComponent ammoCountTextComponent { get; }
        void Shoot();
        void ShotReleased();
        SpriteAnimator handsSprite { get; }
        Components.PlayerState playerState { get; }
    }
}
