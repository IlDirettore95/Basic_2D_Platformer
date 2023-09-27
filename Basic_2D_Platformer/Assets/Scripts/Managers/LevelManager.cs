using GMDG.Basic2DPlatformer.PCG;
using GMDG.Basic2DPlatformer.PCG.WFC;
using GMDG.Basic2DPlatformer.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static GMDG.NoProduct.Utility.Utility2D;

namespace GMDG.Basic2DPlatformer.System
{
    public class LevelManager : MonoBehaviour
    {
        public int Seed = -1;
        public int IterationLimit = -1;
        public float Timeout;
        public bool IsSimulated;
        public bool IsHardSimulated;

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
            // GameManager
            EventManager.Instance.Subscribe(Event.OnWelcome, Delete);
            EventManager.Instance.Subscribe(Event.OnVictory, Delete);

            // Gameplay
            EventManager.Instance.Subscribe(Event.OnGameplay, LoadFirstLevel);
            EventManager.Instance.Subscribe(Event.OnLevelCompleted, LoadNextLevel);
            EventManager.Instance.Subscribe(Event.OnPCGFailed, GenerateLevel);
        }

        private void OnDisable()
        {
            // GameManager
            EventManager.Instance.Unsubscribe(Event.OnWelcome, Delete);
            EventManager.Instance.Unsubscribe(Event.OnVictory, Delete);

            // Gameplay
            EventManager.Instance.Unsubscribe(Event.OnGameplay, LoadFirstLevel);
            EventManager.Instance.Unsubscribe(Event.OnLevelCompleted, LoadNextLevel);
            EventManager.Instance.Unsubscribe(Event.OnPCGFailed, GenerateLevel);
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

            GenerateLevel(null);
        }

        private void LoadNextLevel(object[] args)
        {
            _currentLevel += 1;
            if (_currentLevel < _numberOfLevels)
            {
                GenerateLevel(null);
            }
            else
            {
                EventManager.Instance.Publish(Event.OnAllLevelCompleted);
            }
        }

        private void Delete(object[] args)
        {
            Delete();
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

        private void GenerateLevel(object[] args)
        {
            Delete();

            bool randomSeed = Seed == -1;
            int currentSeed = -1;
            if (Seed == -1) currentSeed = Random.Range(1, int.MaxValue);
            else currentSeed = Seed;

            Random.InitState(currentSeed);

            PCGData data = _dataLoader.LoadData(_currentLevel, this);
            if (IsSimulated) StartCoroutine(new LevelGenerator().Generation?.Invoke(this, data, IterationLimit, Timeout, IsSimulated, IsHardSimulated, currentSeed));
            else new LevelGenerator().Generation?.Invoke(this, data, IterationLimit, Timeout, IsSimulated, IsHardSimulated, currentSeed).MoveNext();
        }
    }
}