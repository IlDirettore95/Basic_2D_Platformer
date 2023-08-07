using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PCG.WFC
{
    [CreateAssetMenu(fileName = "Tile", menuName = "ScriptableObjects/Tile")]
    public class TileScriptableObject : ScriptableObject
    {
        public int ID;
        public GameObject Prefab;
        public int Weight;
        public List<TileScriptableObject> PossibleNeighbours;
    }
}
