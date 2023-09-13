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
            LevelManager manager = (LevelManager)target;
            if (GUILayout.Button("Generate"))
            {
                manager.GenerateLevel();
            }
            if (GUILayout.Button("Delete"))
            {
                manager.Delete();
            }
        }
    }

}
#endif