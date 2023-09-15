using System.Xml;
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer.Explorer;
using System.Linq;
using Unity.VisualScripting;
using System.Xml.Serialization;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.UIElements;
using GMDG.Basic2DPlatformer.Utility;
using System.IO;

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
    private Dictionary<XmlNode, Dictionary<XmlNode, int>> _popupIndexesPerConstraint;
    private string[] _possibleDirection = { "ALL", "NORTH", "EAST", "SOUTH", "WEST", "HORIZONTAL", "VERTICAL", "N_NORTH", "N_EAST", "N_SOUTH", "N_WEST"};

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

                foreach (XmlNode constraint in constraintsList)
                {
                    XmlNodeList neighbours = constraint["Neighbours"].ChildNodes;

                    foreach (XmlNode neighbour in neighbours)
                    {
                        string neighbourId = neighbour.Attributes["ID"].Value;
                        _popupIndexesPerNeighbour[level][neighbour] = _tilesPerLevel[level].IndexOf(_tilesPerLevel[level].First(n => n.Attributes["ID"].Value.Equals(neighbourId)));
                    }
                }
            }
        }

        // PopupIndexesPerConstraint
        _popupIndexesPerConstraint = new Dictionary<XmlNode, Dictionary<XmlNode, int>>();

        foreach (XmlNode level in _xmlDocument.SelectNodes("/Levels/Level"))
        {
            _popupIndexesPerConstraint[level] = new Dictionary<XmlNode, int>();
            XmlNodeList tilesList = level["WFC"]["Tiles"].SelectNodes("Tile");

            foreach (XmlNode tile in tilesList)
            {
                XmlNode constraintsNode = tile["Constraints"];
                XmlNodeList constraintsList = constraintsNode.SelectNodes("Constraint");

                foreach (XmlNode constraint in constraintsList)
                {
                    string direction = constraint.Attributes["Direction"].Value;
                    for (int i = 0; i < _possibleDirection.Length; i++)
                    {
                        if (_possibleDirection[i].Equals(direction)) continue;
                        _popupIndexesPerConstraint[level][constraint] = i;
                        break;
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

                    if (attribute.Name.Equals("Direction"))
                    {
                        int oldIndex = _popupIndexesPerConstraint[_currentLevel][node];
                        int newIndex = EditorGUILayout.Popup(oldIndex, _possibleDirection, GUILayout.ExpandWidth(false));
                        _popupIndexesPerConstraint[_currentLevel][node] = newIndex;

                        if (newIndex < 0) continue;

                        node.Attributes["Direction"].Value = _possibleDirection[newIndex];
                        continue;
                    }

                    if (attribute.Name.Equals("Path"))
                    {
                        string title = attribute.Value == "" ? "DragAndDrop" : attribute.Value;
                        object[] objects = DropZone(title, title.Length * 8, 20);

                        if (objects == null) continue;

                        foreach (object obj in objects) 
                        {
                            if (obj is GameObject)
                            {
                                string path = AssetDatabase.GetAssetPath((GameObject)obj);
                                path = path.Substring(path.IndexOf("/") + 1);
                                path = path.Substring(path.IndexOf("/") + 1);
                                path = path.Substring(0, path.LastIndexOf("."));
                                attribute.Value = path;
                            }
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

        if (GUILayout.Button("Validate XML file", GUILayout.ExpandWidth(false)))
        {
            ValidateXmlDocument();
        }

        if (GUILayout.Button("Save XML file", GUILayout.ExpandWidth(false)))
        {
            _xmlDocument.Save(Application.dataPath + "/Resources/" + _path + ".xml");
            _message = string.Format("File {0} saved!", _path);
            AssetDatabase.Refresh();
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
        if (node.Name.Equals("GridSize")) return text;
        if (node.Name.Equals("CellSize")) return text;
        if (node.Name.Equals("StartingCell")) return text;
        if (node.Name.Equals("EndingCell")) return text;

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

        _tilesPerLevel[level] = new List<XmlNode>();
        _popupIndexesPerNeighbour[level] = new Dictionary<XmlNode, int>();
        _popupIndexesPerConstraint[level] = new Dictionary<XmlNode, int>();

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

        _popupIndexesPerConstraint[_currentLevel][constraint] = -1;

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

    #region ValidateXml

    private void ValidateXmlDocument()
    {
        bool errorsFound = false;
        foreach (XmlNode level in _xmlDocument.SelectNodes("/Levels/Level"))
        {
            if (!ValidateLevel(level))
            {
                errorsFound = true;
                break;
            }

            XmlNode settings = level["Settings"];

            if (!ValidateSettings(settings))
            {
                errorsFound = true;
                break;
            }

            foreach (XmlNode tile in level["WFC"].SelectNodes("/Tiles/Tile"))
            {
                if (!ValidateTile(tile))
                {
                    errorsFound = true;
                    break;
                }
            }

            if (errorsFound)
            {
                break;
            }
        }

        if (!errorsFound) 
        {
            _message = "Validation successful";
        }
    }

    private bool ValidateLevel(XmlNode level) 
    {
        if (level.Attributes["ID"].Value.Equals(string.Empty))
        {
            _message = "All Levels must have a string ID";
            return false;
        }

        return true;
    }

    private bool ValidateSettings(XmlNode settings)
    {
        // GridSize
        if (!int.TryParse(settings["GridSize"].Attributes["x"].InnerText, out int xGridSize))
        {
            _message = "GridSize X must be an integer";
            return false;
        }
        if (!int.TryParse(settings["GridSize"].Attributes["y"].InnerText, out int yGridSize))
        {
            _message = "GridSize Y must be an integer";
            return false;
        }

        // CellSize
        if (!float.TryParse(settings["CellSize"].Attributes["x"].InnerText, out float xCellSize))
        {
            _message = "CellSize X must be a float";
            return false;
        }
        if (!float.TryParse(settings["CellSize"].Attributes["y"].InnerText, out float yCellSize))
        {
            _message = "CellSize Y must be a float";
            return false;
        }


        // StartingCell
        if (!int.TryParse(settings["StartingCell"].Attributes["x"].InnerText, out int xStartingCell))
        {
            _message = "StartingCell X must be an integer";
            return false;
        }
        if (!int.TryParse(settings["StartingCell"].Attributes["y"].InnerText, out int yStartingCell))
        {
            _message = "StartingCell Y must be an integer";
            return false;
        }

        // EndingCell
        if (!int.TryParse(settings["EndingCell"].Attributes["x"].InnerText, out int xEndingCell))
        {
            _message = "EndingCell X must be an integer";
            return false;
        }
        if (!int.TryParse(settings["EndingCell"].Attributes["y"].InnerText, out int yEndingCell))
        {
            _message = "EndingCell Y must be an integer";
            return false;
        }

        bool gridSizeOk = xGridSize > 0 && yGridSize > 0;
        bool cellSizeOk = xCellSize > 0 && yCellSize > 0;
        bool startingCellOk = xStartingCell >= 0 && xStartingCell < xGridSize && yStartingCell >= 0 && yStartingCell < yGridSize;
        bool endingCellOk = xEndingCell >= 0 && xEndingCell < xGridSize && yEndingCell >= 0 && yEndingCell < yGridSize;
        bool startingEndingCellOk = xStartingCell != xEndingCell || yStartingCell != yEndingCell;

        if (!gridSizeOk)
        {
            _message = "GridSize must be above 0!";
            return false;
        }

        if (!cellSizeOk)
        {
            _message = "CellSize must be above 0!";
            return false;
        }

        if (!startingCellOk) 
        {
            _message = "Starting cell must be in GridSize";
            return false;
        }

        if (!endingCellOk)
        {
            _message = "Ending cell must be in GridSize";
            return false;
        }

        if (!startingEndingCellOk)
        {
            _message = "Starting and ending cell cannot be on the same";
            return false;
        }

        return true;
    }

    private bool ValidateTile(XmlNode tile)
    {
        if (!int.TryParse(tile.Attributes["Frequency"].Value, out int frequency))
        {
            _message = "frequency must be an integer";
            return false;
        }

        if (frequency <= 0)
        {
            _message = "frequency must be above 0";
            return false;
        }

        return true;
    }

    #endregion
}