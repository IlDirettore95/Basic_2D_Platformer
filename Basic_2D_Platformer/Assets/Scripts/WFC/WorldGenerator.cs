using GMDG.NoProduct.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PCG.WFC
{
    public class WorldGenerator : MonoBehaviour
    {
        public Vector2Int GridSize;

        [SerializeField] private TileScriptableObject[] _tiles;

        private Utility2D.Grid2D _grid;
        private Dictionary<Vector2, List<TileScriptableObject>> _waves;

        #region UnityCallbacks

        private void Awake()
        {
            Initialize();
            Generate();
        }

        #endregion

        public void Initialize()
        {
            if (GridSize.x <= 0 || GridSize.y <= 0) 
            { 
                Debug.LogError("GridSize must be positive!"); 
                return; 
            }
            if (_tiles.GroupBy(tile => tile.ID).Any(group => group.Count() > 1)) 
            { 
                Debug.LogError("All tiles must have distinct IDs!"); 
                return; 
            }

            _grid = new Utility2D.Grid2D(GridSize.y, GridSize.x, Vector2.one, Vector2.zero);
            _waves = new Dictionary<Vector2, List<TileScriptableObject>>();

            for (int i = 0; i < _grid.YLength; i++)
            {
                for (int j = 0; j < _grid.XLength; j++)
                {
                    Vector2 worldPosition = _grid.CellsPositions[i, j];

                    _waves[worldPosition] = new List<TileScriptableObject>(_tiles);
                }
            }
        }

        public void Generate()
        {

        }

        public void Delete()
        {
            _waves = null;
        }

        #region EditorCallbacks

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            _grid?.Draw();
            if (_waves == null || _waves.Count == 0) return;
            foreach (Vector2 positon in _waves.Keys)
            {
                string stringList = string.Empty;

                for (int i = 0; i < _waves[positon].Count - 1; i++)
                {
                    stringList += _waves[positon][i].ID;
                    stringList += ", ";
                }
                stringList += _waves[positon][_waves[positon].Count - 1].ID;

                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.LowerRight;
                style.fontSize = 2;
                Handles.color = Color.green;
                Handles.Label(positon, stringList, style);
            }
        }
#endif

        #endregion
    }
}

