using GMDG.Basic2DPlatformer.Utility;
using System.Collections.Generic;
using UnityEngine;
using static GMDG.NoProduct.Utility.Utility2D;

namespace GMDG.Basic2DPlatformer.PCG
{
    public class PCGData
    {
        public Vector2Int GridSize;
        public Vector2 CellSize;
        public Vector2Int StartingCell;
        public Vector2Int EndingCell;
        public Vector2Int PassageCell;
        public Grid<HashSet<int>> Grid;
        public List<Vector2Int> FeasiblePath;
        public List<WFCTile> WFCTiles = new List<WFCTile>();

        public const int START_CELL = 0;
        public const int END_CELL = 1;
        public const int PASSAGE_CELL = 2;

        public override string ToString()
        {
            string text = string.Empty;

            text = string.Concat(text, "Settings\n");
            text = string.Concat(text, string.Format("\tGridSize: {0}\n", GridSize));
            text = string.Concat(text, string.Format("\tCellSize: {0}\n", CellSize));
            text = string.Concat(text, string.Format("\tStartingCell: {0}\n", StartingCell));
            text = string.Concat(text, string.Format("\tEndingCell: {0}\n", EndingCell));
            text = string.Concat(text, string.Format("\tPassageCell: {0}\n", PassageCell));
            text = string.Concat(text, "Tiles\n");

            for (int i = 0; i < WFCTiles.Count; i++)
            {
                WFCTile tile = WFCTiles[i];

                text = string.Concat(text, string.Format("\t ({0}) {1}\n", i, tile.Name));
            }

            for (int i = 0; i < WFCTiles.Count; i++)
            {
                WFCTile tile = WFCTiles[i];

                text = string.Concat(text, string.Format("\tName: {0}\n", tile.Name));
                text = string.Concat(text, string.Format("\tFlags:\tN(I-O): {0},{1}  E(I-O): {2},{3}  S(I-O): {4},{5}  W(I-O): {6},{7}\n", tile.PassabilityFlags[WFCTile.N_IN], tile.PassabilityFlags[WFCTile.N_OUT], tile.PassabilityFlags[WFCTile.E_IN], tile.PassabilityFlags[WFCTile.E_OUT], tile.PassabilityFlags[WFCTile.S_IN], tile.PassabilityFlags[WFCTile.S_OUT], tile.PassabilityFlags[WFCTile.W_IN], tile.PassabilityFlags[WFCTile.W_OUT]));
                text = string.Concat(text, string.Format("\tRelative Frequency: {0}\n", tile.RelativeFrequency));
                text = string.Concat(text, "\tConstraints\n");
                foreach (Direction2D direction in tile.PossibleNeighbours.Keys)
                {
                    text = string.Concat(text, string.Format("\t\tDirection: {0}\n", direction));
                    text = string.Concat(text, "\t\tNeighbours:\n");
                    foreach (int possibleNeighbour in tile.PossibleNeighbours[direction])
                    {
                        text = string.Concat(text, string.Format("\t\t\tID: {0}\n", WFCTiles[possibleNeighbour].Name));
                    }
                    text = string.Concat(text, "\n");
                }
            }

            return text;
        }
    }
}

