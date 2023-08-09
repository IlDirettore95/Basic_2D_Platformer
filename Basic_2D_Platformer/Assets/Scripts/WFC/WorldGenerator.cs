using GMDG.NoProduct.Utility;
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
        public int MaxIterations = 0;
        public int Seed = 0;
        public bool IsGenerationSimulated = false;
        public float Timeup = 0;

        [SerializeField] private TileScriptableObject[] _tiles;

        private Utility2D.Grid2D _grid;
        private Dictionary<Vector2, List<TileScriptableObject>> _waves;
        private List<Utility2D.Direction2D> _directions;

        #region UnityCallbacks

        private void Awake()
        {
            Delete();
            Initialize();
            if (IsGenerationSimulated)
            {
                GenerateSimulated(Timeup);
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

            UnityEngine.Random.InitState(Seed <= 0 ? UnityEngine.Random.Range(0, int.MaxValue) : Seed);

            _grid = new Utility2D.Grid2D(GridSize, CellSize, Vector2.zero);
            _waves = new Dictionary<Vector2, List<TileScriptableObject>>();
            _directions = new List<Utility2D.Direction2D>()
            { 
                Utility2D.Direction2D.NORTH, 
                Utility2D.Direction2D.EAST, 
                Utility2D.Direction2D.SOUTH, 
                Utility2D.Direction2D.WEST
            };

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
            int numberOfWavesToCollapse = MaxIterations <= 0? _waves.Count : MaxIterations;

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
                //PropagateConstraints(chosenPosition, chosenTile.Constraints);

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
                Dictionary<Vector2, bool> positionsVisitedByPropagation = new Dictionary<Vector2, bool>();
                positionsVisitedByPropagation[chosenPosition] = true;
                //PropagateConstraints(chosenPosition, chosenTile.Constraints, positionsVisitedByPropagation);

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

        //private void PropagateConstraints(Vector2 chosenPosition, List<Constraint> constraint, Dictionary<Vector2, bool> positionsVisitedByPropagation)
        //{
        //    for (int i = 0; i < constraint.Count; i++) 
        //    {
        //        Utility2D.Direction2D direction = constraint[i].Direction;
        //        Vector2 tileInDirection = chosenPosition + new Vector2Int(Utility2D.GridDirections2D[direction].y, Utility2D.GridDirections2D[direction].x) * CellSize;
        //        List<TileScriptableObject> possibleNeighbours = constraint[i].PossibleNeighbours;
        //        if (!_waves.TryGetValue(tileInDirection, out List<TileScriptableObject> neighboursSuperpositions)) return;
                
        //        for (int j = 0; j < neighboursSuperpositions.Count; j++)
        //        {
        //            if (possibleNeighbours.Contains(neighboursSuperpositions[j])) continue;

        //            neighboursSuperpositions.RemoveAt(j);
        //            j--;
        //        }

        //        positionsVisitedByPropagation[tileInDirection] = true;

        //        for (int j = 0; j < neighboursSuperpositions.Count; j++)
        //        {
        //            if (positionsVisitedByPropagation.TryGetValue(tileInDirection))
        //            PropagateConstraints(tileInDirection, neighboursSuperpositions[j].Constraints, positionsVisitedByPropagation);
        //        }
        //    }
        //}

        private void PropagateContraints(Vector2 chosenPosition)
        {
            //List<Constraint> constraints = _waves[chosenPosition][0].Constraints[0].;
        }

        //private void PropagateConstraint(Vector2 chosenPosition, Utility2D.Direction2D direction, List<TileScriptableObject> possibleNeighbours)
        //{
        //    Vector2 tileInDirection;
        //    Vector2Int directionInGrid = Utility2D.GridDirections2D[direction];
        //    tileInDirection = chosenPosition + new Vector2Int(directionInGrid.y, directionInGrid.x) * CellSize;
        //    if (!_waves.TryGetValue(tileInDirection, out List<TileScriptableObject> neighbours)) return;
        //    for (int i = 0; i < neighbours.Count; i++)
        //    {
        //        if (possibleNeighbours.Contains(neighbours[i])) continue;

        //        neighbours.RemoveAt(i);
        //        i--;
        //    }
        //}

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

