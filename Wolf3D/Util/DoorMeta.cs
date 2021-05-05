using Microsoft.Xna.Framework.Audio;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolf3D.Util
{
    public class DoorMeta : Component, IUpdatable, ITriggerListener
    {
        public int x { get; set; }
        public int y { get; set; }

        public float openRate { get; set; } = 5f;
        public float closedPct { get; set; } = 0f;
        public bool isOpen { get; set; }

        public float colliderOffAt = .3f;

        public BoxCollider wallCollider { get; set; }

        public List<Collider> collidersCurrentlyIn { get; set; }

        ColliderTriggerHelper triggerHelper;

        SoundEffect doorOpen;
        SoundEffect doorClose;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            triggerHelper = new ColliderTriggerHelper(this.Entity);
            collidersCurrentlyIn = new List<Collider>();

            doorOpen = Entity.Scene.Content.Load<SoundEffect>("sfx/door_open");
            doorClose = Entity.Scene.Content.Load<SoundEffect>("sfx/door_close");
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            collidersCurrentlyIn.AddIfNotPresent(other);
            //open door if not already open
            if (!isOpen)
            {
                doorOpen.Play(1.0f, 0f, 0f);
            }
            isOpen = true;
        }

        public void onTriggerStay(Collider other, Collider local) { }

        public void OnTriggerExit(Collider other, Collider local)
        {
            collidersCurrentlyIn.Remove(other);
            if (collidersCurrentlyIn.Count == 0)
            {
                //close door if no one is in the area
                isOpen = false;
                doorClose.Play(1.0f, 0f, 0f);
            }
        }

        public void Update()
        {
            triggerHelper.Update();

            if (isOpen && closedPct > 0.01f)
            {
                closedPct = Mathf.Approach(closedPct, 0f, openRate * Time.DeltaTime);
            }
            else if (!isOpen && closedPct < 0.99f)
            {
                closedPct = Mathf.Approach(closedPct, 1f, openRate * Time.DeltaTime);
            }

            if (wallCollider.Enabled && closedPct < colliderOffAt)
            {
                wallCollider.Enabled = false;
            }
            else if (!wallCollider.Enabled && closedPct > colliderOffAt)
            {
                wallCollider.Enabled = true;
            }
        }
    }
}
