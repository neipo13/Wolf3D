using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Components;
using Wolf3D.Weapons;

namespace Wolf3D.Data
{
    // only to be set at end of level & loaded at start of level
    public class PlayerInfo
    {
        public AmmoType equippedWeapon { get; set; }
        // holds info about equipped weapons & ammo counts
        public Dictionary<AmmoType, int> Ammo = new Dictionary<AmmoType, int>();
        //hp setting
        public int Heath { get; set; }
        public string FirstLevel { get; set; }

        public PlayerInfo(string levelName)
        {
            FirstLevel = levelName;
            //set the default starting stuff if you loaded in directly on that level
            switch (levelName)
            {
                case "tiled-test-2":
                    Heath = 100;
                    Ammo = new Dictionary<AmmoType, int>();
                    Ammo.Add(AmmoType.Pistol, 40);
                    Ammo.Add(AmmoType.Shotgun, 15);
                    equippedWeapon = AmmoType.Pistol;
                    break;
                default:
                    Heath = 5;
                    Ammo = new Dictionary<AmmoType, int>();
                    Ammo.Add(AmmoType.Pistol, 40);
                    equippedWeapon = AmmoType.Pistol;
                    break;
            }
        }
    }
}
