using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Constants;
using Wolf3D.Entities;
using Wolf3D.Util;

namespace Wolf3D.Components
{
    public enum SoldierState
    {
        Idle,
        Reposition,
        Shoot,
        TakeDamage,
        Wander
    }
    public class SoldierController : Nez.AI.FSM.SimpleStateMachine<SoldierState>
    {
        ChaseLogic pathFinder;
        Mover mover;
        CollisionResult collisionResult;

        AnimatedWolfSprite sprite;

        const float moveSpeed = 30f;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            mover = Entity.GetComponent<Mover>();
            pathFinder = Entity.GetComponent<ChaseLogic>();
            collisionResult = new CollisionResult();
            sprite = Entity.GetComponent<AnimatedWolfSprite>();
            InitialState = SoldierState.Idle;
        }
        public void AfterHit()
        {
            if (CurrentState == SoldierState.Wander)
                Wander_Enter();
            else
                CurrentState = SoldierState.Wander;
        }

        RaycastHit CastToPlayer()
        {
            int flag = -1;
            Nez.Flags.SetFlagExclusive(ref flag, (int)Constants.PhysicsLayer.Player);
            Flags.SetFlag(ref flag, (int)Constants.PhysicsLayer.Wall);
            return Nez.Physics.Linecast(Entity.Position, pathFinder.targetEntity.Position, flag);
        }

        #region Wander
        // magnitude of distance to go
        const float MaxWander = 15f;
        const float MinWander = 3f;
        float wanderDist = 0f;
        Vector2 wanderDirection;

        void Wander_Enter()
        {
            sprite.Play(SoldierSpriteState.Walk.ToString(), AnimatedWolfSprite.LoopMode.Loop);
            wanderDist = Nez.Random.Range(MinWander, MaxWander);
            wanderDirection = new Vector2(Nez.Random.Range(-1f, 1f), Nez.Random.Range(-1f, 1f));
        }
        void Wander_Tick()
        {
            var move = wanderDirection * moveSpeed * Time.DeltaTime;
            mover.Move(move, out collisionResult);
            wanderDist -= move.Length();
            if (wanderDist < 0f)
            {
                //check if we can see player
                var ray = CastToPlayer();
                var shoot = false;
                if (ray.Collider != null)
                {
                    if (ray.Collider.Entity.Name == "player")
                    {
                        shoot = true;
                    }
                }

                if (shoot)
                {
                    CurrentState = SoldierState.Shoot;
                }
                else
                {
                    CurrentState = SoldierState.Reposition;
                }
            }
        }
        #endregion

        #region Idle

        void Idle_Enter()
        {
            sprite.Play(SoldierSpriteState.Idle.ToString(), AnimatedWolfSprite.LoopMode.Loop);
        }

        void Idle_Tick()
        {
            // do raycast toward player & see if we hit them or a wall first
            var ray = CastToPlayer();



            if (ray.Collider != null)
            {
                if (ray.Collider.Entity.Name == "player")
                {
                    CurrentState = SoldierState.Wander;
                }
            }
        }
        #endregion

        #region Reposition
        const float SeenForLengthMax = 1f;
        const float SeenForLengthMin = 0.5f;
        float seenFor = 0f;
        void Reposition_Enter()
        {
            seenFor = Nez.Random.Range(SeenForLengthMin, SeenForLengthMax);
            sprite.Play(SoldierSpriteState.Walk.ToString(), AnimatedWolfSprite.LoopMode.Loop);
        }
        void Reposition_Tick()
        {
            //can we see the player?
            var ray = CastToPlayer();
            if (ray.Collider != null && ray.Collider.Entity.Name == "player")
            {
                //adding a lenght of time you must be "seen for" to remove the corner peeking the enemies were doing 
                seenFor -= Time.DeltaTime;
            }
            //otherwise keep moving toward player to get line of sight
            var target = pathFinder.NextTargetPoint();
            var moveDir = target - Entity.Position;
            Vector2Ext.Normalize(ref moveDir);
            mover.Move(moveDir * Time.DeltaTime * moveSpeed, out collisionResult);
            if (seenFor < 0f)
            {
                // if we see them then move to shoot phase
                CurrentState = SoldierState.Shoot;
            }

        }

        #endregion

        #region Shoot

        const float MaxShootWait = .75f;
        const float MinShootWait = .25f;
        float ShootWait = 0f;

        bool shot = false;
        const float afterShotWaitTime = .75f;

        void Shoot_Enter()
        {
            shot = false;
            //set anim state
            sprite.Play(SoldierSpriteState.Shoot.ToString(), AnimatedWolfSprite.LoopMode.Once);
            //get wait timer
            ShootWait = Nez.Random.Range(MinShootWait, MaxShootWait);
        }

        void Shoot_Tick()
        {
            if (ShootWait > 0f)
            {
                //if we havent finished our timer, just tick it down & bail out
                ShootWait -= Time.DeltaTime;
                return;
            }
            if (shot)
            {
                CurrentState = SoldierState.Wander;
                return;
            }

            //otherwise its time to actually fire da gun
            sprite.Play(SoldierSpriteState.Shoot.ToString(), AnimatedWolfSprite.LoopMode.Once);
            var ray = CastToPlayer();
            if (ray.Collider != null)
            {
                var baseDirection = pathFinder.targetEntity.Position - this.Entity.Position;
                Vector2Ext.Normalize(ref baseDirection);
                int physicsLayer = 0;
                Nez.Flags.SetFlagExclusive(ref physicsLayer, (int)PhysicsLayer.EnemyShot);
                int collidesWithLayers = 0;
                Nez.Flags.SetFlag(ref collidesWithLayers, (int)PhysicsLayer.Wall);
                Nez.Flags.SetFlag(ref collidesWithLayers, (int)PhysicsLayer.Player);

                var wolfRenderer = ((Scenes.WolfScene)(Entity.Scene)).wolfRenderer;
                var proj = new Projectile(this.Entity.Scene, ParticleType.RedFlash, baseDirection, physicsLayer, collidesWithLayers, sprite.playerState, 7, 100f);
                proj.Position = this.Entity.Position;
                wolfRenderer.AddWolfSprite(proj.GetComponent<WolfSprite>());
                Entity.Scene.AddEntity(proj);
            }
            shot = true;
            ShootWait = afterShotWaitTime;



        }
        #endregion

    }
}
