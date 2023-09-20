using UnityEditor;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.Tools.XML
{
    public class XMLDataEditor : EditorWindow
    {
        private Model _model;
        private View _view;

        private const int WINDOW_WIDTH = 800;
        private const int WINDOW_HEIGHT = 400;

        [MenuItem("Window/PCG/XMLDataEditor")]
        private static void Init()
        {
            XMLDataEditor window = GetWindow<XMLDataEditor>(true, "XML Data Editor", true);
            window.minSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
            window.maxSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
        }

        private void OnEnable()
        {
            _model = new Model();
            _view = new View(WINDOW_WIDTH, WINDOW_HEIGHT);

            _model.Init(_view);
            _view.Init(_model);
        }

        private void OnGUI()
        {
            _view.Draw();
        }
    }
}
