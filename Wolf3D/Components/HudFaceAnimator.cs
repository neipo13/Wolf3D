using Nez;
using Nez.Sprites;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Entities;

namespace Wolf3D.Components
{
    public class HudFaceAnimator : Component, IUpdatable
    {
        SpriteAnimator sprite;
        List<Sprite> Sprites;

        float timer = 0f;

        int currentFrame = 0;

        ShootableEntity shootableEntity;

        const int healthyMiddle = 0;
        const int healthyLeft = 1;
        const int healthyRight = 2;

        const int hurt = 3;

        const int tongueStart = 3;
        const int tongueEnd = 6;

        const float lookMiddleMax = 3f;
        const float lookMiddleMin = 1f;

        const float lookSideMax = 2f;
        const float lookSideMin = 0.5f;

        bool beingHurt = false;
        bool meleeAttacking = false;
        float beingHurtTimer = 0f;
        const float hurtFaceTime = 1f;

        const float fps = 10;
        const float secondPerFrame = 1f / fps;

        public float frameTimer = 0f;



        

        public HudFaceAnimator(List<Sprite> Sprites, SpriteAnimator sprite)
        {
            this.Sprites = Sprites;
            this.sprite = sprite;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            this.shootableEntity = Entity as ShootableEntity;
            timer = LookMiddle();
        }

        public void TakeDamage()
        {
            beingHurt = true;
            beingHurtTimer = hurtFaceTime;
            timer = -1f; //reset the timer so we adjust our 
            currentFrame = hurt;
        }

        public void MeleeAttack()
        {
            meleeAttacking = true;
            frameTimer = 0f;
            currentFrame = tongueStart;
        }


        public void Update()
        {
            if (beingHurt)
            {
                beingHurtTimer -= Time.DeltaTime;
                if(beingHurtTimer < 0f)
                {
                    beingHurt = false;
                }
            }
            else if (meleeAttacking)
            {
                frameTimer += Time.DeltaTime;
                if(frameTimer > secondPerFrame)
                {
                    frameTimer = 0f;
                    currentFrame++;
                    if(currentFrame > tongueEnd)
                    {
                        currentFrame = healthyMiddle;
                        meleeAttacking = false;
                    }
                }
            }
            else
            {
                if (shootableEntity.hp > 75)
                {
                    updateLook(healthyMiddle, healthyLeft, healthyRight);
                }
                else if (shootableEntity.hp > 30)
                {
                    //update when we have the frames
                    updateLook(healthyMiddle, healthyLeft, healthyRight);
                }
                else
                {
                    //update when we have the frames
                    updateLook(healthyMiddle, healthyLeft, healthyRight);
                }
            }
            sprite.Sprite = Sprites[currentFrame];
        }

        void updateLook(int middleFrame, int leftFrame, int rightFrame)
        {
            timer -= Time.DeltaTime;

            if(timer < 0f)
            {
                if(currentFrame == middleFrame)
                {
                    currentFrame = Nez.Random.Choose<int>(leftFrame, rightFrame);
                    timer = LookSide();
                }
                else
                {
                    currentFrame = middleFrame;
                    timer = LookMiddle();
                }
            }
        }

        float LookMiddle()
        {
            return Nez.Random.Range(lookMiddleMin, lookMiddleMax);
        }

        float LookSide()
        {
            return Nez.Random.Range(lookSideMin, lookSideMax);
        }
    }
}
