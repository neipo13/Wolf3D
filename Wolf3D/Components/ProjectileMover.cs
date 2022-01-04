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

        ColliderTriggerHelper triggerHelper;

        public ProjectileMover(Vector2 direction, float speed, int dmg)
        {
            this.direction = direction;
            this.speed = speed;
            this.dmg = dmg;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            this.mover = Entity.GetComponent<Mover>();
            collisionResult = new CollisionResult();
            triggerHelper = new ColliderTriggerHelper(Entity);
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            ShootableEntity e = other.Entity as ShootableEntity;
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

            triggerHelper.Update();
            //if(collisionResult.Collider != null)
            //{
            //    ShootableEntity e = collisionResult.Collider.Entity as ShootableEntity;
            //    if(e != null)
            //    {
            //        e.Hit(dmg);
            //    }
            //    this.Entity.Destroy();
            //}
        }
    }
}
