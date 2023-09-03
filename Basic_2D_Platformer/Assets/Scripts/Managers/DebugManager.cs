using UnityEngine;

namespace GMDG.Basic2DPlatformer.System
{
    public class DebugManager : MonoBehaviour
    {
        [SerializeField] private GameObject _fpsCounter;

        #region UnityMessages
        private void Awake()
        {
            enabled = false;
            _fpsCounter.SetActive(false);

            EventManager.Instance.Subscribe(Event.OnSystemsLoaded, Activate);
        }

        private void OnDestroy()
        {
            EventManager.Instance.Unsubscribe(Event.OnSystemsLoaded, Activate);
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                _fpsCounter.SetActive(!_fpsCounter.activeSelf);
            }
        }

        #endregion

        #region Listeners

        private void Activate(object[] args)
        {
            enabled = true;
        }

        #endregion
    }
}
