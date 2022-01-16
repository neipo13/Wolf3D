using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Entities;

namespace Wolf3D.Components
{
    public class ProjectileMover : Component, IUpdatable, ITriggerListener
    {
        Mover mover;
        CollisionResult collisionResult;
        Vector2 direction;
        float speed;
        int dmg;
        bool explodeOnContact = false;
        bool bounces = false;

        //ColliderTriggerHelper triggerHelper;

        public ProjectileMover(Vector2 direction, float speed, int dmg, bool explodes = false, bool bounces = false)
        {
            this.direction = direction;
            this.speed = speed;
            this.dmg = dmg;
            this.explodeOnContact = explodes;
            this.bounces = bounces;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            this.mover = Entity.GetComponent<Mover>();
            collisionResult = new CollisionResult();
            //triggerHelper = new ColliderTriggerHelper(Entity);
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            ShootableEntity e = other.Entity as ShootableEntity;
            if (bounces && e == null)
            {
                //reflect handled but dont let destroy happen
                local.CollidesWith(other, out collisionResult);
                if(collisionResult.Collider != null)
                {
                    if (Math.Abs(collisionResult.Normal.X) > 0f) direction.X *= -1f;
                    if (Math.Abs(collisionResult.Normal.Y) > 0f) direction.Y *= -1f;
                }
                else
                {
                    // if we didnt find a collider then how tf did we end up here, back that shit up
                    direction *= -1f;
                }
                return;
            }
            if (explodeOnContact)
            {
                //spawn explosion
                var sprite = Entity.GetComponent<Util.WolfSprite>();
                var physicsLayer = -1;
                Nez.Flags.SetFlagExclusive(ref physicsLayer, (int)Constants.PhysicsLayer.PlayerShot);
                var explosion = new Explosion(Entity.Scene, Entity.Position, physicsLayer, sprite.playerState, 50, 50f);
                explosion.Position = Entity.Position;
                Entity.Scene.AddEntity(explosion);
            }
            if (e != null)
            {
                e.Hit(dmg);
            }
            this.Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }

        public void Update()
        {
            mover.Move(direction * speed * Time.DeltaTime, out collisionResult);

            //triggerHelper.Update();
        }
    }
}
