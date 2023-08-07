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
        public Vector2Int CellSize;
        public bool IsGenerationSimulated = false;
        public float timeup = 0;

        [SerializeField] private TileScriptableObject[] _tiles;

        private Utility2D.Grid2D _grid;
        private Dictionary<Vector2, List<TileScriptableObject>> _waves;

        #region UnityCallbacks

        private void Awake()
        {
            Delete();
            Initialize();
            if (IsGenerationSimulated)
            {
                GenerateSimulated(timeup);
            }
            else
            {
                Generate();
            }
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

            _grid = new Utility2D.Grid2D(GridSize, CellSize, Vector2.zero);
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
            int numberOfWavesToCollapse = _waves.Count;

            while (numberOfWavesToCollapse > 0) 
            {
                // Observation
                // 1. Find element
                float positionEntropy = FindWavesWithLowestEntropy(out Vector2 chosenPosition);
                if (positionEntropy <= 0 || positionEntropy == float.PositiveInfinity)
                {
                    Debug.Log("FALLITO!");
                    break;
                }

                // 2. Collapse
                List<TileScriptableObject> superPositions = _waves[chosenPosition];
                TileScriptableObject chosenTile = CollapseWave(chosenPosition);

                Instantiate(chosenTile.Prefab, chosenPosition, Quaternion.identity, gameObject.transform);

                //Propagation
                PropagateConstraints(chosenPosition, chosenTile.PossibleNeighbours);

                numberOfWavesToCollapse--;
            }

        }

        public void GenerateSimulated(float timeup)
        {
            StartCoroutine(GenerateCoroutine(timeup));
        }

        public void Delete()
        {
            _grid = null;
            _waves = null;

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

        private IEnumerator GenerateCoroutine(float timeup)
        {
            int numberOfWavesToCollapse = _waves.Count;

            while (numberOfWavesToCollapse > 0)
            {
                // Observation
                // 1. Find element
                float positionEntropy = FindWavesWithLowestEntropy(out Vector2 chosenPosition);
                if (positionEntropy <= 0 || positionEntropy == float.PositiveInfinity) break;

                // 2. Collapse
                List<TileScriptableObject> superPositions = _waves[chosenPosition];
                TileScriptableObject chosenTile = CollapseWave(chosenPosition);

                Instantiate(chosenTile.Prefab, chosenPosition, Quaternion.identity, gameObject.transform);

                //Propagation
                PropagateConstraints(chosenPosition, chosenTile.PossibleNeighbours);

                numberOfWavesToCollapse--;
                if (timeup == 0) yield return null;
                yield return new WaitForSecondsRealtime(timeup);
            }
        }

        private TileScriptableObject CollapseWave(Vector2 chosenPosition)
        {
            List<TileScriptableObject> superPositions = _waves[chosenPosition];
            TileScriptableObject collapsedWave = null;

            int totalWeight = 0;
            for (int i = 0; i < superPositions.Count; i++) 
            {
                totalWeight += superPositions[i].Weight;
            }

            int randomChoice = UnityEngine.Random.Range(1, totalWeight + 1);

            for (int i = 0; i < superPositions.Count; i++)
            {
                randomChoice -= superPositions[i].Weight;

                if (randomChoice > 0) continue;

                collapsedWave = superPositions[i];
                _waves.Remove(chosenPosition);
                break;
            }

            return collapsedWave;
        }

        private void PropagateConstraints(Vector2 chosenPosition, List<TileScriptableObject> possibleNeighbours)
        {
            PropagateConstraint(chosenPosition, Utility2D.Direction2D.NORTH, possibleNeighbours);
            PropagateConstraint(chosenPosition, Utility2D.Direction2D.SOUTH, possibleNeighbours);
            PropagateConstraint(chosenPosition, Utility2D.Direction2D.EAST, possibleNeighbours);
            PropagateConstraint(chosenPosition, Utility2D.Direction2D.WEST, possibleNeighbours);
        }

        private void PropagateConstraint(Vector2 chosenPosition, Utility2D.Direction2D direction, List<TileScriptableObject> possibleNeighbours)
        {
            Vector2 tileInDirection;
            List<TileScriptableObject> neighbours;
            tileInDirection = chosenPosition + Utility2D.GridDirections2D[direction] * CellSize;
            if (!_waves.TryGetValue(tileInDirection, out neighbours)) return;
            for (int i = 0; i < neighbours.Count; i++)
            {
                if (possibleNeighbours.Contains(neighbours[i])) continue;

                neighbours.RemoveAt(i);
                i--;
            }
        }

        private float FindWavesWithLowestEntropy(out Vector2 chosenPosition)
        {
            float minEntropy = float.PositiveInfinity;
            chosenPosition = Vector2.zero;
            List<Vector2> candidatesWaves = new List<Vector2>();

            foreach (Vector2 position in _waves.Keys) 
            { 
                float positionEntropy = Entropy(position);

                if (positionEntropy < minEntropy)
                {
                    minEntropy = positionEntropy;
                    candidatesWaves.Clear();
                    candidatesWaves.Add(position);
                }
                else if (positionEntropy == minEntropy)
                {
                    candidatesWaves.Add(position);
                }
            }

            if(candidatesWaves.Count > 0) 
            {
                chosenPosition = candidatesWaves[UnityEngine.Random.Range(0, candidatesWaves.Count)];
            }
            
            return minEntropy;
        }

        private float Entropy(Vector2 position)
        {
            return _waves[position].Count;
        }

        #region EditorCallbacks

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            _grid?.Draw();
            if (_waves == null || _waves.Count == 0) return;
            foreach (Vector2 position in _waves.Keys)
            {
                string stringList = string.Empty;

                for (int i = 0; i < _waves[position].Count; i++)
                {
                    stringList += _waves[position][i].ID;
                    stringList += ", ";
                }
                stringList += string.Format("({0})", Entropy(position));
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.UpperLeft;
                style.normal.textColor = Color.green;
                style.fontSize = 8;
                Handles.Label(position, stringList, style);
            }
        }
#endif

        #endregion
    }
}

