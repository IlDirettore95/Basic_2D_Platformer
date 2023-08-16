using GMDG.NoProduct.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PCG.WFC
{
    [CreateAssetMenu(fileName = "Tile", menuName = "ScriptableObjects/Tile")]
    public class TileScriptableObject : ScriptableObject
    {
        public GameObject Prefab;
        public int Weight;
        //public List<Constraint> Constraints;
    }

    //[Serializable]
    //public class Constraint
    //{
    //    public Utility2D.Direction2D Direction;
    //    public List<TileScriptableObject> PossibleNeighbours;
    //}
}
