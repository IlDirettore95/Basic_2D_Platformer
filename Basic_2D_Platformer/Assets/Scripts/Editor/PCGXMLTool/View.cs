using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.Tools.XML
{
    public class View
    {
        public string Message = string.Empty;

        private Model _model;

        private int _windowWidth;
        private int _windowHeight;

        private Texture2D _hierarchyTexture;
        private Texture2D _currentChildrenTexture;
        private Texture2D _fileActionsTexture;
        private Texture2D _messageTexture;

        private Color _hierarchyColor = new Color(20f / 255f, 40f / 255f, 40f / 255f);
        private Color _currentChildrenColor = new Color(60f / 255f, 80f / 255f, 80f / 255f);
        private Color _fileActionsColor = new Color(40f / 255f, 60f / 255f, 60f / 255f);
        private Color _messageColor = new Color(120f / 255f, 40f / 255f, 40f / 255f);

        private Rect _hierarchyRect;
        private Rect _currentChildrenRect;
        private Rect _fileActionsRect;
        private Rect _messageRect;

        private Vector2 _currentChildrenScrollPosition;

        private const int PADDING_CONTENT = 8;
        private const int BUTTON_SIMPLE_WIDTH = 100;
        private const int BUTTON_FILE_ACTIONS_WIDTH = 120;
        private const int TEXTFIELDS_ATTRIBUTE_WIDTH = 30;
        private const int TEXTFIELDS_ATTRIBUTE_ID_WIDTH = 90;
        private const int POPUP_CONSTRAINT_WIDTH = 100;
        private const int TOGGLE_WIDTH = 10;

        #region Init

        public View(int windowWidth, int windowHeight)
        {
            _windowWidth = windowWidth;
            _windowHeight = windowHeight;
        }

        public void Init(Model model)
        {
            _model = model;

            _hierarchyTexture = new Texture2D(1, 1);
            _hierarchyTexture.SetPixel(0, 0, _hierarchyColor);
            _hierarchyTexture.Apply();

            _currentChildrenTexture = new Texture2D(1, 1);
            _currentChildrenTexture.SetPixel(0, 0, _currentChildrenColor);
            _currentChildrenTexture.Apply();

            _fileActionsTexture = new Texture2D(1, 1);
            _fileActionsTexture.SetPixel(0, 0, _fileActionsColor);
            _fileActionsTexture.Apply();

            _messageTexture = new Texture2D(1, 1);
            _messageTexture.SetPixel(0, 0, _messageColor);
            _messageTexture.Apply();

            _hierarchyRect.x = 0;
            _hierarchyRect.y = 0;
            _hierarchyRect.width = 1000;
            _hierarchyRect.height = 80;

            _currentChildrenRect.x = 0;
            _currentChildrenRect.y = 80;
            _currentChildrenRect.width = 840;
            _currentChildrenRect.height = 640;

            _fileActionsRect.x = 840;
            _fileActionsRect.y = 80;
            _fileActionsRect.width = 160;
            _fileActionsRect.height = 640;

            _messageRect.x = 0;
            _messageRect.y = 720;
            _messageRect.width = 1000;
            _messageRect.height = 80;
        }

        #endregion

        #region Draw

        #region Main

        public void Draw()
        {
            DrawLayout();
            DrawContent();
        }

        private void DrawLayout()
        {
            GUI.DrawTexture(_hierarchyRect, _hierarchyTexture);
            GUI.DrawTexture(_currentChildrenRect, _currentChildrenTexture);
            GUI.DrawTexture(_fileActionsRect, _fileActionsTexture);
            GUI.DrawTexture(_messageRect, _messageTexture);
        }

        private void DrawContent()
        {
            RectOffset padding = new RectOffset(PADDING_CONTENT, PADDING_CONTENT, PADDING_CONTENT, PADDING_CONTENT);
            Rect area = new Rect(padding.right, padding.top, _windowWidth - (padding.right + padding.left), _windowHeight - (padding.top + padding.bottom));

            GUILayout.BeginArea(area);

            DrawHierarchy();
            DrawCurrentChildren();
            DrawFileActions();
            DrawMessage();

            GUILayout.EndArea();
        }

        #endregion

        #region Hierarchy

        private void DrawHierarchy()
        {
            GUILayout.BeginArea(_hierarchyRect);
            GUILayout.Label("Hierarchy");
            GUILayout.BeginHorizontal();

            List<XmlNode> hierarchy = _model.GetHierarchy();
            for (int i = 0; i < hierarchy.Count; i++)
            {
                XmlNode element = hierarchy[i];

                if (Utils.HasNodeAttribute(element,"List") && !Utils.IsNodeNameEqual(element, "Levels")) continue;

                if (GUILayout.Button(Utils.GetHierarchyButtonText(element), GUILayout.ExpandWidth(false)))
                {
                    _model.HierarchyButtonClicked(i);
                }

                if (i == hierarchy.Count - 1) break;
                GUILayout.Label("->", GUILayout.ExpandWidth(false));
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion

        #region CurrentChildren

        private void DrawCurrentChildren()
        {
            GUILayout.BeginArea(_currentChildrenRect);
            GUILayout.BeginVertical();
            GUILayout.Label("CurrentChildren");
            _currentChildrenScrollPosition = GUILayout.BeginScrollView(_currentChildrenScrollPosition, GUILayout.Width(_currentChildrenRect.width - 10), GUILayout.Height(_currentChildrenRect.height - 40));
            XmlNode currentNode = _model.GetCurrentNode();
            List<XmlNode> childrenList = Utils.GetCurrentChildren(currentNode);
            for (int i = 0; i < childrenList.Count; i++) 
            { 
                GUILayout.BeginHorizontal();

                XmlNode child = childrenList[i];
                DrawChildName(child);
                DrawChildAttributes(child);
                DrawDeleting(child);

                GUILayout.EndHorizontal();

            }
            DrawAdding(currentNode);

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawChildName(XmlNode child)
        {
            string childText = Utils.GetCurrentChildString(child);
            if (Utils.HasNodeAttribute(child, "List"))
            {
                GUILayout.Label("-" + childText + "-");
            }
            else if (Utils.HasNodeAttribute(child, "Element"))
            {
                GUILayout.Label(childText, GUILayout.Width(BUTTON_SIMPLE_WIDTH));
            }
            else if (GUILayout.Button(childText, GUILayout.Width(BUTTON_SIMPLE_WIDTH)))
            {
                _model.CurrentChildrenButtonClicked(child);
            }
        }

        private void DrawChildAttributes(XmlNode child)
        {
            if (!Utils.HasNodeAttributes(child)) return;

            foreach (XmlAttribute attribute in child.Attributes)
            {
                if (Utils.IsAttributeNameEqual(attribute, "List")) continue;

                if (Utils.IsAttributeNameEqual(attribute, "Element")) continue;

                if (Utils.IsAttributeNameEqual(attribute, "Path"))
                {
                    string title = attribute.Value == "" ? "DragAndDrop" : attribute.Value;
                    object droppedObject = DropZone(title, title.Length * 8, 22);
                    if (!(droppedObject is GameObject)) continue;
                    if (droppedObject == null) continue;

                    attribute.Value = Utils.GetResourcesPath(AssetDatabase.GetAssetPath((GameObject)droppedObject));

                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Level") && Utils.IsAttributeNameEqual(attribute, "ID"))
                {
                    GUILayout.Space(15);
                    GUILayout.Label(attribute.Name, GUILayout.ExpandWidth(false));
                    attribute.Value = GUILayout.TextField(attribute.Value, GUILayout.Width(TEXTFIELDS_ATTRIBUTE_ID_WIDTH));
                    _model.EditLevel(child);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Tile") && Utils.IsAttributeNameEqual(attribute, "ID"))
                {
                    GUILayout.Space(15);
                    GUILayout.Label(attribute.Name, GUILayout.ExpandWidth(false));
                    attribute.Value = GUILayout.TextField(attribute.Value, GUILayout.Width(TEXTFIELDS_ATTRIBUTE_ID_WIDTH));
                    _model.EditTile(child);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Tile") && Utils.IsAttributeNameEqual(attribute, "RH"))
                {
                    GUILayout.Space(10);
                    GUILayout.Label("R (H-V)", GUILayout.ExpandWidth(false));
                    bool oldBool = _model.GetRHBool(child);
                    bool newBool = EditorGUILayout.Toggle(oldBool, GUILayout.Width(TOGGLE_WIDTH));
                    _model.SetRHBool(child, newBool);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Tile") && Utils.IsAttributeNameEqual(attribute, "RV"))
                {
                    GUILayout.Space(5);
                    bool oldBool = _model.GetRVBool(child);
                    bool newBool = EditorGUILayout.Toggle(oldBool, GUILayout.Width(TOGGLE_WIDTH));
                    _model.SetRVBool(child, newBool);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Tile") && Utils.IsAttributeNameEqual(attribute, "N_IN"))
                {
                    GUILayout.Space(10);
                    GUILayout.Label("N (I-O)", GUILayout.ExpandWidth(false));
                    bool oldBool = _model.GetNInBool(child);
                    bool newBool = EditorGUILayout.Toggle(oldBool, GUILayout.Width(TOGGLE_WIDTH));
                    _model.SetNInBool(child, newBool);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Tile") && Utils.IsAttributeNameEqual(attribute, "N_OUT"))
                {
                    GUILayout.Space(5);
                    bool oldBool = _model.GetNOutBool(child);
                    bool newBool = EditorGUILayout.Toggle(oldBool, GUILayout.Width(TOGGLE_WIDTH));
                    _model.SetNOutBool(child, newBool);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Tile") && Utils.IsAttributeNameEqual(attribute, "E_IN"))
                {
                    GUILayout.Space(10);
                    GUILayout.Label("E (I-O)", GUILayout.ExpandWidth(false));
                    bool oldBool = _model.GetEInBool(child);
                    bool newBool = EditorGUILayout.Toggle(oldBool, GUILayout.Width(TOGGLE_WIDTH));
                    _model.SetEInBool(child, newBool);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Tile") && Utils.IsAttributeNameEqual(attribute, "E_OUT"))
                {
                    GUILayout.Space(5);
                    bool oldBool = _model.GetEOutBool(child);
                    bool newBool = EditorGUILayout.Toggle(oldBool, GUILayout.Width(TOGGLE_WIDTH));
                    _model.SetEOutBool(child, newBool);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Tile") && Utils.IsAttributeNameEqual(attribute, "S_IN"))
                {
                    GUILayout.Space(10);
                    GUILayout.Label("S (I-O)", GUILayout.ExpandWidth(false));
                    bool oldBool = _model.GetSInBool(child);
                    bool newBool = EditorGUILayout.Toggle(oldBool, GUILayout.Width(TOGGLE_WIDTH));
                    _model.SetSInBool(child, newBool);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Tile") && Utils.IsAttributeNameEqual(attribute, "S_OUT"))
                {
                    GUILayout.Space(5);
                    bool oldBool = _model.GetSOutBool(child);
                    bool newBool = EditorGUILayout.Toggle(oldBool, GUILayout.Width(TOGGLE_WIDTH));
                    _model.SetSOutBool(child, newBool);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Tile") && Utils.IsAttributeNameEqual(attribute, "W_IN"))
                {
                    GUILayout.Space(10);
                    GUILayout.Label("W (I-O)", GUILayout.ExpandWidth(false));
                    bool oldBool = _model.GetWInBool(child);
                    bool newBool = EditorGUILayout.Toggle(oldBool, GUILayout.Width(TOGGLE_WIDTH));
                    _model.SetWInBool(child, newBool);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Tile") && Utils.IsAttributeNameEqual(attribute, "W_OUT"))
                {
                    GUILayout.Space(5);
                    bool oldBool = _model.GetWOutBool(child);
                    bool newBool = EditorGUILayout.Toggle(oldBool, GUILayout.Width(TOGGLE_WIDTH));
                    _model.SetWOutBool(child, newBool);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Constraint") && Utils.IsAttributeNameEqual(attribute, "Type"))
                {
                    GUILayout.Space(15);
                    string[] choices = _model.GetConstraintChoices();
                    int oldIndex = _model.GetConstraintIndex(child);
                    int newIndex = EditorGUILayout.Popup(oldIndex, choices, GUILayout.Width(POPUP_CONSTRAINT_WIDTH));
                    if (newIndex == oldIndex) continue;
                    _model.EditConstraint(child, newIndex, oldIndex);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Neighbour") && Utils.IsAttributeNameEqual(attribute, "ID"))
                {
                    GUILayout.Space(15);
                    string[] choices = _model.GetNeighbourChoices();
                    int oldIndex = _model.GetNeighbourIndex(child);
                    int newIndex = EditorGUILayout.Popup(oldIndex, choices, GUILayout.Width(TEXTFIELDS_ATTRIBUTE_ID_WIDTH));
                    if (newIndex == oldIndex) continue;
                    _model.EditNeighbour(child, newIndex, oldIndex);
                    continue;
                }

                GUILayout.Space(15);
                GUILayout.Label(attribute.Name, GUILayout.ExpandWidth(false));
                attribute.Value = GUILayout.TextField(attribute.Value, GUILayout.Width(TEXTFIELDS_ATTRIBUTE_WIDTH));
            }
        }

        public object DropZone(string title, int w, int h)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            Rect dropArea = new Rect(lastRect.x + lastRect.width + 20, lastRect.y, w, h);

            GUI.Box(dropArea, title);
            GUILayout.Label("", GUILayout.Width(dropArea.width + 20), GUILayout.Height(dropArea.height));

            EventType eventType = Event.current.type;
            bool isAccepted = false;

            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                if (dropArea.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }
                if (eventType == EventType.DragPerform && dropArea.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.AcceptDrag();
                    isAccepted = true;
                }
                Event.current.Use();
            }
            return isAccepted ? DragAndDrop.objectReferences[0] : null;
        }

        private void DrawDeleting(XmlNode child)
        {
            if (!Utils.HasNodeAttribute(child.ParentNode, "List")) return;

            if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
            {
                _model.DeleteNode(child.ParentNode, child, _model.GetCurrentLevel(), _model.GetCurrentTile(), _model.GetCurrentConstraint());
            }
        }

        private void DrawAdding(XmlNode currentNode)
        {
            foreach (XmlNode childNode in currentNode.ChildNodes)
            {
                if (!Utils.HasNodeAttribute(childNode, "List") || Utils.IsNodeNameEqual(childNode,"Settings")) continue;
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                {
                    _model.CreateNode(childNode, _model.GetCurrentLevel(), _model.GetCurrentTile(), _model.GetCurrentConstraint());
                }
            }
        }

        #endregion

        #region FileActions

        private void DrawFileActions()
        {
            GUILayout.BeginArea(_fileActionsRect);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();

            if (GUILayout.Button("Reload", GUILayout.Width(BUTTON_FILE_ACTIONS_WIDTH)))
            {
                _model.Reload();
            }

            if (GUILayout.Button("Validate", GUILayout.Width(BUTTON_FILE_ACTIONS_WIDTH)))
            {
                _model.Validate();
            }

            if (GUILayout.Button("Save", GUILayout.Width(BUTTON_FILE_ACTIONS_WIDTH)))
            {
                _model.Save();
            }

            if (GUILayout.Button("Validate and Save", GUILayout.Width(BUTTON_FILE_ACTIONS_WIDTH)))
            {
                _model.ValidateAndSave();
            }

            if (GUILayout.Button("Debug", GUILayout.Width(BUTTON_FILE_ACTIONS_WIDTH)))
            {
                Debug.Log(_model.PrintDebug());
            }

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        #endregion

        #region Message

        private void DrawMessage()
        {
            GUILayout.BeginArea(_messageRect);
            GUILayout.Label(Message);
            GUILayout.EndArea();
        }

        #endregion

        #endregion
    }
}