using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static GMDG.NoProduct.Utility.Utility2D;

public class WFCTile
{
    public string Name;
    public GameObject Prefab;
    public float RelativeFrequency;
    public float Log2RelativeFrequency;

    public bool NIn;    
    public bool NOut;
    public bool EIn;    
    public bool EOut;
    public bool SIn;    
    public bool SOut;
    public bool WIn;    
    public bool WOut;

    public Dictionary<Direction2D, HashSet<int>> PossibleNeighbours = new Dictionary<Direction2D, HashSet<int>>()
    {
        { Direction2D.NORTH,  new HashSet<int>()},
        { Direction2D.EAST,  new HashSet<int>()},
        { Direction2D.SOUTH,  new HashSet<int>()},
        { Direction2D.WEST,  new HashSet<int>()},
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
