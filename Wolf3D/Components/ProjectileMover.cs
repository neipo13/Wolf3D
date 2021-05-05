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
    public class ProjectileMover : Component, IUpdatable
    {
        Mover mover;
        CollisionResult collisionResult;
        Vector2 direction;
        float speed;
        int dmg;

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
        }

        public void Update()
        {
            mover.Move(direction * speed * Time.DeltaTime, out collisionResult);

            if(collisionResult.Collider != null)
            {
                ShootableEntity e = collisionResult.Collider.Entity as ShootableEntity;
                if(e != null)
                {
                    e.Hit(dmg);
                }
                this.Entity.Destroy();
            }
        }
    }
}
