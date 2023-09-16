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

        private PCGDataLoader _dataLoader = new PCGDataLoader();
        private int _currentLevel;
        private int _numberOfLevels;

        #region UnityMessages
        private void Awake()
        {
            _currentLevel = 0;
            _numberOfLevels = _dataLoader.GetNumberOfLevels();

#if !UNITY_EDITOR
            IsSimulated = false;
#endif

            enabled = false;

            EventManager.Instance.Subscribe(Event.OnSystemsLoaded, Activate);
        }

        private void OnEnable()
        {
            EventManager.Instance.Subscribe(Event.OnGameplay, LoadFirstLevel);
            EventManager.Instance.Subscribe(Event.OnLevelCompleted, LoadNextLevel);
        }

        private void OnDisable()
        {
            EventManager.Instance.Unsubscribe(Event.OnGameplay, LoadFirstLevel);
            EventManager.Instance.Unsubscribe(Event.OnLevelCompleted, LoadNextLevel);
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

        private void LoadFirstLevel(object[] args)
        {
            _currentLevel = 0;

            GenerateLevel();
        }

        private void LoadNextLevel(object[] args)
        {
            _currentLevel += 1;
            if (_currentLevel < _numberOfLevels) LoadFirstLevel(null);
            else
            {
                Delete();
                EventManager.Instance.Publish(Event.OnEndVictoryTrasition);
            }
        }

        #endregion

#if UNITY_EDITOR
        public void LoadFirstLevel()
        {
            LoadFirstLevel(null);
        }
#endif

        public void Delete()
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    Destroy(gameObject.transform.GetChild(i).gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
                    i--;
                }
#else
                Destroy(gameObject.transform.GetChild(i).gameObject);
#endif
            }
            StopAllCoroutines();
        }

        private void GenerateLevel()
        {
            Delete();

            PCGData data = _dataLoader.LoadData(_currentLevel);
            if (IsSimulated) StartCoroutine(new LevelGenerator().Generation?.Invoke(this, data, IterationLimit, Timeout, IsSimulated));
            else new LevelGenerator().Generation?.Invoke(this, data, IterationLimit, Timeout, IsSimulated).MoveNext();
        }
    }
}