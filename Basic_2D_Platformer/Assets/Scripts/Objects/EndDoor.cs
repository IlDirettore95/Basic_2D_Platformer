using GMDG.Basic2DPlatformer.System;
using UnityEngine;
using Event = GMDG.Basic2DPlatformer.System.Event;

namespace GMDG.Basic2DPlatformer.Objects
{
    public class EndDoor : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag.Equals("Player"))
            {
                EventManager.Instance.Publish(Event.OnLevelCompleted);
            }
        }
    }
}
