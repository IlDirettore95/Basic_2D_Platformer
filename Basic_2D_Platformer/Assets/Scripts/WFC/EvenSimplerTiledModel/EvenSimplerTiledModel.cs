using GMDG.NoProduct.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PCG.WFC
{
    public class EvenSimplerTiledModel : IWFCModel
    {
        private MonoBehaviour _caller;
        //private WFCData _data;
        private PCGData _data;
        //private WFCTile[] _tiles;
        private Dictionary<Vector2, HashSet<WFCTile>> _uncollapsedPositions;
        private Dictionary<Vector2, WFCTile> _collapsedPositions;
        private IEnumerator _solverProcedure;
        
        public EvenSimplerTiledModel(MonoBehaviour caller, PCGData data, int iterationsLimit)
        {
            _caller = caller;
            _data = data;
            _solverProcedure = SolverProcedure(iterationsLimit);
        }

        public IEnumerator Generate()
        {
            // Initialize
            Initialize(_data);

            //Solver
            yield return _caller.StartCoroutine(_solverProcedure);
        }

        public IEnumerator SolverProcedure(int iterationsLimit)
        {
            for (int i = 0; i < iterationsLimit || iterationsLimit < 0; i++)
            {
                bool positionFound = ChooseNextPosition(out Vector2 chosenPosition);
                if (!positionFound) break;
                Observe(chosenPosition);
                Propagate(chosenPosition);
                yield return null;
            }
            if (AreAllPositionsCollapsed())
            {
                Debug.Log("Finished!");
            }
            else
            {
                Debug.Log("Failed!");
            }
        }

        public void Draw()
        {
            _data.Grid.Draw();
            if (_uncollapsedPositions == null || _uncollapsedPositions.Count == 0) return;
            foreach (Vector2 position in _uncollapsedPositions.Keys)
            {
                string stringList = string.Empty;

                foreach (WFCTile tile in _uncollapsedPositions[position])
                {
                    stringList += _data.WFCTiles.FirstOrDefault(x => x == tile).Name;
                    stringList += ", ";
                }
                stringList += string.Format("({0})", Entropy(_uncollapsedPositions[position]));
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.UpperLeft;
                style.normal.textColor = Color.green;
                style.fontSize = 8;
                Handles.Label(position, stringList, style);
            }
        }

        private void Initialize(PCGData data)
        {
            //_tiles = new WFCTile[data.Tiles.Values.Count];
            //WFCTile[] dataTiles = data.Tiles.Values.ToArray();
            //for (int i = 0; i < dataTiles.Length; i++) 
            //{
            //    _tiles[i] = dataTiles[i];
            //}

            _uncollapsedPositions = new Dictionary<Vector2, HashSet<WFCTile>>();
            for (int i = 0; i < data.Grid.GridSize.y; i++)
            {
                for (int j = 0; j < data.Grid.GridSize.x; j++)
                {
                    Vector2 position = data.Grid.CellsPositions[i, j];
                    _uncollapsedPositions[position] = new HashSet<WFCTile>();
                    foreach (WFCTile tile in data.WFCTiles)
                    {
                        _uncollapsedPositions[position].Add(tile);
                    }
                }
            }

            _collapsedPositions = new Dictionary<Vector2, WFCTile>();
        }

        private bool ChooseNextPosition(out Vector2 chosenPosition)
        {
            float minEntropy = float.PositiveInfinity;
            List<Vector2> possibleChosenPosition = new List<Vector2>();
            chosenPosition = Vector2.zero;

            foreach (Vector2 position in _uncollapsedPositions.Keys) 
            {
                float entropy = Entropy(_uncollapsedPositions[position]);

                if (entropy < minEntropy)
                {
                    minEntropy = entropy;
                    possibleChosenPosition.Clear();
                    possibleChosenPosition.Add(position);
                }
                else if (entropy == minEntropy)
                {
                    possibleChosenPosition.Add(position);
                }
            }
            if (possibleChosenPosition.Count > 0)
            {
                chosenPosition = possibleChosenPosition[UnityEngine.Random.Range(0, possibleChosenPosition.Count)];
                return true;
            }

            return false;
        }

        private void Observe(Vector2 position)
        {
            HashSet<WFCTile> superPositions = _uncollapsedPositions[position];
            WFCTile collapsedWave = null;

            float totalRelativeFrequency = 0;
            foreach (WFCTile superPosition in superPositions)
            {
                totalRelativeFrequency += superPosition.RelativeFrequency;
            }

            float randomChoice = UnityEngine.Random.Range(0, totalRelativeFrequency);

            foreach (WFCTile superPosition in superPositions)
            {
                randomChoice -= superPosition.RelativeFrequency;

                if (randomChoice > 0) continue;

                collapsedWave = superPosition;
                break;
            }

            _uncollapsedPositions.Remove(position);
            _collapsedPositions.Add(position, collapsedWave);

            GameObject.Instantiate(collapsedWave.Prefab, position, Quaternion.identity, _caller.gameObject.transform);
        }

        private void Propagate(Vector2 chosenPosition)
        {
            Queue<Vector2> positionsToPropagate = new Queue<Vector2>();
            positionsToPropagate.Enqueue(chosenPosition);

            while (positionsToPropagate.Count > 0)
            {
                Vector2 currentPosition = positionsToPropagate.Dequeue();
                foreach (Utility2D.Direction2D direction in Utility2D.Directions2D)
                {
                    HashSet<WFCTile> possibleNeighbours = new HashSet<WFCTile>();

                    if (IsPositionCollapsed(currentPosition))
                    {
                        possibleNeighbours.UnionWith(_collapsedPositions[currentPosition].PossibleNeighbours[direction]);
                    }
                    else
                    {
                        foreach (WFCTile superPosition in _uncollapsedPositions[currentPosition])
                        {
                            possibleNeighbours.UnionWith(superPosition.PossibleNeighbours[direction]);
                        }
                    }

                    Vector2Int directionInGrid = Utility2D.GridDirections2D[direction];
                    Vector2 positionInDirection = currentPosition + new Vector2Int(directionInGrid.y, directionInGrid.x) * _data.Grid.CellSize;

                    if (!_uncollapsedPositions.TryGetValue(positionInDirection, out HashSet<WFCTile> neighbourSuperPositions)) continue;

                    int numberOfNeighbourSuperPositions = neighbourSuperPositions.Count;

                    neighbourSuperPositions.IntersectWith(possibleNeighbours);

                    if (neighbourSuperPositions.Count == numberOfNeighbourSuperPositions) continue;

                    positionsToPropagate.Enqueue(positionInDirection);
                }
            }
        }

        private float Entropy(HashSet<WFCTile> superPositions)
        {
            float totalFrequency = 0;
            float totalSum = 0;
            foreach (WFCTile superPosition in superPositions)
            {
                totalFrequency += superPosition.RelativeFrequency;
                totalSum += superPosition.RelativeFrequency * Mathf.Log(superPosition.RelativeFrequency, 2);
            }
            return Mathf.Log(totalFrequency, 2) - totalSum / totalFrequency;
        }

        private bool AreAllPositionsCollapsed()
        {
            return _uncollapsedPositions.Count == 0;
        }

        private bool IsPositionCollapsed(Vector2 position)
        {
            return _collapsedPositions.ContainsKey(position);
        }
    }
}
