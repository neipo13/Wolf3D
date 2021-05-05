using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolf3D.Data.Tiled
{

    public class TiledMap
    {
        public int compressionlevel { get; set; }
        public Editorsettings editorsettings { get; set; }
        public int height { get; set; }
        public bool infinite { get; set; }
        public TiledLayer[] layers { get; set; }
        public int nextlayerid { get; set; }
        public int nextobjectid { get; set; }
        public string orientation { get; set; }
        public string renderorder { get; set; }
        public string tiledversion { get; set; }
        public int tileheight { get; set; }
        public Tileset[] tilesets { get; set; }
        public int tilewidth { get; set; }
        public string type { get; set; }
        public float version { get; set; }
        public int width { get; set; }
        public Dictionary<string, TiledLayer> layerDictionary { get; set; }
        public void FillLayerDictionary()
        {
            layerDictionary = new Dictionary<string, TiledLayer>();
            for(int i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];
                layerDictionary.Add(layer.name, layer);
            }
        }

        /// <summary>
        /// Fix for tiled maping y inverse to how we do it in XNA
        /// </summary>
        public void FixAllObjectPositions()
        {
            int mapHeight = tileheight * height;
            for(int i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];
                if (layer.objects == null) continue;
                for(int j = 0; j < layer.objects.Length; j++)
                {
                    var obj = layer.objects[j];
                    obj.y = mapHeight - obj.y;
                }
            }
        }
    }

    public class Editorsettings
    {
        public Export export { get; set; }
    }

    public class Export
    {
        public string format { get; set; }
        public string target { get; set; }
    }

    public class TiledLayer
    {
        public int[] data { get; set; }
        public int height { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int opacity { get; set; }
        public string type { get; set; }
        public bool visible { get; set; }
        public int width { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public string draworder { get; set; }
        public TiledObject[] objects { get; set; }

        private int [,] _data2D { get; set; }
        public void FillData2D()
        {
            _data2D = new int[width, height];
            for (int i = 0; i < data.Length; i++)
            {
                var x = i % width;
                int y = height - 1  -  (i / width); // have to do this fix because Tiled inverts y vs what we do in XNA
                Console.WriteLine($"{x},{y} -- {data[i]}");
                _data2D[x, y] = data[i];
            }
        }
        public int[,] data2D => _data2D;
    }

    public class TiledObject
    {
        public int height { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int rotation { get; set; }
        public string type { get; set; }
        public bool visible { get; set; }
        public int width { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int gid { get; set; }
        public TiledObjectProperty[] properties { get; set; }
        public Dictionary<string, string> propertiesDictionary { get; set; }
        public void FillPropDictionary()
        {
            propertiesDictionary = new Dictionary<string, string>();
            for(int i= 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                propertiesDictionary.Add(prop.name, prop.value);
            }
        }

        public Vector2 position => new Vector2(x, y);
    }

    public class TiledObjectProperty
    {
        public string name { get; set; }
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Tileset
    {
        public int columns { get; set; }
        public int firstgid { get; set; }
        public string image { get; set; }
        public int imageheight { get; set; }
        public int imagewidth { get; set; }
        public int margin { get; set; }
        public string name { get; set; }
        public int spacing { get; set; }
        public int tilecount { get; set; }
        public int tileheight { get; set; }
        public int tilewidth { get; set; }
    }

}
