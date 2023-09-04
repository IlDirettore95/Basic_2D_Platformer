using GMDG.NoProduct.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PCG.WFC
{
    public class WorldGeneratorWFC : MonoBehaviour
    {
        public Vector2Int GridSize;
        public Vector2Int CellSize;
        public int MaxIterations = 0;
        public int Seed = 0;
        public bool IsGenerationSimulated = false;
        public float Timeup = 0;

        [SerializeField] private TileScriptableObject[] _possibleTiles;

        private Utility2D.Grid2D<int> _grid;
        private Dictionary<Vector2, List<TileScriptableObject>> _waves;
        private Dictionary<Vector2, TileScriptableObject> _collapsedPositions;
        private List<Utility2D.Direction2D> _directions;

        #region UnityCallbacks

        private void Awake()
        {
            Delete();
            Initialize();
            Generate(Timeup);
        }

        #endregion

        public void Initialize()
        {
            if (GridSize.x <= 0 || GridSize.y <= 0) 
            { 
                Debug.LogError("GridSize must be positive!"); 
                return; 
            }

            UnityEngine.Random.InitState(Seed <= 0 ? UnityEngine.Random.Range(0, int.MaxValue) : Seed);

            _grid = new Utility2D.Grid2D<int>(GridSize, CellSize, Vector2.zero);
            _waves = new Dictionary<Vector2, List<TileScriptableObject>>();
            _collapsedPositions = new Dictionary<Vector2, TileScriptableObject>();
            _directions = new List<Utility2D.Direction2D>()
            { 
                Utility2D.Direction2D.NORTH, 
                Utility2D.Direction2D.EAST, 
                Utility2D.Direction2D.SOUTH, 
                Utility2D.Direction2D.WEST
            };

            //for (int i = 0; i < _grid.YLength; i++)
            //{
            //    for (int j = 0; j < _grid.XLength; j++)
            //    {
            //        Vector2 worldPosition = _grid.CellsPositions[i, j];
            //        _waves[worldPosition] = new List<TileScriptableObject>(_possibleTiles);
            //    }
            //}
        }

        public void Generate(float timeup)
        {
            StartCoroutine(GenerateCoroutine(timeup));
        }

        public void Delete()
        {
            _grid = null;
            _waves = null;
            //_visitedPositionsByPropagation = null;
            _collapsedPositions = null;
            _directions = null;

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
            int numberOfWavesToCollapse = MaxIterations <= 0 ? _waves.Count : MaxIterations;

            while (numberOfWavesToCollapse > 0)
            {
                // Observation
                // 1. Find element
                float positionEntropy = FindWavesWithLowestEntropy(out Vector2 chosenPosition);
                if (positionEntropy <= 0 || positionEntropy == float.PositiveInfinity) break;

                // 2. Collapse
                TileScriptableObject chosenTile = CollapseWave(chosenPosition);
                Instantiate(chosenTile.Prefab, chosenPosition, Quaternion.identity, gameObject.transform);

                //Propagation
                PropagateContraints(chosenPosition);

                numberOfWavesToCollapse--;
                if (!IsGenerationSimulated) continue;
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
                break;
            }

            _waves.Remove(chosenPosition);
            _collapsedPositions.Add(chosenPosition, collapsedWave);

            return collapsedWave;
        }

        private void PropagateContraints(Vector2 chosenPosition)
        {
            List<TileScriptableObject> superPositions;
            
            if (_waves.ContainsKey(chosenPosition))
            {
                superPositions = _waves[chosenPosition];
            }
            else
            {
                superPositions = new List<TileScriptableObject>
                {
                    _collapsedPositions[chosenPosition]
                };
            }

            for (int i = 0; i < _directions.Count; i++)
            {
                Vector2Int directionInGrid = Utility2D.GridDirections2D[_directions[i]];
                Vector2 positionInDirection = chosenPosition + new Vector2Int(directionInGrid.y, directionInGrid.x) * CellSize;
                if (CantPropagate(positionInDirection, out List<TileScriptableObject> neighbourSuperPositions)) continue;
                
                List<TileScriptableObject> possibleNeighbourSuperpositions = new List<TileScriptableObject>();
                for (int j = 0; j < superPositions.Count; j++)
                {
                    //possibleNeighbourSuperpositions = new List<TileScriptableObject>(possibleNeighbourSuperpositions.Union(superPositions[j].Constraints[i].PossibleNeighbours));
                }

                int numberOfNeighbourSuperpositions = neighbourSuperPositions.Count;
                for (int j = 0; j < neighbourSuperPositions.Count; j++)
                {
                    if (possibleNeighbourSuperpositions.Contains(neighbourSuperPositions[j])) continue;
                    neighbourSuperPositions.RemoveAt(j);
                    j--;
                }

                if (neighbourSuperPositions.Count == numberOfNeighbourSuperpositions) continue;
                PropagateContraints(positionInDirection);
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

        private bool CantPropagate(Vector2 position, out List<TileScriptableObject> neighbourSuperPositions)
        {
            neighbourSuperPositions = null;
            return !_waves.TryGetValue(position, out neighbourSuperPositions);
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
                    stringList += _waves[position][i].name;
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