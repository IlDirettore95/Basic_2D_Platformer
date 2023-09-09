using GMDG.Basic2DPlatformer.PCG.WFC;
using GMDG.Basic2DPlatformer.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GMDG.NoProduct.Utility.Utility2D;

namespace GMDG.Basic2DPlatformer.PCG
{
    public class PCGData
    {
        public Vector2Int GridSize;
        public Vector2 CellSize;
        public Grid<int> Grid;
        public Vector2Int StartingCell;
        public Vector2Int EndingCell;
        public List<WFCTile> WFCTiles = new List<WFCTile>();

        public override string ToString()
        {
            string text = string.Empty;

            text = string.Concat(text, "Settings\n");
            text = string.Concat(text, string.Format("\tGridSize: {0}\n", GridSize));
            text = string.Concat(text, string.Format("\tCellSize: {0}\n", CellSize));
            text = string.Concat(text, "Tiles\n");

            for (int i = 0; i < WFCTiles.Count; i++)
            {
                WFCTile tile = WFCTiles[i];

                text = string.Concat(text, string.Format("\tName: {0}\n", tile.Name));
                text = string.Concat(text, string.Format("\tRelative Frequency: {0}\n", tile.RelativeFrequency));
                text = string.Concat(text, "\tConstraints\n");
                foreach (Direction2D direction in tile.PossibleNeighbours.Keys)
                {
                    text = string.Concat(text, string.Format("\t\tDirection: {0}\n", direction));
                    text = string.Concat(text, "\t\tNeighbours:\n");
                    foreach (WFCTile possibleNeighbour in tile.PossibleNeighbours[direction])
                    {
                        text = string.Concat(text, string.Format("\t\t\tID: {0}\n", possibleNeighbour.Name));
                    }
                    text = string.Concat(text, "\n");
                }
            }

            return text;
        }
    }
}

