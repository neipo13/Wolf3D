using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Components;
using Wolf3D.Util;

namespace Wolf3D.Entities
{
    public enum SceneryType
    {
        barrel = 5
    }
    public class SceneryEntity : ShootableEntity
    {
        bool shootable = false;
        bool alive = true;
        SceneryType type;
        PlayerState state;

        public SceneryEntity(SceneryType type, Texture2D wallTexture, PlayerState playerState)
        {
            this.type = type;
            this.state = playerState;
            var wallSprites = Sprite.SpritesFromAtlas(wallTexture, 32, 32).ToArray();
            var sprite = new WolfSprite(playerState);
            sprite.SetSprite(wallSprites[(int)type]);
            this.AddComponent(sprite);
            if (type == SceneryType.barrel)
            {
                sprite.ShowClose = true;
                this.shootable = true; // must have collider to be shootable
                var collider = new BoxCollider(10f, 10f);
                collider.PhysicsLayer = -1;
                AddComponent(collider);
            }


        }

        public override void OnDeath()
        {
            //how tf did you get here if not shootable but will put here just incase
            if (shootable && alive)
            {
                alive = false;
                if(this.type == SceneryType.barrel)
                {
                    int physicsLayer = -1;
                    Nez.Flags.SetFlagExclusive(ref physicsLayer, (int)Constants.PhysicsLayer.PlayerShot);
                    Nez.Flags.SetFlag(ref physicsLayer, (int)Constants.PhysicsLayer.EnemyShot);
                    var explosion = new Explosion(Scene, Position, physicsLayer, state, 50, 10f);
                    explosion.Position = this.Position;
                    Scene.AddEntity(explosion);
                }
                this.state = null;
                this.Destroy();
            }
        }
    }
}
