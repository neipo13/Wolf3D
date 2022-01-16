using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Entities;
using Wolf3D.Util;

namespace Wolf3D.Components
{
    /// <summary>
    /// Basic starting idea for states:
    /// idle = raycast @ player every few frames & if we can see them then go into alert mode
    /// alert = play the alert anim & kind of jump in place or something to give player time to react - indicating a charge is coming
    /// charge = @ charge start pick the direction toward the player, then on charge just press forward until we collide with either the player or a wall, then fall back to wait
    /// wait = kill some random amt of time before deciding its time to go back to alert mode (if player raycast connects -- otherwise back to idle)
    /// </summary>
    public class ChargerController : Nez.AI.FSM.SimpleStateMachine<ChargerState>
    {
        Mover mover;
        CollisionResult collisionResult;
        Entity player;

        AnimatedWolfSprite sprite;

        const float moveSpeed = 100f;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            mover = Entity.GetComponent<Mover>();
            collisionResult = new CollisionResult();
            sprite = Entity.GetComponent<AnimatedWolfSprite>();
            player = sprite.playerState.Entity;
            InitialState = ChargerState.Idle;
        }
        
        RaycastHit CastToPlayer()
        {
            int flag = -1;
            Nez.Flags.SetFlagExclusive(ref flag, (int)Constants.PhysicsLayer.Player);
            Flags.SetFlag(ref flag, (int)Constants.PhysicsLayer.Wall);
            return Nez.Physics.Linecast(Entity.Position, player.Position, flag);
        }

        #region Idle
        const float checkAlertEverySeconds = 0.5f;
        float timeSinceLastAlertCheck;
        void Idle_Enter()
        {
            sprite.Play(ChargerState.Idle.ToString(), AnimatedWolfSprite.LoopMode.Loop);
            timeSinceLastAlertCheck = checkAlertEverySeconds + 1; //start with a check just so the wait triggers right away instead of adding checkEverySeconds on
        }
        void Idle_Tick()
        {
            timeSinceLastAlertCheck += Time.DeltaTime;
            if(timeSinceLastAlertCheck > checkAlertEverySeconds)
            {
                timeSinceLastAlertCheck = 0f;
                var castHit = CastToPlayer();
                if(castHit.Collider != null && castHit.Collider.Entity.Name == "player")
                {
                    CurrentState = ChargerState.Alert;
                }
            }
        }
        #endregion

        #region Alert
        const float alertWaitTime = 0.6f;
        float alertWaitTimer;
        void Alert_Enter()
        {
            sprite.Play(ChargerState.Alert.ToString(), AnimatedWolfSprite.LoopMode.Loop);
            alertWaitTimer = 0f;
            //sprite.flashing = true;
            //sprite.flashColor = Color.Red;
        }
        void Alert_Tick()
        {
            alertWaitTimer += Time.DeltaTime;
            if(alertWaitTimer > alertWaitTime)
            {
                //sprite.flashColor = Color.White;
                //sprite.flashing = false;
                CurrentState = ChargerState.Charge;
            }
        }
        #endregion

        #region Charge
        Vector2 chargeDirection;
        void Charge_Enter()
        {
            sprite.Play(ChargerState.Charge.ToString(), AnimatedWolfSprite.LoopMode.Loop);
            chargeDirection = player.Position - Entity.Position;
            Vector2Ext.Normalize(ref chargeDirection);
        }
        void Charge_Tick()
        {
            mover.Move(chargeDirection * moveSpeed * Time.DeltaTime, out collisionResult);

            if(collisionResult.Collider != null)
            {
                if(collisionResult.Collider.Entity.Name == "player")
                {
                    ShootableEntity e = collisionResult.Collider.Entity as ShootableEntity;
                    if (e != null)
                    {
                        e.Hit(12);
                    }
                    CurrentState = ChargerState.Wait;
                }
                else
                {
                    CurrentState = ChargerState.Wait;
                }
            }
        }
        #endregion

        #region Wait
        const float waitTimeMax = 2f;
        const float waitTimeMin = 1f;
        float waitTime;
        float waitTimer;
        void Wait_Enter()
        {
            sprite.Play(ChargerState.Idle.ToString(), AnimatedWolfSprite.LoopMode.Loop);
            waitTime = Nez.Random.Range(waitTimeMin, waitTimeMax);
            waitTimer = 0f;
        }
        void Wait_Tick()
        {
            waitTimer += Time.DeltaTime;
            if(waitTimer > waitTime)
            {
                CurrentState = ChargerState.Idle;//will handle flipping back to alert
            }
        }
        #endregion
    }
}
