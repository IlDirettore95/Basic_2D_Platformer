using GMDG.Basic2DPlatformer.System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Event = GMDG.Basic2DPlatformer.System.Event;

namespace GMDG.Basic2DPlatformer.PCG.WFC
{
    public class EvenSimplerTiledModel : IWFCModel
    {
        private MonoBehaviour _caller;
        private PCGData _data;
        private Dictionary<Vector2, HashSet<int>> _uncollapsedPositions;
        private Dictionary<Vector2, int> _collapsedPositions;
        private IEnumerator _solverProcedure;
        
        public EvenSimplerTiledModel(MonoBehaviour caller, PCGData data, int iterationsLimit)
        {
            _caller = caller;
            _data = data;
            _collapsedPositions = new Dictionary<Vector2, int>();
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
            yield return new WaitForSeconds(2);
            for (int i = 0; i < iterationsLimit || iterationsLimit < 0; i++)
            {
                bool positionFound = ChooseNextPosition(out Vector2 chosenPosition);
                if (!positionFound) break;
                Observe(chosenPosition);
                Propagate(chosenPosition);
                EventManager.Instance.Publish(Event.OnGridUpdated, _data.Grid);
                yield return null;
            }
            if (AreAllPositionsCollapsed())
            {
#if UNITY_EDITOR
                Debug.Log("Finished!");
#endif
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("Failed!");
#endif
            }
        }

        private void Initialize(PCGData data)
        {
            _uncollapsedPositions = new Dictionary<Vector2, HashSet<int>>();
            for (int i = 0; i < data.Grid.GridSize.y; i++)
            {
                for (int j = 0; j < data.Grid.GridSize.x; j++)
                {
                    Vector2 position = data.Grid.CellsPositions[i, j];

                    HashSet<int> superPositions = data.Grid.GetElement(i, j);

                    if (superPositions.Count == 1)
                    {
                        CollapsePosition(position, superPositions.First());
                        Propagate(position);
                    }
                    else
                    {
                        _uncollapsedPositions[position] = superPositions;
                    }
                }
            }
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
                chosenPosition = possibleChosenPosition[Random.Range(0, possibleChosenPosition.Count)];
                return true;
            }

            return false;
        }

        private void Observe(Vector2 position)
        {
            HashSet<int> superPositions = _uncollapsedPositions[position];
            int collapsedWave = -1;

            float totalRelativeFrequency = 0;
            foreach (int superPosition in superPositions)
            {
                totalRelativeFrequency += _data.WFCTiles[superPosition].RelativeFrequency;
            }

            float randomChoice = Random.Range(0, totalRelativeFrequency);

            foreach (int superPosition in superPositions)
            {
                randomChoice -= _data.WFCTiles[superPosition].RelativeFrequency;

                if (randomChoice > 0) continue;

                collapsedWave = superPosition;
                break;
            }

            CollapsePosition(position, collapsedWave);
        }

        private void Propagate(Vector2 chosenPosition)
        {
            Queue<Vector2> positionsToPropagate = new Queue<Vector2>();
            positionsToPropagate.Enqueue(chosenPosition);

            while (positionsToPropagate.Count > 0)
            {
                Vector2 currentPosition = positionsToPropagate.Dequeue();
                foreach (NoProduct.Utility.Utility2D.Direction2D direction in NoProduct.Utility.Utility2D.Directions2D)
                {
                    HashSet<int> possibleNeighbours = new HashSet<int>();

                    if (IsPositionCollapsed(currentPosition))
                    {
                        WFCTile currentWFCTile = _data.WFCTiles[_collapsedPositions[currentPosition]];
                        possibleNeighbours.UnionWith(currentWFCTile.PossibleNeighbours[direction]);
                    }
                    else
                    {
                        foreach (int superPosition in _uncollapsedPositions[currentPosition])
                        {
                            WFCTile currentWFCTile = _data.WFCTiles[superPosition];
                            possibleNeighbours.UnionWith(currentWFCTile.PossibleNeighbours[direction]);
                        }
                    }

                    Vector2Int directionInGrid = NoProduct.Utility.Utility2D.GridDirections2D[direction];
                    Vector2 positionInDirection = currentPosition + new Vector2Int(directionInGrid.y, directionInGrid.x) * _data.Grid.CellSize;

                    if (!_uncollapsedPositions.TryGetValue(positionInDirection, out HashSet<int> neighbourSuperPositions)) continue;

                    int numberOfNeighbourSuperPositions = neighbourSuperPositions.Count;

                    neighbourSuperPositions.IntersectWith(possibleNeighbours);

                    if (neighbourSuperPositions.Count == numberOfNeighbourSuperPositions) continue;
                    if (neighbourSuperPositions.Count == 1) CollapsePosition(positionInDirection, neighbourSuperPositions.First());

                    positionsToPropagate.Enqueue(positionInDirection);
                }
            }
        }

        private void CollapsePosition(Vector2 position, int collapsedWave)
        {
            _uncollapsedPositions.Remove(position);
            _collapsedPositions.Add(position, collapsedWave);

            GameObject.Instantiate(_data.WFCTiles[collapsedWave].Prefab, position, Quaternion.identity, _caller.gameObject.transform);
        }

        private float Entropy(HashSet<int> superPositions)
        {
            float totalFrequency = 0;
            float totalSum = 0;
            foreach (int superPosition in superPositions)
            {
                float relativeFrequency = _data.WFCTiles[superPosition].RelativeFrequency;
                totalFrequency += relativeFrequency;
                totalSum += relativeFrequency * Mathf.Log(relativeFrequency, 2);
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