using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wolf3D.Scenes;
using Wolf3D.Weapons;
using Nez.Sprites;
using Microsoft.Xna.Framework.Audio;

namespace Wolf3D.Components
{
    public class PlayerController : Component, IUpdatable
    {
        PlayerState state;
        Mover mover;
        CollisionResult collisionResult;
        float rotateSpeed = 0.03f;
        float moveTest = 60f;
        int[,] worldMap;
        WolfScene scene;
        Renderers.WolfRenderer wolfRenderer;

        float mouseSensitivity = 0.005f;

        float bounceSpeed = 5f;
        float movementBounce = 0f;
        float bounceMax = 10f;

        const float mouseWheelDelay = 0.2f;
        float mouseWheelDelayTimer = 0f;

        IWeapon equippedWeapon => heldWeapons[equippedWeaponIndex];
        IWeapon[] heldWeapons = new IWeapon[5];
        Melee meleeWeapon;
        public int equippedWeaponIndex = 0;
        SpriteAnimator handSprite;
        TextComponent ammoText;

        public PlayerController(int[,] worldMap, TextComponent ammoText, Renderers.WolfRenderer wolfRenderer) : base()
        {
            this.worldMap = worldMap;
            this.ammoText = ammoText;
            this.wolfRenderer = wolfRenderer;
        }

        public bool EquipWeapon(AmmoType ammoType)
        {

            if (state.GetAmmo(ammoType) > -1)
            {
                equippedWeaponIndex = (int)ammoType;
                equippedWeapon.handsSprite.Play(equippedWeapon.idleHands, SpriteAnimator.LoopMode.ClampForever);
                ammoText.SetText(equippedWeapon.ammo.ToString());
                return true;
            }

            return false;
        }

        public void SetHandsSprite(SpriteAnimator sprite)
        {
            handSprite = sprite;
            handSprite.OnAnimationCompletedEvent += HandsSprite_onAnimationCompletedEvent;
        }

        private void HandsSprite_onAnimationCompletedEvent(string name)
        {
            if (name == Constants.HandStates.PISTOL_SHOOT)
            {
                handSprite.Play(Constants.HandStates.PISTOL_IDLE, SpriteAnimator.LoopMode.ClampForever);
            }
            else if(name == Constants.HandStates.MELEE_SHOOT)
            {
                handSprite.Play(equippedWeapon.idleHands, SpriteAnimator.LoopMode.ClampForever);
            }
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            this.state = this.Entity.GetComponent<PlayerState>();
            this.mover = this.Entity.GetComponent<Mover>();
            collisionResult = new CollisionResult();
            this.scene = this.Entity.Scene as WolfScene;
            var pistolSound = Entity.Scene.Content.Load<SoundEffect>("sfx/blaster_sound");
            heldWeapons[0] = new Pistol(handSprite, state, ammoText, pistolSound);
            var chainGunSound = Entity.Scene.Content.Load<SoundEffect>("sfx/cg1");
            heldWeapons[2] = new Chaingun(handSprite, state, ammoText, chainGunSound);
            var shottySound = Entity.Scene.Content.Load<SoundEffect>("sfx/shotgun");
            heldWeapons[1] = new Shotgun(handSprite, state, ammoText, shottySound);
            meleeWeapon = new Melee(handSprite, state, ammoText, pistolSound);
            heldWeapons[3] = new GrenadeLauncher(handSprite, state, ammoText, chainGunSound);
            heldWeapons[4] = new Toaster(handSprite, state, ammoText, shottySound);

            EquipWeapon((AmmoType)equippedWeaponIndex);

            state.onAmmoUpdated += onAmmoUpdated;
        }

        public override void OnRemovedFromEntity()
        {
            state.onAmmoUpdated -= onAmmoUpdated;
            base.OnRemovedFromEntity();
        }

        private void onAmmoUpdated(object sender, Util.AmmoUpdatedEvent e)
        {
            //if its a new pickup, change to that weapon
            if (e.isNewPickup)
            {
                var weaponIndex = Array.FindIndex(heldWeapons, (w) => w.ammoType == e.type);
                equippedWeaponIndex = weaponIndex;
                equippedWeapon.handsSprite.Play(equippedWeapon.idleHands, SpriteAnimator.LoopMode.ClampForever);
                ammoText.SetText(equippedWeapon.ammo.ToString());
            }
            else if (equippedWeapon.ammoType == e.type)
            {
                ammoText.SetText(equippedWeapon.ammo.ToString());
            }
        }

        public void Update()
        {
            var mouseDelta = -Nez.Input.MousePositionDelta.X * mouseSensitivity * NezGame.gameSettings.mouseSenseMultiplier;
            if (Nez.Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
            {
                state.rotate(rotateSpeed);
            }
            else if (Nez.Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
            {
                state.rotate(-rotateSpeed);
            }
            else
            {
                state.rotate(mouseDelta);
            }

            var move = new Vector2();
            if (Nez.Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
            {
                //float xMove = 0f;
                //float yMove = 0f;
                //if (worldMap[(int)(state.RayCasterPosition.X + (state.Direction.X * moveSpeed)), (int)(state.RayCasterPosition.Y)] == 0) xMove = state.Direction.X * moveSpeed;
                //if (worldMap[(int)(state.RayCasterPosition.X), (int)(state.RayCasterPosition.Y + (state.Direction.Y * moveSpeed))] == 0) yMove = state.Direction.Y * moveSpeed;
                //this.Entity.Position += new Vector2(xMove, yMove);
                move.X += state.Direction.X;
                move.Y += state.Direction.Y;
                movementBounce += bounceSpeed * Time.DeltaTime;

            }
            else if (Nez.Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
            {
                //float xMove = 0f;
                //float yMove = 0f;
                //if (worldMap[(int)(state.RayCasterPosition.X + (state.Direction.X * -moveSpeed)), (int)(state.RayCasterPosition.Y)] == 0) xMove = state.Direction.X * -moveSpeed;
                //if (worldMap[(int)(state.RayCasterPosition.X), (int)(state.RayCasterPosition.Y + (state.Direction.Y * -moveSpeed))] == 0) yMove = state.Direction.Y * -moveSpeed;
                //this.Entity.Position += new Vector2(xMove, yMove);
                move.X += -state.Direction.X;
                move.Y += -state.Direction.Y;
                movementBounce += bounceSpeed * Time.DeltaTime;
            }

            if (Nez.Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
            {
                //float xMove = 0f;
                //float yMove = 0f;
                //if (worldMap[(int)(state.RayCasterPosition.X + (state.Plane.X * moveSpeed)), (int)(state.RayCasterPosition.Y)] == 0) xMove = state.Plane.X * moveSpeed;
                //if (worldMap[(int)(state.RayCasterPosition.X), (int)(state.RayCasterPosition.Y + (state.Plane.Y * moveSpeed))] == 0) yMove = state.Plane.Y * moveSpeed;
                //this.Entity.Position += new Vector2(xMove, yMove);
                //mover.Move(new Vector2(xMove, yMove), out collisionResult);
                move.X += state.Plane.X;
                move.Y += state.Plane.Y;
                movementBounce += bounceSpeed * Time.DeltaTime;
            }
            else if (Nez.Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
            {
                //float xMove = 0f;
                //float yMove = 0f;
                //if (worldMap[(int)(state.RayCasterPosition.X + (state.Plane.X * -moveSpeed)), (int)(state.RayCasterPosition.Y)] == 0) xMove = state.Plane.X * -moveSpeed;
                //if (worldMap[(int)(state.RayCasterPosition.X), (int)(state.RayCasterPosition.Y + (state.Plane.Y * -moveSpeed))] == 0) yMove = state.Plane.Y * -moveSpeed;
                //this.Entity.Position += new Vector2(xMove, yMove);
                move.X += -state.Plane.X;
                move.Y += -state.Plane.Y;
                movementBounce += bounceSpeed * Time.DeltaTime;
            }
            if (move == Vector2.Zero)
            {
                float approach = 0f;
                if (movementBounce > Mathf.PI) approach = 2f * Mathf.PI;
                movementBounce = Mathf.Approach(movementBounce, approach, moveTest * 2f * Time.DeltaTime);
            }
            wolfRenderer.moveBounceOffset = Mathf.Sin(movementBounce) * bounceMax;
            handSprite.LocalOffset = new Vector2(Mathf.Sin(movementBounce * 0.5f) * bounceMax, handSprite.LocalOffset.Y);
            //Console.WriteLine($"moveBounce:{movementBounce} offset:{wolfRenderer.MoveBounceOffset} ");
            if (movementBounce > 2f * Mathf.PI) movementBounce -= 2f * Mathf.PI;
            Vector2Ext.Normalize(ref move);
            move *= Time.DeltaTime * moveTest;

            mover.Move(move, out collisionResult);

            if (Nez.Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space) || Nez.Input.LeftMouseButtonDown)
            {
                // test raycast for hits on IShootables
                equippedWeapon.Shoot();
            }
            else if (Nez.Input.RightMouseButtonPressed)
            {
                meleeWeapon.Shoot();
            }
            else if (Nez.Input.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.Space) || Nez.Input.LeftMouseButtonReleased)
            {
                equippedWeapon.ShotReleased();
            }
            //pistol
            if (Nez.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.D1))
            {
                EquipWeapon(AmmoType.Pistol);
            }
            //shotty
            else if (Nez.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.D2))
            {
                EquipWeapon(AmmoType.Shotgun);
            }
            //chaingun
            else if (Nez.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.D3))
            {
                EquipWeapon(AmmoType.MachineGun);
            }
            //nade launcher
            else if (Nez.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.D4))
            {
                EquipWeapon(AmmoType.Explosive);
            }
            //toaster
            else if (Nez.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.D5))
            {
                EquipWeapon(AmmoType.Disc);
            }

            if (mouseWheelDelayTimer > 0f) mouseWheelDelayTimer -= Time.DeltaTime;
            if (Nez.Input.MouseWheelDelta > 0f && mouseWheelDelayTimer <= 0f)
            {
                var equipped = false;
                while (!equipped)
                {
                    mouseWheelDelayTimer = mouseWheelDelay;
                    equippedWeaponIndex++;
                    if (equippedWeaponIndex > heldWeapons.Length - 1)
                    {
                        equippedWeaponIndex = 0;
                    }
                    equipped = EquipWeapon((AmmoType)equippedWeaponIndex);
                }
            }
            else if (Nez.Input.MouseWheelDelta < 0f && mouseWheelDelayTimer <= 0f)
            {
                var equipped = false;
                while (!equipped)
                {
                    mouseWheelDelayTimer = mouseWheelDelay;
                    equippedWeaponIndex--;
                    if (equippedWeaponIndex < 0)
                    {
                        equippedWeaponIndex = heldWeapons.Length - 1;
                    }
                    equipped = EquipWeapon((AmmoType)equippedWeaponIndex);
                }
            }
        }
    }
}
