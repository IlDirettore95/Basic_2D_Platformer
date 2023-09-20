using System;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Metadata;

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

        private Vector2 _currentHierarchyScrollPosition;
        private Vector2 _currentChildrenScrollPosition;

        private const int PADDING_CONTENT = 8;
        private const int BUTTON_SIMPLE_WIDTH = 100;
        private const int BUTTON_FILE_ACTIONS_WIDTH = 120;
        private const int TEXTFIELDS_ATTRIBUTE_WIDTH = 80;

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
            _hierarchyRect.width = 800;
            _hierarchyRect.height = 80;

            _currentChildrenRect.x = 0;
            _currentChildrenRect.y = 80;
            _currentChildrenRect.width = 600;
            _currentChildrenRect.height = 240;

            _fileActionsRect.x = 600;
            _fileActionsRect.y = 80;
            _fileActionsRect.width = 200;
            _fileActionsRect.height = 240;

            _messageRect.x = 0;
            _messageRect.y = 320;
            _messageRect.width = 800;
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

            XmlNode currentNode = _model.GetCurrentNode();
            List<XmlNode> childrenList = Utils.GetCurrentChildren(currentNode);
            for (int i = 0; i < childrenList.Count; i++) 
            { 
                GUILayout.BeginHorizontal();

                XmlNode child = childrenList[i];
                DrawChildName(child);
                DrawChildAttributes(child);
                DrawChildDeleting(child);

                GUILayout.EndHorizontal();

            }
            DrawAdding(currentNode);

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

                if (Utils.IsNodeNameEqual(child, "Constraint") && Utils.IsAttributeNameEqual(attribute, "Type"))
                {
                    string[] choices = _model.GetConstraintChoices();
                    int oldIndex = _model.GetConstraintIndex(child);
                    int newIndex = EditorGUILayout.Popup(oldIndex, choices, GUILayout.ExpandWidth(false));
                    _model.SetConstraintIndex(child, newIndex);

                    if (newIndex < 0) continue;

                    _model.SetConstraintValue(child, newIndex);
                    continue;
                }

                if (Utils.IsNodeNameEqual(child, "Neighbour") && Utils.IsAttributeNameEqual(attribute, "ID"))
                {
                    string[] choices = _model.GetNeighbourChoices();
                    int oldIndex = _model.GetNeighbourIndex(child);
                    int newIndex = EditorGUILayout.Popup(oldIndex, choices, GUILayout.ExpandWidth(false));
                    _model.SetNeighbourIndex(child, newIndex);

                    if (newIndex < 0) continue;

                    _model.SetNeighbourValue(child, newIndex);
                    continue;
                }
                //if (child.Name.Equals("Neighbour") && attribute.Name.Equals("ID"))
                //{
                //    int oldIndex = _popupIndexesPerNeighbour[_currentLevel][child];
                //   int newIndex = EditorGUILayout.Popup(oldIndex, GetTilesIDs(_tilesPerLevel[_currentLevel]), GUILayout.ExpandWidth(false));
                //    _popupIndexesPerNeighbour[_currentLevel][child] = newIndex;

                //    if (newIndex < 0) continue;

                //    child.Attributes["ID"].Value = _tilesPerLevel[_currentLevel][newIndex].Attributes["ID"].Value;

                //    if (newIndex != oldIndex) UpdateMutualTilesConstraint(oldIndex, newIndex, child);

                //    continue;
                //}

                //if (attribute.Name.Equals("Type"))
                //{
                //    int oldIndex = _popupIndexesPerConstraint[_currentLevel][child];
                //    int newIndex = EditorGUILayout.Popup(oldIndex, _possibleConstraints, GUILayout.ExpandWidth(false));
                //    _popupIndexesPerConstraint[_currentLevel][child] = newIndex;

                //    if (newIndex < 0) continue;

                //    if (newIndex != oldIndex) UpdateMutualTilesConstraint(oldIndex, newIndex, child);

                //    child.Attributes["Type"].Value = _possibleConstraints[newIndex];
                //    continue;
                //}

                GUILayout.Space(20);
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

        private void DrawChildDeleting(XmlNode child)
        {
            if (!Utils.HasNodeAttribute(child.ParentNode, "List")) return;

            if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
            {
                _model.DeleteNode(child.ParentNode, child);
            }
        }

        private void DrawAdding(XmlNode currentNode)
        {
            foreach (XmlNode childNode in currentNode.ChildNodes)
            {
                if (!Utils.HasNodeAttribute(childNode, "List")) continue;
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                {
                    _model.CreateNode(childNode);
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
                Debug.Log(_model.Debug());
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