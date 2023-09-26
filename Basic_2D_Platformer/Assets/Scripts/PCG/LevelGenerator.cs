using GMDG.Basic2DPlatformer.PCG.WFC;
using GMDG.Basic2DPlatformer.System;
using GMDG.Basic2DPlatformer.Utility;
using GMDG.NoProduct.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Event = GMDG.Basic2DPlatformer.System.Event;
using EventManager = GMDG.Basic2DPlatformer.System.EventManager;

namespace GMDG.Basic2DPlatformer.PCG
{
    public class LevelGenerator
    {
        public Func<MonoBehaviour, PCGData, int, float, bool, bool, IEnumerator> Generation;

        public LevelGenerator() 
        {
            Generation = Generate;       
        }

        private IEnumerator Generate(MonoBehaviour caller, PCGData data, int iterationLimit, float timeout, bool isSimulated, bool isHardSimulated)
        {
            // Generate a Path from Start to Passage to End

            // Initialize SuperPositions
            if (isHardSimulated) yield return caller.StartCoroutine(InitializeSuperPositions(caller, data.Grid, data, timeout, isSimulated, isHardSimulated));
            else InitializeSuperPositions(caller, data.Grid, data, timeout, isSimulated, isHardSimulated).MoveNext();


            // Use WFC for placing chunks
            if (isSimulated) yield return caller.StartCoroutine(new SimpleTiledModel(caller, data).Generate(iterationLimit, timeout, isSimulated, isHardSimulated));
            else new SimpleTiledModel(caller, data).Generate(iterationLimit, timeout, isSimulated, isHardSimulated).MoveNext();

            EventManager.Instance.Publish(Event.OnLevelGenerated, data);
        }

        private IEnumerator InitializeSuperPositions(MonoBehaviour caller, Grid<HashSet<int>> grid, PCGData data, float timeout, bool isSimulated, bool isHardSimulated)
        {
            for (int i = 0; i < grid.GridSize.y; i++)
            {
                for (int j = 0; j < grid.GridSize.x; j++)
                {
                    Vector2Int position = new Vector2Int(j, i);

                    HashSet<int> superPositions = new HashSet<int>();
                    grid.PlaceElement(position, superPositions);
                }
            }

            if (isHardSimulated) yield return caller.StartCoroutine(PlaceDefaultSuperPositions(data.Grid, data, timeout, isSimulated, isHardSimulated));
            else PlaceDefaultSuperPositions(data.Grid, data, timeout, isSimulated, isHardSimulated).MoveNext();

            if (isHardSimulated) yield return caller.StartCoroutine(PlacePOISuperPositions(data.Grid, data, timeout, isSimulated, isHardSimulated));
            else PlacePOISuperPositions(data.Grid, data, timeout, isSimulated, isHardSimulated).MoveNext();

            if (isHardSimulated) yield return caller.StartCoroutine(PlaceBorderSuperPositions(data.Grid, data, timeout, isSimulated, isHardSimulated));
            else PlaceBorderSuperPositions(data.Grid, data, timeout, isSimulated, isHardSimulated).MoveNext();

            if (isHardSimulated) yield return caller.StartCoroutine(PlaceFeasiblePathSupePositions(caller, data.Grid, data, timeout, isSimulated, isHardSimulated));
            else PlaceFeasiblePathSupePositions(caller, data.Grid, data, timeout, isSimulated, isHardSimulated).MoveNext();
        }

        private IEnumerator PlaceDefaultSuperPositions(Grid<HashSet<int>> grid, PCGData data, float timeout, bool isSimulated, bool isHardSimulated)
        {
            for (int i = 0; i < grid.GridSize.y; i++)
            {
                for (int j = 0; j < grid.GridSize.x; j++)
                {
                    Vector2Int position = new Vector2Int(j, i);

                    HashSet<int> superPositions = new HashSet<int>();
                    superPositions.UnionWith(Enumerable.Range(0, data.WFCTiles.Count));
                    grid.PlaceElement(position, superPositions);

                    if (isHardSimulated)
                    {
                        EventManager.Instance.Publish(Event.OnPCGUpdated, data);
                        yield return new WaitForSeconds(timeout);
                    }
                }
            }
        }

        private IEnumerator PlacePOISuperPositions(Grid<HashSet<int>> grid, PCGData data, float timeout, bool isSimulated, bool isHardSimulated)
        {
            for (int i = 0; i < grid.GridSize.y; i++)
            {
                for (int j = 0; j < grid.GridSize.x; j++)
                {
                    Vector2Int position = new Vector2Int(j, i);

                    HashSet<int> superPositions = grid.GetElement(position);

                    if (position == data.StartingCell)
                    {
                        superPositions.IntersectWith(new HashSet<int> { PCGData.START_CELL });
                    }
                    else if (position == data.EndingCell)
                    {
                        superPositions.IntersectWith(new HashSet<int> { PCGData.END_CELL });
                    }
                    else if (position == data.PassageCell)
                    {
                        superPositions.IntersectWith(new HashSet<int> { PCGData.PASSAGE_CELL });
                    }
                    else
                    {
                        superPositions.ExceptWith(new HashSet<int> { PCGData.START_CELL, PCGData.END_CELL, PCGData.PASSAGE_CELL });
                    }
                    if (isHardSimulated)
                    {
                        EventManager.Instance.Publish(Event.OnPCGUpdated, data);
                        yield return new WaitForSeconds(timeout);
                    }
                }
            }
        }

        private IEnumerator PlaceBorderSuperPositions(Grid<HashSet<int>> grid, PCGData data, float timeout, bool isSimulated, bool isHardSimulated)
        {
            // Border N
            for (int j = 0; j < grid.GridSize.x; j++)
            {
                Vector2Int position = new Vector2Int(j, grid.GridSize.y - 1);

                HashSet<int> superPositions = grid.GetElement(position);
                HashSet<int> possibleSuperPositions = new HashSet<int>();
                for (int k = 0; k < data.WFCTiles.Count; k++)
                {
                    WFCTile tile = data.WFCTiles[k];
                    if (tile.PassabilityFlags[WFCTile.N_IN] || tile.PassabilityFlags[WFCTile.N_OUT]) continue;
                    possibleSuperPositions.Add(k);
                }
                superPositions.IntersectWith(possibleSuperPositions);

                if (isHardSimulated)
                {
                    EventManager.Instance.Publish(Event.OnPCGUpdated, data);
                    yield return new WaitForSeconds(timeout);
                }
            }
            // Border E
            for (int i = 0; i < grid.GridSize.y; i++)
            {
                Vector2Int position = new Vector2Int(grid.GridSize.x - 1, i);

                HashSet<int> superPositions = grid.GetElement(position);
                HashSet<int> possibleSuperPositions = new HashSet<int>();
                for (int k = 0; k < data.WFCTiles.Count; k++)
                {
                    WFCTile tile = data.WFCTiles[k];
                    if (tile.PassabilityFlags[WFCTile.E_IN] || tile.PassabilityFlags[WFCTile.E_OUT]) continue;
                    possibleSuperPositions.Add(k);
                }
                superPositions.IntersectWith(possibleSuperPositions);

                if (isHardSimulated)
                {
                    EventManager.Instance.Publish(Event.OnPCGUpdated, data);
                    yield return new WaitForSeconds(timeout);
                }
            }
            // Border S
            for (int j = 0; j < grid.GridSize.x; j++)
            {
                Vector2Int position = new Vector2Int(j, 0);

                HashSet<int> superPositions = grid.GetElement(position);
                HashSet<int> possibleSuperPositions = new HashSet<int>();
                for (int k = 0; k < data.WFCTiles.Count; k++)
                {
                    WFCTile tile = data.WFCTiles[k];
                    if (tile.PassabilityFlags[WFCTile.S_IN] || tile.PassabilityFlags[WFCTile.S_OUT]) continue;
                    possibleSuperPositions.Add(k);
                }
                superPositions.IntersectWith(possibleSuperPositions);

                if (isHardSimulated)
                {
                    EventManager.Instance.Publish(Event.OnPCGUpdated, data);
                    yield return new WaitForSeconds(timeout);
                }
            }
            // Border W
            for (int i = 0; i < grid.GridSize.y; i++)
            {
                Vector2Int position = new Vector2Int(0, i);

                HashSet<int> superPositions = grid.GetElement(position);
                HashSet<int> possibleSuperPositions = new HashSet<int>();
                for (int k = 0; k < data.WFCTiles.Count; k++)
                {
                    WFCTile tile = data.WFCTiles[k];
                    if (tile.PassabilityFlags[WFCTile.W_IN] || tile.PassabilityFlags[WFCTile.W_OUT]) continue;
                    possibleSuperPositions.Add(k);
                }
                superPositions.IntersectWith(possibleSuperPositions);

                if (isHardSimulated)
                {
                    EventManager.Instance.Publish(Event.OnPCGUpdated, data);
                    yield return new WaitForSeconds(timeout);
                }
            }

        }

        private IEnumerator PlaceFeasiblePathSupePositions(MonoBehaviour caller, Grid<HashSet<int>> grid, PCGData data, float timeout, bool isSimulated, bool isHardSimulated)
        {
            data.FeasiblePath = new List<Vector2Int>();

            // From Start to Passage
            if (isHardSimulated) yield return caller.StartCoroutine(PlaceFeasiblePathBetween(data.StartingCell, data.WFCTiles[PCGData.START_CELL], data.PassageCell, data.WFCTiles[PCGData.PASSAGE_CELL], data.Grid, data, timeout, isSimulated, isHardSimulated));
            else PlaceFeasiblePathBetween(data.StartingCell, data.WFCTiles[PCGData.START_CELL], data.PassageCell, data.WFCTiles[PCGData.PASSAGE_CELL], data.Grid, data, timeout, isSimulated, isHardSimulated).MoveNext();

            // From Passage to End
            if (isHardSimulated) yield return caller.StartCoroutine(PlaceFeasiblePathBetween(data.PassageCell, data.WFCTiles[PCGData.PASSAGE_CELL], data.EndingCell, data.WFCTiles[PCGData.END_CELL], grid, data, timeout, isSimulated, isHardSimulated));
            else PlaceFeasiblePathBetween(data.PassageCell, data.WFCTiles[PCGData.PASSAGE_CELL], data.EndingCell, data.WFCTiles[PCGData.END_CELL], grid, data, timeout, isSimulated, isHardSimulated).MoveNext();

            for (int i = 1; i < data.FeasiblePath.Count - 1; i++)
            {
                Vector2Int precedentPosition = data.FeasiblePath[i - 1];
                Vector2Int currentPosition = data.FeasiblePath[i];
                Vector2Int subsequentPosition = data.FeasiblePath[i + 1];

                HashSet<int> superPositions = grid.GetElement(currentPosition);
                HashSet<int> possibleSuperPositions = new HashSet<int>();

                HashSet<int> requiredFlags = new HashSet<int>();
                if (precedentPosition.y > currentPosition.y) requiredFlags.Add(WFCTile.N_IN);
                if (precedentPosition.x > currentPosition.x) requiredFlags.Add(WFCTile.E_IN);
                if (precedentPosition.y < currentPosition.y) requiredFlags.Add(WFCTile.S_IN);
                if (precedentPosition.x < currentPosition.x) requiredFlags.Add(WFCTile.W_IN);
                if (subsequentPosition.y > currentPosition.y) requiredFlags.Add(WFCTile.N_OUT);
                if (subsequentPosition.x > currentPosition.x) requiredFlags.Add(WFCTile.E_OUT);
                if (subsequentPosition.y < currentPosition.y) requiredFlags.Add(WFCTile.S_OUT);
                if (subsequentPosition.x < currentPosition.x) requiredFlags.Add(WFCTile.W_OUT);

                for (int k = 0; k < data.WFCTiles.Count; k++)
                {
                    WFCTile tile = data.WFCTiles[k];
                    bool allFlags = true;
                    foreach (int flagType in requiredFlags)
                    {
                        allFlags &= tile.PassabilityFlags[flagType];
                    }
                    if (!allFlags) continue;
                    possibleSuperPositions.Add(k);
                }

                superPositions.IntersectWith(possibleSuperPositions);

                if (isHardSimulated)
                {
                    EventManager.Instance.Publish(Event.OnPCGUpdated, data);
                    yield return new WaitForSeconds(timeout);
                }
            }
        }

        private IEnumerator PlaceFeasiblePathBetween(Vector2Int start, WFCTile startTile, Vector2Int end, WFCTile endTile, Grid<HashSet<int>> grid, PCGData data, float timeout, bool isSimulated, bool isHardSimulated)
        {
            List<Vector2Int> currentStartPath = new List<Vector2Int> { start };
            List<Vector2Int> currentEndPath = new List<Vector2Int> { end };

            Vector2Int currentStartPosition = start;
            Vector2Int currentEndPosition = end;

            Vector2Int nextStartPosition = currentStartPosition + TakeAGridPositionBasedOnTileInfo(startTile, true);
            Vector2Int nextEndPosition = currentEndPosition + TakeAGridPositionBasedOnTileInfo(endTile, false);

            currentStartPath.Add(nextStartPosition);
            currentEndPath.Add(nextEndPosition);

            currentStartPosition = nextStartPosition;
            currentEndPosition = nextEndPosition;

            while (currentStartPosition != currentEndPosition)
            {
                nextStartPosition = GetNextPosition(currentStartPosition, currentEndPosition, currentStartPath, currentEndPath);
                currentStartPath.Add(nextStartPosition);

                if (currentEndPosition == nextStartPosition) break;

                nextEndPosition = GetNextPosition(currentEndPosition, nextStartPosition, currentStartPath, currentEndPath);
                currentEndPath.Add(nextEndPosition);

                currentStartPosition = nextStartPosition;
                currentEndPosition = nextEndPosition;

                if (isHardSimulated)
                {
                    EventManager.Instance.Publish(Event.OnPCGUpdated, data);
                    yield return new WaitForSeconds(timeout);
                }
            }

            if (data.FeasiblePath.Contains(start)) data.FeasiblePath.AddRange(currentStartPath.Skip(1));
            else data.FeasiblePath.AddRange(currentStartPath);

            currentEndPath.Reverse();
            data.FeasiblePath.AddRange(currentEndPath);
        }

        private Vector2Int GetNextPosition(Vector2Int currentPosition, Vector2Int end) 
        {
            int nextX = currentPosition.x;
            int nextY = currentPosition.y;
            
            if (currentPosition.x < end.x)
            {
                nextX++;
            }
            else if (currentPosition.x > end.x)
            {
                nextX--;
            }
            else if (currentPosition.y < end.y)
            {
                nextY++;
            }
            else if (currentPosition.y > end.y)
            {
                nextY--;
            }

            return new Vector2Int(nextX, nextY);
        }

        private Vector2Int TakeAGridPositionBasedOnTileInfo(WFCTile tile, bool fromStart)
        {
            Vector2Int resultInGrid = new Vector2Int(-1, -1);

            if (fromStart)
            {
                if (tile.PassabilityFlags[WFCTile.N_OUT]) resultInGrid = Utility2D.GridDirections2D[Utility2D.Direction2D.NORTH];
                else if (tile.PassabilityFlags[WFCTile.E_OUT]) resultInGrid = Utility2D.GridDirections2D[Utility2D.Direction2D.EAST];
                else if (tile.PassabilityFlags[WFCTile.S_OUT]) resultInGrid = Utility2D.GridDirections2D[Utility2D.Direction2D.SOUTH];
                else if (tile.PassabilityFlags[WFCTile.W_OUT]) resultInGrid = Utility2D.GridDirections2D[Utility2D.Direction2D.WEST];
            }
            else
            {
                if (tile.PassabilityFlags[WFCTile.N_IN]) resultInGrid = Utility2D.GridDirections2D[Utility2D.Direction2D.NORTH];
                else if (tile.PassabilityFlags[WFCTile.E_IN]) resultInGrid = Utility2D.GridDirections2D[Utility2D.Direction2D.EAST];
                else if (tile.PassabilityFlags[WFCTile.S_IN]) resultInGrid = Utility2D.GridDirections2D[Utility2D.Direction2D.SOUTH];
                else if (tile.PassabilityFlags[WFCTile.W_IN]) resultInGrid = Utility2D.GridDirections2D[Utility2D.Direction2D.WEST];
            }

            return new Vector2Int(resultInGrid.y, resultInGrid.x);
        }
    }
}