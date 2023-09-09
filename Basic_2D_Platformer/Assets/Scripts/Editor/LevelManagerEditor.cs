using GMDG.Basic2DPlatformer.PCG.WFC;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace GMDG.Basic2DPlatformer.System
{
    [CustomEditor(typeof(LevelManager))]
    public class LevelManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            LevelManager generator = (LevelManager)target;
            if (GUILayout.Button("Generate"))
            {
                generator.GenerateLevel();
            }
            if (GUILayout.Button("Delete"))
            {
                //generator.Delete();
            }
        }
    }

}
#endif
