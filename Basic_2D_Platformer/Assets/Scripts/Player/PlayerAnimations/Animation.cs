using GMDG.Basic2DPlatformer.PlayerMovement;
using GMDG.Basic2DPlatformer.System;
using Event = GMDG.Basic2DPlatformer.System.Event;
using UnityEngine;


namespace GMDG.Basic2DPlatformer.PlayerAnimations
{
    public class Animation : MonoBehaviour
    {
        private Transform _transform;
        private float _orientation;

        private void Awake()
        {
            _transform = transform;
            _orientation = Mathf.Sign(_transform.localScale.x);
        }

        private void OnEnable()
        {
            EventManager.Instance.Subscribe(Event.OnPlayerWalking, ChangeOrientation);
        }

        private void OnDisable()
        {
            EventManager.Instance.Unsubscribe(Event.OnPlayerWalking, ChangeOrientation);
        }

        private void ChangeOrientation(object[] args)
        {
            Movement movement = (Movement)args[0];

            if (Mathf.Sign(movement.Sensors.HorizontalInput) == Mathf.Sign(_orientation)) return;

            _orientation = -_orientation;
            _transform.localScale = new Vector3(_orientation * 1f, 1f, 1f);
        }
    }
}
