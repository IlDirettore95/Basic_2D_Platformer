using UnityEditor;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PCG.WFC
{
    public class WorldGenerator : MonoBehaviour
    {
        public int Seed = 0;
        public int IterationsLimit = -1;

        private IWFCModel _model;

        public void Generate()
        {
            Delete();

            WFCData data = new WFCData();
            data.ReadData();
            Debug.Log(data);

            UnityEngine.Random.InitState(Seed <= 0 ? UnityEngine.Random.Range(0, int.MaxValue) : Seed);

            _model = new EvenSimplerTiledModel(this, data, IterationsLimit);
            _model.Generate();
        }

        public void Delete()
        {
            _model = null;
            for (int i = 0; i < gameObject.transform.childCount;)
            {
    #if UNITY_EDITOR
                DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
#else
                Destroy(gameObject.transform.GetChild(i).gameObject);
#endif
            }
            StopAllCoroutines();
        }

        #region EditorCallbacks

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            _model?.Draw();
        }
#endif

        #endregion
    }
}
