using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolf3D.Components
{
    public class ChaseLogic : Component
    {
        Nez.AI.Pathfinding.AstarGridGraph graph;
        public Entity targetEntity;
        PlayerState targetState;
        PlayerState myState;

        public ChaseLogic(int[,] map, PlayerState EntityState)
        {
            //create astar map from the tilemap
            graph = new Nez.AI.Pathfinding.AstarGridGraph(map.GetLength(0), map.GetLength(1));
            for (var y = 0; y < map.GetLength(1); y++)
            {
                for (var x = 0; x < map.GetLength(0); x++)
                {
                    if (map[x, y] > 0 && map[x,y] < 100)
                        graph.Walls.Add(new Point(x, y));
                }
            }
            this.targetState = EntityState;
            this.targetEntity = targetState.Entity;

        }
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            this.myState = Entity.GetComponent<PlayerState>();
        }

        public Vector2 NextTargetPoint()
        {
            var path = graph.Search(myState.RayCasterPosition.ToPoint(), targetState.RayCasterPosition.ToPoint());
            var targetPoint = path.Skip(1).FirstOrDefault();
            if (targetPoint == null) return myState.RayCasterPosition;

            return (targetPoint.ToVector2() * 10f) + new Vector2(5f);
        }

        public override void DebugRender(Batcher batcher)
        {
            base.DebugRender(batcher);
            var path = graph.Search(myState.RayCasterPosition.ToPoint(), targetState.RayCasterPosition.ToPoint());
            if (path == null) return;
            for(int i = 0; i < path.Count - 1; i++)
            {
                batcher.DrawLine(path[i].ToVector2() * 10f + new Vector2(5f), path[i + 1].ToVector2() * 10f + new Vector2(5f), Color.Blue);
            }
        }
    }
}
