using GMDG.Basic2DPlatformer.System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using static GMDG.NoProduct.Utility.Utility2D;
using Event = GMDG.Basic2DPlatformer.System.Event;

namespace GMDG.Basic2DPlatformer.PCG.WFC
{
    public class EvenSimplerTiledModel : IWFCModel
    {
        private MonoBehaviour _caller;
        private PCGData _data;
        private Dictionary<Vector2, HashSet<int>> _uncollapsedPositions;
        private Dictionary<Vector2, int> _collapsedPositions;
        
        public EvenSimplerTiledModel(MonoBehaviour caller, PCGData data)
        {
            _caller = caller;
            _data = data;
            _collapsedPositions = new Dictionary<Vector2, int>();
        }

        public IEnumerator Generate(int iterationsLimit, float timeout, bool isSimulated)
        {
            if (isSimulated) yield return _caller.StartCoroutine(SolverProcedure(iterationsLimit, timeout, isSimulated));
            else SolverProcedure(iterationsLimit, timeout, isSimulated).MoveNext();
        }

        public IEnumerator SolverProcedure(int iterationsLimit, float timeout, bool isSimulated)
        {
            if (isSimulated) yield return _caller.StartCoroutine(Initialize(timeout, isSimulated));
            else Initialize(timeout, isSimulated).MoveNext();

            Debug.Log("Uncollapsed : " + _uncollapsedPositions.Keys.Count);
            Debug.Log("Collapsed : " + _collapsedPositions.Keys.Count);

            for (int i = 0; i < iterationsLimit || iterationsLimit < 0; i++)
            {
                bool positionFound = ChooseNextPosition(out Vector2 chosenPosition);
                if (!positionFound) break;

                if (isSimulated) yield return _caller.StartCoroutine(Observe(chosenPosition, timeout, isSimulated));
                else Observe(chosenPosition, timeout, isSimulated).MoveNext();

                if (isSimulated) yield return _caller.StartCoroutine(Propagate(chosenPosition, timeout, isSimulated));
                else Propagate(chosenPosition, timeout, isSimulated).MoveNext();

                if (isSimulated)
                {
                    EventManager.Instance.Publish(Event.OnGridUpdated, _data.Grid);
                    yield return new WaitForSeconds(timeout);
                }
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

            if (isSimulated)
            {
                EventManager.Instance.Publish(Event.OnGridUpdated, _data.Grid);
                yield return null;
            }
        }

        private IEnumerator Initialize(float timeout, bool isSimulated)
        {
            _uncollapsedPositions = new Dictionary<Vector2, HashSet<int>>();
            for (int i = 0; i < _data.Grid.GridSize.y; i++)
            {
                for (int j = 0; j < _data.Grid.GridSize.x; j++)
                {
                    Vector2 position = _data.Grid.CellsPositions[i, j];
                    HashSet<int> superPositions = _data.Grid.GetElement(i, j);
                    _uncollapsedPositions[position] = superPositions;
                }
            }
            for (int i = 0; i < _data.Grid.GridSize.y; i++)
            {
                for (int j = 0; j < _data.Grid.GridSize.x; j++)
                {
                    Vector2 position = _data.Grid.CellsPositions[i, j];

                    HashSet<int> superPositions = _data.Grid.GetElement(i, j);

                    if (superPositions.Count == 1)
                    {
                        Debug.Log(string.Format("Posizione {0} {1}", i, j));
                        CollapsePosition(position, superPositions.First());

                        if (isSimulated) yield return _caller.StartCoroutine(Propagate(position, timeout, isSimulated));
                        else Propagate(position, timeout, isSimulated).MoveNext();
                    }

                    if (isSimulated)
                    {
                        EventManager.Instance.Publish(Event.OnGridUpdated, this._data.Grid);
                        yield return new WaitForSeconds(timeout);
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

        private IEnumerator Observe(Vector2 position, float timeout, bool isSimulated)
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

            if (isSimulated)
            {
                EventManager.Instance.Publish(Event.OnGridUpdated, _data.Grid);
                yield return new WaitForSeconds(timeout);
            }
        }

        private IEnumerator Propagate(Vector2 chosenPosition, float timeout, bool isSimulated)
        {
            Queue<Vector2> positionsToPropagate = new Queue<Vector2>();
            positionsToPropagate.Enqueue(chosenPosition);

            while (positionsToPropagate.Count > 0)
            {
                Vector2 currentPosition = positionsToPropagate.Dequeue();
                foreach (Direction2D direction in Directions2D)
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

                    if (currentPosition == _data.Grid.CellsPositions[4, 0])
                    {
                        Debug.Log("Direction : " + direction);
                        Debug.Log("possibleNeighbours : ");
                        foreach (int i in possibleNeighbours)
                        {
                            Debug.Log(_data.WFCTiles[i].Name);
                        }
                    }

                    Vector2Int directionInGrid = GridDirections2D[direction];
                    Vector2 positionInDirection = currentPosition + new Vector2Int(directionInGrid.y, directionInGrid.x) * _data.Grid.CellSize;

                    if (!_uncollapsedPositions.TryGetValue(positionInDirection, out HashSet<int> neighbourSuperPositions)) continue;

                    int numberOfNeighbourSuperPositions = neighbourSuperPositions.Count;

                    neighbourSuperPositions.IntersectWith(possibleNeighbours);

                    if (currentPosition == _data.Grid.CellsPositions[4, 0])
                    {
                        Debug.Log("Direction : " + direction);
                        Debug.Log("possibleNeighbours after intersect : ");
                        foreach (int i in neighbourSuperPositions)
                        {
                            Debug.Log(_data.WFCTiles[i].Name);
                        }
                    }

                    if (neighbourSuperPositions.Count == numberOfNeighbourSuperPositions) continue;

                    positionsToPropagate.Enqueue(positionInDirection);

                    if (isSimulated)
                    {
                        EventManager.Instance.Publish(Event.OnGridUpdated, _data.Grid);
                        yield return new WaitForSeconds(timeout);
                    }
                }
            }
        }

        private void CollapsePosition(Vector2 position, int collapsedWave)
        {
            _uncollapsedPositions.Remove(position);
            _collapsedPositions.Add(position, collapsedWave);

            GameObject.Instantiate(_data.WFCTiles[collapsedWave].Prefab, position, Quaternion.identity, _caller.gameObject.transform);

            EventManager.Instance.Publish(Event.OnGridUpdated, _data.Grid);
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