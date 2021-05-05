using Microsoft.Xna.Framework.Audio;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Components;
using Wolf3D.Util;
using Wolf3D.Weapons;

namespace Wolf3D.Entities
{
    public class AmmoEntity : Entity
    {
        public AmmoEntity(AmmoType type, int value, WolfSprite sprite, SoundEffect soundEffect) : base("ammo")
        {
            // set sprite
            this.AddComponent(sprite);
            // set collider
            var collider = new BoxCollider(10f, 10f);
            collider.IsTrigger = true;
            int collidesWithLayers = 0;
            int physicsLayer = 0;
            Flags.SetFlag(ref physicsLayer, (int)Constants.PhysicsLayer.Pickup);
            Flags.SetFlag(ref collidesWithLayers, (int)Constants.PhysicsLayer.Player);
            collider.PhysicsLayer = physicsLayer;
            collider.CollidesWithLayers = collidesWithLayers;
            this.AddComponent(collider);
            // set the pickup component
            var pickup = new PickupTrigger(PickupType.Ammo, value, soundEffect, type);
            AddComponent(pickup);
        }
    }
}
