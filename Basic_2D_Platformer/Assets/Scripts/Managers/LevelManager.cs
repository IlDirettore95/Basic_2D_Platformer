using GMDG.Basic2DPlatformer.PCG;
using GMDG.Basic2DPlatformer.PCG.WFC;
using GMDG.Basic2DPlatformer.Utility;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static GMDG.NoProduct.Utility.Utility2D;

namespace GMDG.Basic2DPlatformer.System
{
    public class LevelManager : MonoBehaviour
    {
        public int IterationLimit;
        public float Timeout;
        public bool IsSimulated;

        private int _currentLevel;

        #region UnityMessages
        private void Awake()
        {
            _currentLevel = 0;

#if !UNITY_EDITOR
            IsSimulated = false;
#endif

            enabled = false;

            EventManager.Instance.Subscribe(Event.OnSystemsLoaded, Activate);
        }

        private void OnEnable()
        {
            EventManager.Instance.Subscribe(Event.OnGameplay, GenerateLevel);
        }

        private void OnDisable()
        {
            EventManager.Instance.Unsubscribe(Event.OnGameplay, GenerateLevel);
        }

        private void OnDestroy()
        {
            EventManager.Instance.Unsubscribe(Event.OnSystemsLoaded, Activate);
        }

#endregion

        #region Listeners

        private void Activate(object[] args)
        {
            enabled = true;
        }

        private void GenerateLevel(object[] args)
        {
            Delete();

            PCGData data = new PCGDataLoader().LoadData(_currentLevel);
            if (IsSimulated) StartCoroutine(new LevelGenerator().Generation?.Invoke(this, data, IterationLimit, Timeout, IsSimulated));
            else new LevelGenerator().Generation?.Invoke(this, data, IterationLimit, Timeout, IsSimulated).MoveNext();
        }

        #endregion

#if UNITY_EDITOR
        public void GenerateLevel()
        {
            GenerateLevel(null);
        }
#endif

        public void Delete()
        {
            for (int i = 0; i < gameObject.transform.childCount;)
            {
#if UNITY_EDITOR
                DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
#else
                Destroy(gameObject.transform.GetChild(i).gameObject);
#endif
            }
            StopAllCoroutines();
        }
    }
}