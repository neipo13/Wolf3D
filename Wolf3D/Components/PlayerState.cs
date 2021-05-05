using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wolf3D.Weapons;
using Wolf3D.Util;

namespace Wolf3D.Components
{
    public class PlayerState : Component
    {
        private Dictionary<AmmoType, int> Ammo = new Dictionary<AmmoType, int>();
        protected Vector2 direction = new Vector2(-1, 0);
        protected Vector2 plane = new Vector2(0, 0.66f);
        public event EventHandler<AmmoUpdatedEvent> onAmmoUpdated;
        
        public int GetAmmo(AmmoType type)
        {
            if (Ammo.ContainsKey(type))
            {
                return Ammo[type];
            }
            else
            {
                return -1;
            }
        }

        public int SetAmmo(AmmoType type, int value)
        {
            bool newPickup = false;
            if (Ammo.ContainsKey(type))
            {
                Ammo[type] = value;
            }
            else
            {
                Ammo.Add(type, value);
                newPickup = true;
            }
            onAmmoUpdated?.Invoke(this, new AmmoUpdatedEvent(type,value, newPickup));
            return Ammo[type];
        }

        public Vector2 Direction => direction;
        public Vector2 Plane => plane;

        public Vector2 RayCasterPosition
        {
            get
            {
                if (this.Entity == null) return Vector2.Zero;
                return this.Entity.Position / 10f;
            }
        }

        public void rotate(float rSpeed)
        {
            //both camera direction and camera plane must be rotated
            float oldDirX = direction.X;
            direction.X = direction.X * Mathf.Cos(rSpeed) - direction.Y * Mathf.Sin(rSpeed);
            direction.Y = oldDirX * Mathf.Sin(rSpeed) + direction.Y * Mathf.Cos(rSpeed);
            float oldPlaneX = plane.X;
            plane.X = plane.X * Mathf.Cos(rSpeed) - plane.Y * Mathf.Sin(rSpeed);
            plane.Y = oldPlaneX * Mathf.Sin(rSpeed) + plane.Y * Mathf.Cos(rSpeed);
        }

        //transform sprite with the inverse camera matrix
        // [ planeX   dirX ] -1                                       [ dirY      -dirX ]
        // [               ]       =  1/(planeX*dirY-dirX*planeY) *   [                 ]
        // [ planeY   dirY ]                                          [ -planeY  planeX ]
        public float invDet => 1f / (plane.X * direction.Y - direction.X * plane.Y);

        public override void DebugRender(Batcher batcher)
        {
            base.DebugRender(batcher);
            //also draw a line for directionwd
            batcher.DrawLineAngle(this.Entity.Position, Mathf.Atan2(direction.Y, direction.X), 10f, Color.Yellow);

        }
    }
}
