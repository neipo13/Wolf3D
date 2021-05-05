using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolf3D.Constants
{
    public enum PhysicsLayer
    {
        Player,
        Wall,
        SemiSolidWall,
        Enemy,
        PlayerShot,
        EnemyShot,
        Pickup,
        Gib,
        DoorTrigger
    }
}
