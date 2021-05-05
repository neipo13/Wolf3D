using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolf3D.Components
{
    public enum ChaseState
    {
        Idle,
        Chase
    }
    public class ChaseContoller : Nez.AI.FSM.SimpleStateMachine<ChaseState>
    {
        ChaseLogic pathFinder;
        Mover mover;
        CollisionResult collisionResult;

        const float moveSpeed = 40f;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            mover = Entity.GetComponent<Mover>();
            pathFinder = Entity.GetComponent<ChaseLogic>();
            collisionResult = new CollisionResult();
            InitialState = ChaseState.Idle;
        }

        #region Chase
        void Chase_Tick()
        {
            var target = pathFinder.NextTargetPoint();
            var moveDir = target - Entity.Position;
            Vector2Ext.Normalize(ref moveDir);
            
            mover.Move(moveDir * Time.DeltaTime * moveSpeed, out collisionResult);
        }
        #endregion

        #region Idle

        void Idle_Enter()
        {

        }

        void Idle_Tick()
        {
            // do raycast toward player & see if we hit them or a wall first

            int flag = -1;
            Nez.Flags.SetFlagExclusive(ref flag, (int)Constants.PhysicsLayer.Player);
            Flags.SetFlag(ref flag, (int)Constants.PhysicsLayer.Wall);
            var ray = Nez.Physics.Linecast(Entity.Position, pathFinder.targetEntity.Position, flag);


            if (ray.Collider != null)
            {
                if (ray.Collider.Entity.Name == "player")
                {
                    CurrentState = ChaseState.Chase;
                }
            }
        }
        #endregion

    }
}
