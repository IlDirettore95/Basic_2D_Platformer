using System.Xml;
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer.Explorer;
using System.Linq;
using Unity.VisualScripting;

public class PCGXMLEditor : EditorWindow
{
    // Graphics
    private const int PADDING = 8;

    private Texture2D _headerTexture;
    private Color _headerColor = new Color(10f / 255f, 10f / 255f, 10f / 255f);
    private Rect _headerRect;

    private Texture2D _bodyTexture;
    private Color _bodyColor = new Color(50f / 255f, 50f / 255f, 50f / 255f);
    private Rect _bodyRect;

    private Texture2D _messageTexture;
    private Color _messageColor = new Color(150f / 255f, 50f / 255f, 50f / 255f);
    private Rect _messageRect;

    private Vector2 _currentHierarchyScrollPosition;
    private Vector2 _currentChildrenScrollPosition;

    // Texts
    private string _path = "XML/WorldGenerationData";
    private string _message = string.Empty;

    // DataStructure
    private XmlDocument _xmlDocument;
    private List<object> _currentHierarchy;
    private XmlNode _currentLevel;
    private XmlNode _currentTile;
    private Dictionary<XmlNode, List<XmlNode>> _tilesPerLevel;
    private Dictionary<XmlNode, Dictionary<XmlNode, int>> _popupIndexesPerNeighbour;

    [MenuItem("Window/PCG/XMLData")]
    private static void Init()
    {
        PCGXMLEditor window = GetWindow<PCGXMLEditor>(true, "XML Data", true);
        window.minSize = new Vector2(800, 400);
        window.maxSize = new Vector2(800, 400);
    }

    private void OnEnable()
    {
        InitTextures();
        InitXml();
        InitDataStructures();
    }

    private void OnGUI()
    {
        DrawLayout();
        DrawContent();
    }

    #region Init

    private void InitTextures()
    {
        _headerTexture = new Texture2D(1, 1);
        _headerTexture.SetPixel(0, 0, _headerColor);
        _headerTexture.Apply();

        _bodyTexture = new Texture2D(1, 1);
        _bodyTexture.SetPixel(0, 0, _bodyColor);
        _bodyTexture.Apply();

        _messageTexture = new Texture2D(1, 1);
        _messageTexture.SetPixel(0, 0, _messageColor);
        _messageTexture.Apply();
    }

    private void InitXml()
    {
        TextAsset textAsset = Resources.Load<TextAsset>(_path);

        if (textAsset == null)
        {
            _message = string.Format("File {0} not found!", _path);
            return;
        }

        _xmlDocument = new XmlDocument();
        _xmlDocument.LoadXml(textAsset.text);

        _message = string.Format("File {0} loaded!", _path);
    }

    private void InitDataStructures()
    {
        // Current Hierarchy
        _currentHierarchy = new List<object>
        {
            _xmlDocument.SelectNodes("/Levels")[0]
        };

        // CurrentTilesPerLevel
        _tilesPerLevel = new Dictionary<XmlNode, List<XmlNode>>();
        
        foreach (XmlNode level in _xmlDocument.SelectNodes("/Levels/Level"))
        {
            _tilesPerLevel[level] = new List<XmlNode>();
            XmlNodeList tilesList = level["WFC"]["Tiles"].SelectNodes("Tile");
            
            foreach (XmlNode tile in tilesList) 
            {
                _tilesPerLevel[level].Add(tile);
            }
        }

        // PopupIndexesPerNeighbour
        _popupIndexesPerNeighbour = new Dictionary<XmlNode, Dictionary<XmlNode, int>>();

        foreach (XmlNode level in _xmlDocument.SelectNodes("/Levels/Level"))
        {
            _popupIndexesPerNeighbour[level] = new Dictionary<XmlNode, int>();
            XmlNodeList tilesList = level["WFC"]["Tiles"].SelectNodes("Tile");

            foreach (XmlNode tile in tilesList)
            {
                XmlNode constraintsNode = tile["Constraints"];
                XmlNodeList constraintsList = constraintsNode.SelectNodes("Constraint");

                foreach (XmlNode contraint in constraintsList)
                {
                    XmlNodeList neighbours = contraint["Neighbours"].ChildNodes;

                    foreach (XmlNode neighbour in neighbours)
                    {
                        string neighbourId = neighbour.Attributes["ID"].Value;
                        _popupIndexesPerNeighbour[level][neighbour] = _tilesPerLevel[level].IndexOf(_tilesPerLevel[level].First(n => n.Attributes["ID"].Value.Equals(neighbourId)));
                    }
                }
            }
        }
    }

    #endregion

    #region Graphics

    private void DrawLayout()
    {
        _headerRect.x = 20;
        _headerRect.y = 0;
        _headerRect.width = Screen.width - 40;
        _headerRect.height = 70;
        GUI.DrawTexture(_headerRect, _headerTexture);

        _bodyRect.x = 20;
        _bodyRect.y = 70;
        _bodyRect.width = Screen.width - 40;
        _bodyRect.height = Screen.height - 50;
        GUI.DrawTexture(_bodyRect, _bodyTexture);

        _messageRect.x = 0;
        _messageRect.y = Screen.height - 50;
        _messageRect.width = Screen.width;
        _messageRect.height = 50;
        GUI.DrawTexture(_messageRect, _messageTexture);
    }

    private void DrawContent()
    {
        RectOffset padding = new RectOffset(PADDING, PADDING, PADDING, PADDING);
        Rect area = new Rect(padding.right, padding.top, position.width - (padding.right + padding.left), position.height - (padding.top + padding.bottom));

        GUILayout.BeginArea(area);

        DrawHeader();
        DrawBody();
        DrawMessage();

        GUILayout.EndArea();
    }

    private void DrawHeader()
    {
        GUILayout.BeginArea(_headerRect);
        GUILayout.Label("Current hierarchy:");
        DrawHierarchyScrollBar();
        GUILayout.EndArea();
    }

    private void DrawBody()
    {
        GUILayout.BeginArea(_bodyRect);
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label("Current children:");
        DrawChildrenScrollBar();
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        DrawOtherActions();
        GUILayout.EndVertical();
        GUILayout.Space(80);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void DrawMessage()
    {
        GUILayout.BeginArea(_messageRect);
        GUILayout.Space(10);
        GUILayout.Label(_message);
        GUILayout.EndArea();
    }

    private void DrawHierarchyScrollBar()
    {
        _currentHierarchyScrollPosition = GUILayout.BeginScrollView(_currentHierarchyScrollPosition, GUILayout.Width(_headerRect.width - 20), GUILayout.Height(_headerRect.height - 30));
        DrawHierarchy();
        GUILayout.EndScrollView();
    }

    private void DrawChildrenScrollBar()
    {
        _currentChildrenScrollPosition = GUILayout.BeginScrollView(_currentChildrenScrollPosition, GUILayout.Width(Screen.width - 200), GUILayout.Height(_bodyRect.height - 110));
        DrawCurrentChildren();
        DrawPossibleActions();
        GUILayout.EndScrollView();
    }
    
    private void DrawHierarchy()
    {
        GUILayout.BeginHorizontal();

        for (int i = 0; i < _currentHierarchy.Count; i++) 
        { 
            object element = _currentHierarchy[i];
            UpdateData(element);
            string buttonText = TakeString(element);

            if (GUILayout.Button(buttonText, GUILayout.ExpandWidth(false)))
            {
                for (int j = i + 1; j <  _currentHierarchy.Count;)
                {
                    _currentHierarchy.RemoveAt(j);
                }
            }

            if (i == _currentHierarchy.Count - 1) break;
            GUILayout.Label("->", GUILayout.ExpandWidth(false));
        }

        GUILayout.EndHorizontal();
    }

    private void DrawCurrentChildren()
    {
        object lastElement = _currentHierarchy[_currentHierarchy.Count - 1];

        XmlNodeList currentNodeList = TakeChildren(lastElement);

        if (currentNodeList == null) return;

        foreach (XmlNode node in currentNodeList) 
        {
            string buttonText = TakeString(node);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(buttonText, GUILayout.ExpandWidth(false)))
            {
                _currentHierarchy.Add(node);
            }

            if (node.Attributes.Count > 0) 
            { 
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (node.Name.Equals("Neighbour") && attribute.Name.Equals("ID"))
                    {
                        int oldIndex = _popupIndexesPerNeighbour[_currentLevel][node];
                        int newIndex = EditorGUILayout.Popup(oldIndex, GetTilesIDs(_tilesPerLevel[_currentLevel]), GUILayout.ExpandWidth(false));
                        _popupIndexesPerNeighbour[_currentLevel][node] = newIndex;

                        if (newIndex < 0) continue;

                        node.Attributes["ID"].Value = _tilesPerLevel[_currentLevel][newIndex].Attributes["ID"].Value;
                        continue;
                    }

                    if (attribute.Name.Equals("Path"))
                    {
                        string title = attribute.Value == "" ? "DragAndDrop" : attribute.Value;
                        object[] objects = DropZone(title, title.Length * 8, 20);

                        if (objects == null) continue;

                        foreach (object obj in objects) 
                        {
                            if (obj is GameObject) attribute.Value = AssetDatabase.GetAssetPath((GameObject)obj);
                        }

                        continue;
                    }

                    GUILayout.Label(attribute.Name, GUILayout.ExpandWidth(false));
                    attribute.Value = GUILayout.TextField(attribute.Value, GUILayout.Width(60));
                    GUILayout.Space(15);
                }
            }

            GUILayout.EndHorizontal();
        }
    }
    
    private void DrawPossibleActions()
    {
        XmlNode node = _currentHierarchy[_currentHierarchy.Count - 1] as XmlNode;

        if (node == null) return;

        switch (node.Name)
        {
            case "Levels":
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                {
                    node.AppendChild(CreateLevelNode());
                }
                break;
            case "Tiles":
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                {
                    node.AppendChild(CreateTileNode());
                }
                break;
            case "Constraints":
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                {
                    node.AppendChild(CreateConstraintNode());
                }
                break;
            case "Neighbours":
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                {
                    node.AppendChild(CreateNeighbourNode());
                }
                break;
        }
    }

    private void DrawOtherActions()
    {
        if (GUILayout.Button("Reload XML file", GUILayout.ExpandWidth(false)))
        {
            AssetDatabase.Refresh();
            InitXml();
            InitDataStructures();
        }

        if (GUILayout.Button("Save XML file", GUILayout.ExpandWidth(false)))
        {
            _xmlDocument.Save(Application.dataPath + "/Resources/" + _path + ".xml");
        }
    }

    public object[] DropZone(string title, int w, int h)
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
        return isAccepted ? DragAndDrop.objectReferences : null;
    }

    #endregion

    #region ReadXml

    private void UpdateData(object element)
    {
        XmlNode node = element as XmlNode;

        if (node == null) return;

        switch (node.Name)
        {
            case "Level":
                _currentLevel = node;
                break;
            case "Tile":
                _currentTile = node;
                break;
        }
    }

    private string TakeString(object element)
    {
        string text = string.Empty;

        XmlNode node = element as XmlNode;

        if (node == null) return text;
            
        text += node.Name;

        if (node.Attributes.Count <= 0) return text;
        if (node.Attributes["Path"] != null) return text;

        text += " " + node.Attributes[0].Value;

        return text;
    }

    private string[] GetTilesIDs(List<XmlNode> list)
    {
        string[] ids = new string[list.Count];

        for (int i = 0; i < list.Count; i++)
        {
            ids[i] = list[i].Attributes["ID"].Value;
        }

        return ids;
    }

    private XmlNodeList TakeChildren(object element)
    {
        XmlNode node = element as XmlNode;

        if (node != null) return node.ChildNodes;

        return null;    
    }

    #endregion

    #region WriteXml

    private XmlElement CreateLevelNode()
    {
        XmlElement level = _xmlDocument.CreateElement("Level");

        XmlAttribute id = _xmlDocument.CreateAttribute("ID");

        level.Attributes.Append(id);
        level.AppendChild(CreateSettingsNode());
        level.AppendChild(CreateWFCNode());

        return level;
    }

    private XmlElement CreateTileNode()
    {
        XmlElement tile = _xmlDocument.CreateElement("Tile");

        XmlAttribute id = _xmlDocument.CreateAttribute("ID");
        XmlAttribute frequency = _xmlDocument.CreateAttribute("Frequency");

        tile.Attributes.Append(id);
        tile.Attributes.Append(frequency);

        XmlElement prefab = _xmlDocument.CreateElement("Prefab");
        XmlElement constraints = _xmlDocument.CreateElement("Constraints");

        XmlAttribute path = _xmlDocument.CreateAttribute("Path");
        prefab.Attributes.Append(path);

        tile.AppendChild(prefab);
        tile.AppendChild(constraints);

        _tilesPerLevel[_currentLevel].Add(tile);

        return tile;
    }

    private XmlElement CreateConstraintNode()
    {
        XmlElement constraint = _xmlDocument.CreateElement("Constraint");

        XmlAttribute direction = _xmlDocument.CreateAttribute("Direction");
        XmlElement neighbours = _xmlDocument.CreateElement("Neighbours");

        constraint.Attributes.Append(direction);
        constraint.AppendChild(neighbours);

        return constraint;
    }

    private XmlElement CreateNeighbourNode()
    {
        XmlElement neighbour = _xmlDocument.CreateElement("Neighbour");

        XmlAttribute id = _xmlDocument.CreateAttribute("ID");
        neighbour.Attributes.Append(id);

        _popupIndexesPerNeighbour[_currentLevel][neighbour] = -1;

        return neighbour;
    }

    private XmlNode CreateSettingsNode()
    {
        XmlElement setting = _xmlDocument.CreateElement("Settings");

        setting.AppendChild(CreateVector2Element("GridSize"));
        setting.AppendChild(CreateVector2Element("CellSize"));
        setting.AppendChild(CreateVector2Element("StartingCell"));
        setting.AppendChild(CreateVector2Element("EndingCell"));

        return setting;
    }

    private XmlNode CreateWFCNode()
    {
        XmlElement wfc = _xmlDocument.CreateElement("WFC");

        wfc.AppendChild(_xmlDocument.CreateElement("Tiles"));

        return wfc;
    }

    private XmlElement CreateVector2Element(string name)
    {
        XmlElement vector2 = _xmlDocument.CreateElement(name);

        XmlAttribute x = _xmlDocument.CreateAttribute("x");
        XmlAttribute y = _xmlDocument.CreateAttribute("y");

        x.Value = "0";
        y.Value = "0";

        vector2.Attributes.Append(x);
        vector2.Attributes.Append(y);

        return vector2;
    }

    #endregion
}