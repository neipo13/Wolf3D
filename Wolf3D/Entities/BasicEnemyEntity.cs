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
    public class BasicEnemyEntity : ShootableEntity
    {
        public WolfSprite sprite;
        BoxCollider collider;
        public BasicEnemyEntity(PlayerState playerstate, Sprite Sprite, int[,] map)
        {
            collider = new BoxCollider(10f, 10f);
            Flags.SetFlagExclusive(ref collider.PhysicsLayer, (int)Constants.PhysicsLayer.Enemy);

            Flags.SetFlagExclusive(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Wall);
            Flags.SetFlag(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.PlayerShot);
            Flags.SetFlag(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Player);

            Console.WriteLine($"ENEMY: {Nez.Flags.BinaryStringRepresentation(collider.PhysicsLayer)}");

            sprite = new WolfSprite(playerstate);
            sprite.SetSprite(Sprite);

            var state = new PlayerState();

            var mover = new Mover();

            var pathfinder = new ChaseLogic(map, playerstate);

            var chaseController = new ChaseContoller();
            

            AddComponent(collider);
            AddComponent(sprite);
            AddComponent(mover);
            AddComponent(state);
            AddComponent(pathfinder);
            AddComponent(chaseController);

        }

        public override void OnDeath()
        {
            // for now just shut stuff off no need to delete it
            sprite.Enabled = false;
            collider.Enabled = false;
            this.Destroy();
        }
    }
}
