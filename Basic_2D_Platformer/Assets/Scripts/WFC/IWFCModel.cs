using GMDG.NoProduct.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PCG.WFC
{
    public interface IWFCModel
    {
        public IEnumerator Generate();
        public void Draw();
    }
}
