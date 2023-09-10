using GMDG.Basic2DPlatformer.PCG;
using GMDG.Basic2DPlatformer.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.System
{
    public class DebugManager : MonoBehaviour
    {
        [SerializeField] private GameObject _fpsCounter;

        // Grid Debug
        private Grid<HashSet<int>> _grid;
        private GameObject _gridDebugGo;

        #region UnityMessages

        private void Awake()
        {
            enabled = false;
            _fpsCounter.SetActive(false);
            _gridDebugGo = GameObject.Find("Grid Debug");
            _gridDebugGo.SetActive(false);

            EventManager.Instance.Subscribe(Event.OnSystemsLoaded, Activate);
            EventManager.Instance.Subscribe(Event.OnGridUpdated, UpdateGrid);
        }

        private void OnDestroy()
        {
            EventManager.Instance.Unsubscribe(Event.OnSystemsLoaded, Activate);
            EventManager.Instance.Unsubscribe(Event.OnGridUpdated, UpdateGrid);
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                _fpsCounter.SetActive(!_fpsCounter.activeSelf);
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                _gridDebugGo.SetActive(!_gridDebugGo.activeSelf);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_gridDebugGo == null)
            {
                _gridDebugGo = GameObject.Find("Grid Debug");
            }
            if (_gridDebugGo.activeSelf) _grid?.Draw();
        }
#endif

        #endregion

        #region Listeners

        private void Activate(object[] args)
        {
            enabled = true;
        }

        private void UpdateGrid(object[] args)
        {
            _grid = (Grid<HashSet<int>>)args[0];
#if UNITY_EDITOR
            _grid?.DrawContent(_gridDebugGo, 20, ColorHeuristic, StringHeuristic);
#endif
        }

        private Color ColorHeuristic(HashSet<int> superPositions)
        {
            if (superPositions.Contains(PCGData.START_CELL))
            {
                return Color.red;
            }
            else if (superPositions.Contains(PCGData.END_CELL)) 
            {
                return Color.green;
            }
            else
            {
                return Color.white;
            }
        }

        private string StringHeuristic(HashSet<int> superPositions)
        {
            string text = string.Empty;

            foreach (int pos in superPositions) 
            { 
                text += pos.ToString() + "-";
            }

            text = text.Remove(text.Length - 1);

            return text;
        }

        #endregion
    }
}
