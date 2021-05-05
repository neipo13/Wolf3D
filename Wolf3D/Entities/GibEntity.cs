using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;
using Nez.Textures;
using Wolf3D.Util;
using Wolf3D.Components;

namespace Wolf3D.Entities
{
    public enum GibType
    {
        eyeball,
        solider_head
    }
    public class GibEntity : Entity
    {
        public GibEntity(PlayerState playerState, List<Sprite> Sprites, GibType type, Vector2 direction) : base("gib")
        {
            //animated wofl sprite -- needs Sprites
            var sprite = new AnimatedWolfSprite(playerState);
            //add all the anims
            var eyeAnim = new SpriteAnimation(Sprites.Take(15).ToArray());
            sprite.AddAnimation(GibType.eyeball.ToString(), eyeAnim);
            var soliderHeadAnim = new SpriteAnimation(Sprites.Skip(15).Take(7).ToArray());
            sprite.AddAnimation(GibType.solider_head.ToString(), soliderHeadAnim);
            //play the relevant one
            sprite.Play(type.ToString(), AnimatedWolfSprite.LoopMode.ClampForever);
            AddComponent(sprite);
            //set flash to transparent for fade out??? -- maybe later
            //collider for walls only (so it doesnt phase through them)
            var collider = new BoxCollider(10f, 10f);
            Nez.Flags.SetFlagExclusive(ref collider.PhysicsLayer, (int)Constants.PhysicsLayer.Gib);
            Nez.Flags.SetFlagExclusive(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Wall);
            AddComponent(collider);
            //mover
            var mover = new Mover();
            AddComponent(mover);
            //controller -- moves Entity in direction slowing over time, flips X based on angle to player
            var controller = new GibController(playerState, direction, 20f, 10f);
            AddComponent(controller);

        }
    }
}
