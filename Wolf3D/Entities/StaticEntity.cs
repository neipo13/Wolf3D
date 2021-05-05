using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wolf3D.Components;
using Wolf3D.Util;
using Nez.Textures;

namespace Wolf3D.Entities
{
    public class StaticEntity : Entity
    {
        public WolfSprite sprite;
        public StaticEntity(PlayerState playerState, Sprite Sprite, bool hasCollider = false)
        {
            sprite = new WolfSprite(playerState);
            sprite.SetSprite(Sprite);
            this.AddComponent(sprite);
            if (hasCollider)
            {
                var collider = new BoxCollider(10f, 10f);
                AddComponent(collider);
            }
        }
    }
}
