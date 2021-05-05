using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Weapons;

namespace Wolf3D.Util
{
    public class AmmoUpdatedEvent : EventArgs
    {
        public AmmoType type;
        public int value;
        public bool isNewPickup;

        public AmmoUpdatedEvent(AmmoType type, int value, bool isNewPickup)
        {
            this.type = type;
            this.value = value;
            this.isNewPickup = isNewPickup;
        }
    }
}
