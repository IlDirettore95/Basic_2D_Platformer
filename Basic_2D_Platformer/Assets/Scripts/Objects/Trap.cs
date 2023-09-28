using GMDG.Basic2DPlatformer.System;
using Event = GMDG.Basic2DPlatformer.System.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.Objects
{
    public class Trap : MonoBehaviour
    {
        private void OnTriggerStay2D(Collider2D collision)
        {
            Debug.Log("Trigger");
            if (collision.gameObject.tag.Equals("Player"))
            {
                EventManager.Instance.Publish(Event.OnTrapHit);
            }
        }
    }
}
