using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Components;
using Wolf3D.Entities;

namespace Wolf3D.Util
{
    public static class HitEffectPool
    {
        private static Queue<HitEffect> _objectQueue = new Queue<HitEffect>(10);


        /// <summary>
        /// warms up the cache filling it with a max of cacheCount objects
        /// </summary>
        /// <param name="cacheCount">new cache count</param>
        public static void warmCache(int cacheCount, Scene scene, PlayerState state)
        {
            cacheCount -= _objectQueue.Count;
            if (cacheCount > 0)
            {
                for (var i = 0; i < cacheCount; i++)
                {
                    var obj = new HitEffect(scene, state);
                    scene.AddEntity(obj);
                    _objectQueue.Enqueue(obj);

                }
            }
        }


        /// <summary>
        /// trims the cache down to cacheCount items
        /// </summary>
        /// <param name="cacheCount">Cache count.</param>
        public static void trimCache(int cacheCount)
        {
            while (cacheCount > _objectQueue.Count)
                _objectQueue.Dequeue();
        }


        /// <summary>
        /// clears out the cache
        /// </summary>
        public static void clearCache()
        {
            _objectQueue.Clear();
        }


        /// <summary>
        /// pops an item off the stack if available creating a new item as necessary
        /// </summary>
        public static HitEffect obtain(Scene scene, PlayerState state)
        {
            if (_objectQueue.Count > 0)
                return _objectQueue.Dequeue();

            var obj = new HitEffect(scene, state);
            return obj;
        }


        /// <summary>
        /// pushes an item back on the stack
        /// </summary>
        /// <param name="obj">Object.</param>
        public static void free(HitEffect obj)
        {
            _objectQueue.Enqueue(obj);

            if (obj is IPoolable)
                ((IPoolable)obj).Reset();
        }
    }
}
