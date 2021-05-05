using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolf3D.Scenes;

namespace Wolf3D.Components
{
    public class LevelEndTrigger : Component, IUpdatable, ITriggerListener
    {
        ColliderTriggerHelper triggerHelper;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            triggerHelper = new ColliderTriggerHelper(Entity);
        }
        public void OnTriggerEnter(Collider other, Collider local)
        {
            //end the level
            WolfScene scene = Core.Scene as WolfScene;
            if(scene != null)
            {
                scene.EndLevel();
            }
        }

        public void OnTriggerExit(Collider other, Collider local) { }

        public void onTriggerStay(Collider other, Collider local) { }

        public void Update()
        {
            //update trigger listener
            triggerHelper.Update();
        }
    }
}
