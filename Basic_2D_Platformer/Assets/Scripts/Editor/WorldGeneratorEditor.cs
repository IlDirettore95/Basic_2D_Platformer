using UnityEditor;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PCG.WFC
{
#if UNITY_EDITOR
    [CustomEditor(typeof(WorldGenerator))]
    public class WorldGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            WorldGenerator generator = (WorldGenerator)target;
            if (GUILayout.Button("GenerateMap"))
            {
                generator.Initialize();
                generator.Generate();
            }
            if (GUILayout.Button("DeleteMap"))
            {
                generator.Delete();
            }
        }
    }
#endif
}


