using GMDG.Basic2DPlatformer.PCG.WFC;
using GMDG.Basic2DPlatformer.System;
using GMDG.Basic2DPlatformer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Event = GMDG.Basic2DPlatformer.System.Event;

namespace GMDG.Basic2DPlatformer.PCG
{
    public class LevelGenerator
    {
        public Func<MonoBehaviour, PCGData, int, float, bool, IEnumerator> Generation;

        public LevelGenerator() 
        {
            Generation = Generate;       
        }

        private IEnumerator Generate(MonoBehaviour caller, PCGData data, int iterationLimit, float timeout, bool isSimulated)
        {
            // Place Starting and Ending Cell
            if (isSimulated) yield return caller.StartCoroutine(InitializeSuperPositions(data.Grid, data, timeout, isSimulated));
            else InitializeSuperPositions(data.Grid, data, timeout, isSimulated).MoveNext();

            // Use WFC for placing chunks
            if (isSimulated) yield return caller.StartCoroutine(new EvenSimplerTiledModel(caller, data).Generate(iterationLimit, timeout, isSimulated));
            else new EvenSimplerTiledModel(caller, data).Generate(iterationLimit, timeout, isSimulated).MoveNext();
        }

        private IEnumerator InitializeSuperPositions(Grid<HashSet<int>> grid, PCGData data, float timeout, bool isSimulated)
        {
            for (int i = 0; i < grid.GridSize.y; i++) 
            { 
                for (int j = 0; j < grid.GridSize.x; j++)
                {
                    Vector2Int currentPosition = new Vector2Int(j, i);
                    HashSet<int> superPositions = new HashSet<int>();

                    if (currentPosition == data.StartingCell)
                    {
                        grid.PlaceElement(currentPosition, new HashSet<int>(PCGData.START_CELL));
                        superPositions.Add(PCGData.START_CELL);
                    }
                    else if (currentPosition == data.EndingCell) 
                    {
                        grid.PlaceElement(currentPosition, new HashSet<int>());
                        superPositions.Add(PCGData.END_CELL);
                    }
                    else
                    {
                        superPositions.UnionWith(Enumerable.Range(PCGData.END_CELL + 1, data.WFCTiles.Count - 2));
                    }
                    
                    grid.PlaceElement(currentPosition, superPositions);
                }
            }

            if (isSimulated)
            {
                EventManager.Instance.Publish(Event.OnGridUpdated, grid);
                yield return new WaitForSeconds(timeout);
            }
        }
    }
}