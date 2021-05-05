using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Util;

namespace Wolf3D.Components
{
    public class GibController : Component, IUpdatable
    {
        private WolfSprite sprite;
        private PlayerState playerState;
        private Mover mover;
        private BoxCollider collider;
        private CollisionResult collisionResult;
        private Vector2 moveDir;
        private float moveSpeed;
        private float drag = 100f;

        public bool stopped = false;

        public GibController(PlayerState playerState, Vector2 moveDir, float moveSpeed, float drag = 100f )
        {
            moveDir = Vector2Ext.Normalize(moveDir); //normalize this to apply move speed correctly
            this.moveDir = moveDir;
            this.moveSpeed = moveSpeed;
            this.drag = drag;
            this.playerState = playerState;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            sprite = this.Entity.GetComponent<WolfSprite>();
            mover = this.Entity.GetComponent<Mover>();
            collider = this.Entity.GetComponent<BoxCollider>();
            collisionResult = new CollisionResult();
        }

        public void Update()
        {
            //if we stopped remove the mover 
            if(moveSpeed <= 0f && !stopped)
            {
                stopped = true;
                this.Entity.RemoveComponent(mover);
                this.Entity.RemoveComponent(collider);
                this.moveSpeed = 0f;
            }

            if (!stopped)
            {
                //move
                var moveStuff = moveDir * moveSpeed * Time.DeltaTime;
                mover.Move(moveStuff, out collisionResult);

                //slow
                if (collisionResult.Collider != null)
                {
                    moveSpeed = -1f;
                }
                else
                {
                    moveSpeed = Mathf.Approach(moveSpeed, -1f, drag * Time.DeltaTime);
                }

                //flip x stuff?
            }
            else
            {
                //just do flipx stuff
            }
        }
    }
}
