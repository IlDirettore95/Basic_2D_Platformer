using GMDG.Basic2DPlatformer.PCG;
using GMDG.Basic2DPlatformer.Utility;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.System
{
    public class DebugManager : MonoBehaviour
    {
        [SerializeField] private GameObject _fpsCounter;

        // Grid Debug
        private Grid<int> _grid;
        private GameObject _gridDebugGo;

        #region UnityMessages

        private void Awake()
        {
            enabled = false;
            _fpsCounter.SetActive(true);
            _gridDebugGo = new GameObject("Grid Debug");
            _gridDebugGo.SetActive(true);

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
            _grid = (Grid<int>)args[0];

            _grid?.DrawContent(_gridDebugGo, ColorHeuristic);
        }

        private Color ColorHeuristic(int cellType)
        {
            switch (cellType) 
            {
                case (int)LevelGenerator.CellType.None: return Color.white;
                case (int)LevelGenerator.CellType.Start: return Color.red;
                case (int)LevelGenerator.CellType.End: return Color.green;
                case (int)LevelGenerator.CellType.Passable: return Color.cyan;
                case (int)LevelGenerator.CellType.UnPassable: return Color.gray;
            }

            return Color.white;
        }

        #endregion
    }
}
