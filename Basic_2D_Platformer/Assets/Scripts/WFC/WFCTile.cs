using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GMDG.NoProduct.Utility.Utility2D;

public class WFCTile
{
    public string Name;
    public GameObject Prefab;
    public float RelativeFrequency;
    public Dictionary<Direction2D, HashSet<int>> PossibleNeighbours = new Dictionary<Direction2D, HashSet<int>>()
    {
        { Direction2D.NORTH,  new HashSet<int>()},
        { Direction2D.EAST,  new HashSet<int>()},
        { Direction2D.SOUTH,  new HashSet<int>()},
        { Direction2D.WEST,  new HashSet<int>()},
    };
}
