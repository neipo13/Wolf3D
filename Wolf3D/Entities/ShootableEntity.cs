using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Components;
using Wolf3D.Util;

namespace Wolf3D.Entities
{
    public abstract class ShootableEntity : Entity
    {
        public ShootableEntity() { }
        public ShootableEntity(string name) : base(name) { }
        public virtual void Hit(int dmg)
        {
            hp -= dmg;
            if (hp <= 0) OnDeath();
        }
        public virtual void Hit(int dmg, Vector2 hitLocation, PlayerState playerState)
        {

            var effect = HitEffectPool.obtain(playerState.Entity.Scene, playerState);
            effect.Position = hitLocation;
            effect.PlayEffect(HitEffectType);
            Hit(dmg);
        }
        public int physicsLayer { get; protected set; }
        public int hp { get; protected set; }

        public abstract void OnDeath();

        public HitEffectType HitEffectType { get; set; }
    }
}
