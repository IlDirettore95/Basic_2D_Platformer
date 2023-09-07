using GMDG.Basic2DPlatformer.System;
using GMDG.Basic2DPlatformer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Event = GMDG.Basic2DPlatformer.System.Event;

namespace GMDG.Basic2DPlatformer.PCG
{
    public class LevelGenerator
    {
        public Func<MonoBehaviour, PCGData,IEnumerator> Generation;

        private WaitForSeconds _waitForSeconds;
        private 

        public LevelGenerator() 
        {
            Generation = Generate;
            _waitForSeconds = new WaitForSeconds(0.25f);
        }

        private IEnumerator Generate(MonoBehaviour caller, PCGData data)
        {
            Grid<int> grid = new Grid<int>(data.GridSize, data.CellSize, Vector3.zero);

            // Place Air
            for (int i = 0; i < grid.GridSize.y; i++)
            {
                for (int j = 0; j < grid.GridSize.x; j++)
                {
                    grid.PlaceElement(i, j, (int)CellType.UnPassable);
                }
            }

            // Place Starting and Ending Cell
            yield return caller.StartCoroutine(PlaceStartingEndingPoints(grid, data));

            // Place Feasible Path
            yield return caller.StartCoroutine(PlaceFeasiblePath(grid, data));
        }

        private IEnumerator PlaceStartingEndingPoints(Grid<int> grid, PCGData data)
        {
            grid.PlaceElement(data.StartingCell, (int)CellType.Start);
            grid.PlaceElement(data.StartingCell.y + 1, data.StartingCell.x, (int)CellType.Passable);
            grid.PlaceElement(data.StartingCell.y + 2, data.StartingCell.x, (int)CellType.Passable);
            grid.PlaceElement(data.EndingCell, (int)CellType.End);
            grid.PlaceElement(data.EndingCell.y + 1, data.EndingCell.x, (int)CellType.Passable);
            grid.PlaceElement(data.EndingCell.y + 2, data.EndingCell.x, (int)CellType.Passable);

            EventManager.Instance.Publish(Event.OnGridUpdated, grid);

            yield return _waitForSeconds;
        }

        private IEnumerator PlaceFeasiblePath(Grid<int> grid, PCGData data)
        {
            for (int i = data.StartingCell.y, j = data.StartingCell.x + 1; j <= data.EndingCell.x - 1; j++)
            {
                grid.PlaceElement(i, j, (int)CellType.Passable);
                grid.PlaceElement(i + 1, j, (int)CellType.Passable);
                grid.PlaceElement(i + 2, j, (int)CellType.Passable);

                EventManager.Instance.Publish(Event.OnGridUpdated, grid);

                yield return _waitForSeconds;
            }
        }

        public enum CellType
        {
            None,
            Start,
            End,
            Passable,
            UnPassable
        }
    }
}
