using System.Collections.Generic;
using UnityEngine;
using static GMDG.NoProduct.Utility.Utility2D;

public class WFCTile
{
    public const int N_IN = 0;
    public const int N_OUT = 1;
    public const int E_IN = 2;
    public const int E_OUT = 3;
    public const int S_IN = 4;
    public const int S_OUT = 5;
    public const int W_IN = 6;
    public const int W_OUT = 7;

    public string Name;
    public GameObject Prefab;
    public float RelativeFrequency;
    public float Log2RelativeFrequency;
    public bool[] PassabilityFlags = new bool[8];

    public Dictionary<Direction2D, HashSet<int>> PossibleNeighbours = new Dictionary<Direction2D, HashSet<int>>()
    {
        { Direction2D.NORTH,  new HashSet<int>() },
        { Direction2D.EAST,  new HashSet<int>() },
        { Direction2D.SOUTH,  new HashSet<int>() },
        { Direction2D.WEST,  new HashSet<int>() },
    };

    public override string ToString()
    {
        string text = string.Empty;

        text = string.Concat(text, string.Format("\tName: {0}\n", Name));
        text = string.Concat(text, string.Format("\tRelative Frequency: {0}\n", RelativeFrequency));
        text = string.Concat(text, "\tConstraints\n");
        foreach (Direction2D direction in PossibleNeighbours.Keys)
        {
            text = string.Concat(text, string.Format("\t\tDirection: {0}\n", direction));
            text = string.Concat(text, "\t\tNeighbours:\n");
            foreach (int possibleNeighbour in PossibleNeighbours[direction])
            {
                text = string.Concat(text, string.Format("\t\t\tID: {0}\n", possibleNeighbour));
            }
            text = string.Concat(text, "\n");
        }

        return text;
    }
}
