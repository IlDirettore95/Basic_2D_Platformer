using System.Xml;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using static GMDG.NoProduct.Utility.Utility2D;

public class PCGXMLEditorOld : EditorWindow
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

    private const int BUTTON_SIMPLE_WIDTH = 100;
    private const int BUTTON_ACTIONS_WIDTH = 140;
    private const int BUTTON_HIERARCHY_WIDTH = 150;

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
    private string[] _possibleConstraints = { "ALL", "NORTH", "EAST", "SOUTH", "WEST", "HORIZONTAL", "VERTICAL", "N_NORTH", "N_EAST", "N_SOUTH", "N_WEST"};

    //[MenuItem("Window/PCG/XMLData")]
    private static void Init()
    {
        PCGXMLEditorOld window = GetWindow<PCGXMLEditorOld>(true, "XML Data", true);
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
                    string direction = constraint.Attributes["Type"].Value;

                    for (int i = 0; i < _possibleConstraints.Length; i++)
                    {
                        if (!_possibleConstraints[i].Equals(direction)) continue;
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
        DrawAdding();
        GUILayout.EndScrollView();
    }
    
    private void DrawHierarchy()
    {
        GUILayout.BeginHorizontal();

        for (int i = 0; i < _currentHierarchy.Count; i++) 
        { 
            object element = _currentHierarchy[i];
            UpdateData(element);
            string buttonText = TakeCurrentHierarchyString(element);

            if (GUILayout.Button(buttonText, GUILayout.Width(BUTTON_HIERARCHY_WIDTH), GUILayout.ExpandWidth(false)))
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

        List<XmlNode> currentNodeList = TakeChildren(lastElement);

        if (currentNodeList == null) return;

        foreach (XmlNode node in currentNodeList) 
        {

            GUILayout.BeginHorizontal();

            DrawChildName(node);

            if (node.Attributes.Count > 0) 
            {
                DrawChildAttributes(node);
                DrawDeleting(node);
            }

            GUILayout.EndHorizontal();
        }
    }

    private void DrawChildName(XmlNode node)
    {
        string buttonText = TakeCurrentChildString(node);
        if (node.Attributes["List"] != null)
        {
            GUILayout.Label("-" + buttonText + "-", GUILayout.ExpandWidth(false));
        }
        else if (node.Attributes["Element"] != null)
        {
            GUILayout.Label(buttonText, GUILayout.Width(BUTTON_SIMPLE_WIDTH), GUILayout.ExpandWidth(false));
        }
        else if (GUILayout.Button(buttonText, GUILayout.Width(BUTTON_SIMPLE_WIDTH), GUILayout.ExpandWidth(false)))
        {
            _currentHierarchy.Add(node);
        }
    }
    

    private void DrawChildAttributes(XmlNode node)
    {
        foreach (XmlAttribute attribute in node.Attributes)
        {
            if (attribute.Name.Equals("List"))
            {
                continue;
            }
            if (attribute.Name.Equals("Element"))
            {
                continue;
            }
            if (node.Name.Equals("Neighbour") && attribute.Name.Equals("ID"))
            {
                int oldIndex = _popupIndexesPerNeighbour[_currentLevel][node];
                int newIndex = EditorGUILayout.Popup(oldIndex, GetTilesIDs(_tilesPerLevel[_currentLevel]), GUILayout.ExpandWidth(false));
                _popupIndexesPerNeighbour[_currentLevel][node] = newIndex;

                if (newIndex < 0) continue;

                node.Attributes["ID"].Value = _tilesPerLevel[_currentLevel][newIndex].Attributes["ID"].Value;

                if (newIndex != oldIndex) UpdateMutualTilesConstraint(oldIndex, newIndex, node);

                continue;
            }

            if (attribute.Name.Equals("Type"))
            {
                int oldIndex = _popupIndexesPerConstraint[_currentLevel][node];
                int newIndex = EditorGUILayout.Popup(oldIndex, _possibleConstraints, GUILayout.ExpandWidth(false));
                _popupIndexesPerConstraint[_currentLevel][node] = newIndex;

                if (newIndex < 0) continue;

                if (newIndex != oldIndex) UpdateMutualTilesConstraint(oldIndex, newIndex, node);

                node.Attributes["Type"].Value = _possibleConstraints[newIndex];
                continue;
            }

            if (attribute.Name.Equals("Path"))
            {
                string title = attribute.Value == "" ? "DragAndDrop" : attribute.Value;
                object[] objects = DropZone(title, title.Length * 8, 22);

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

    private void DrawAdding()
    {
        XmlNode node = _currentHierarchy[_currentHierarchy.Count - 1] as XmlNode;

        if (node == null) return;

        if (node.Name.Equals("Levels"))
        {
            if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
            {
                node.AppendChild(CreateLevelNode());
            }
        }
        else
        {
            foreach(XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Attributes["List"] == null) continue;
                switch (childNode.Name)
                {
                    case "Tiles":
                        if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                        {
                            childNode.AppendChild(CreateTileNode());
                        }
                        break;
                    case "Constraints":
                        if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                        {
                            childNode.AppendChild(CreateConstraintNode());
                        }
                        break;
                    case "Neighbours":
                        if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                        {
                            childNode.AppendChild(CreateNeighbourNode());
                        }
                        break;
                }
            }
        }
    }

    private void DrawDeleting(XmlNode node)
    {
        switch (node.Name)
        {
            case "Level":
                if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                {
                    node.ParentNode.RemoveChild(node);
                }
                break;
            case "Tile":
                if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                {
                    node.ParentNode.RemoveChild(node);
                }
                break;
            case "Constraint":
                if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                {
                    node.ParentNode.RemoveChild(node);
                }
                break;
            case "Neighbour":
                if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                {
                    node.ParentNode.RemoveChild(node);
                }
                break;
        }
    }

    private void DrawOtherActions()
    {
        if (GUILayout.Button("Reload", GUILayout.Width(BUTTON_ACTIONS_WIDTH), GUILayout.ExpandWidth(false)))
        {
            AssetDatabase.Refresh();
            InitXml();
            InitDataStructures();
        }

        if (GUILayout.Button("Validate", GUILayout.Width(BUTTON_ACTIONS_WIDTH), GUILayout.ExpandWidth(false)))
        {
            ValidateXmlDocument();
        }

        if (GUILayout.Button("Save", GUILayout.Width(BUTTON_ACTIONS_WIDTH), GUILayout.ExpandWidth(false)))
        {
            _xmlDocument.Save(Application.dataPath + "/Resources/" + _path + ".xml");
            _message = string.Format("File {0} saved!", _path);
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Validate and save", GUILayout.Width(BUTTON_ACTIONS_WIDTH), GUILayout.ExpandWidth(false)))
        {
            if (ValidateXmlDocument())
            {
                _xmlDocument.Save(Application.dataPath + "/Resources/" + _path + ".xml");
                _message = string.Format("File {0} saved!", _path);
                AssetDatabase.Refresh();
            }
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

    private string TakeCurrentChildString(object element)
    {
        string text = string.Empty;

        XmlNode node = element as XmlNode;

        if (node == null) return text;
            
        text += node.Name;

        if (node.Attributes.Count <= 0) return text;
        if (node.Attributes["ID"] != null) return node.Attributes["ID"].Value.Equals("") ? "-" : node.Attributes["ID"].Value;
        if (node.Attributes["Path"] != null) return text;
        if (node.Name.Equals("GridSize")) return text;
        if (node.Name.Equals("CellSize")) return text;
        if (node.Name.Equals("StartingCell")) return text;
        if (node.Name.Equals("EndingCell")) return text;

        return text;
    }

    private string TakeCurrentHierarchyString(object element)
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

        for (int i = 0; i < ids.Length; i++)
        {
            ids[i] = list[i].Attributes["ID"].Value;
        }

        return ids;
    }

    private List<XmlNode> TakeChildren(object element)
    {
        XmlNode node = element as XmlNode;

        if (node == null) return null;

        List<XmlNode> children = new List<XmlNode>();

        foreach (XmlNode childNode in node.ChildNodes) 
        {
            children.Add(childNode);
            if (childNode.Attributes["List"] != null) children.AddRange(childNode.ChildNodes);
        }

        return children;
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

        XmlAttribute list = _xmlDocument.CreateAttribute("List");
        constraints.Attributes.Append(list);

        XmlAttribute element = _xmlDocument.CreateAttribute("Element");
        XmlAttribute path = _xmlDocument.CreateAttribute("Path");
        prefab.Attributes.Append(element);
        prefab.Attributes.Append(path);


        tile.AppendChild(prefab);
        tile.AppendChild(constraints);

        _tilesPerLevel[_currentLevel].Add(tile);

        return tile;
    }

    private XmlElement CreateConstraintNode()
    {
        XmlElement constraint = _xmlDocument.CreateElement("Constraint");

        XmlAttribute direction = _xmlDocument.CreateAttribute("Type");
        XmlElement neighbours = _xmlDocument.CreateElement("Neighbours");

        XmlAttribute list = _xmlDocument.CreateAttribute("List");
        neighbours.Attributes.Append(list);

        constraint.Attributes.Append(direction);
        constraint.AppendChild(neighbours);

        _popupIndexesPerConstraint[_currentLevel][constraint] = -1;

        return constraint;
    }

    private XmlElement CreateNeighbourNode()
    {
        XmlElement neighbour = _xmlDocument.CreateElement("Neighbour");

        XmlAttribute element = _xmlDocument.CreateAttribute("Element");
        XmlAttribute id = _xmlDocument.CreateAttribute("ID");
        neighbour.Attributes.Append(element);
        neighbour.Attributes.Append(id);

        _popupIndexesPerNeighbour[_currentLevel][neighbour] = -1;

        return neighbour;
    }

    private XmlNode CreateSettingsNode()
    {
        XmlElement settings = _xmlDocument.CreateElement("Settings");

        XmlAttribute list = _xmlDocument.CreateAttribute("List");
        settings.Attributes.Append(list);

        settings.AppendChild(CreateVector2Element("GridSize"));
        settings.AppendChild(CreateVector2Element("CellSize"));
        settings.AppendChild(CreateVector2Element("StartingCell"));
        settings.AppendChild(CreateVector2Element("EndingCell"));

        return settings;
    }

    private XmlNode CreateWFCNode()
    {
        XmlElement wfc = _xmlDocument.CreateElement("WFC");

        XmlElement tiles = _xmlDocument.CreateElement("Tiles");

        XmlAttribute list = _xmlDocument.CreateAttribute("List");
        tiles.Attributes.Append(list);

        wfc.AppendChild(tiles);

        return wfc;
    }

    private XmlElement CreateVector2Element(string name)
    {
        XmlElement vector2 = _xmlDocument.CreateElement(name);

        XmlAttribute element = _xmlDocument.CreateAttribute("Element");
        XmlAttribute x = _xmlDocument.CreateAttribute("x");
        XmlAttribute y = _xmlDocument.CreateAttribute("y");

        x.Value = "0";
        y.Value = "0";

        vector2.Attributes.Append(element);
        vector2.Attributes.Append(x);
        vector2.Attributes.Append(y);

        return vector2;
    }

    private void UpdateMutualTilesConstraint(int oldIndex, int newIndex, XmlNode node)
    {
        if (node.Name.Equals("Constraint"))
        {
            string oldConstraintType = oldIndex == -1 ? "" : _possibleConstraints[oldIndex];
            string newConstraintType = _possibleConstraints[newIndex];
        }
        else if (node.Name.Equals("Neighbour"))
        {
            XmlNode oldTile = oldIndex == -1 ? null : _tilesPerLevel[_currentLevel][oldIndex];
            XmlNode newTile = _tilesPerLevel[_currentLevel][newIndex];

            string constraintType = node.ParentNode.ParentNode.Attributes["Type"].Value;
            string oppositeConstraintType = TakeOppositeConstraintType(constraintType);

            // Adding New
            XmlNode newNeighbour = CreateNeighbourNode();
            newNeighbour.Attributes["ID"].Value = _currentTile.Attributes["ID"].Value;

            XmlNode constraint = null;
            XmlNodeList constraints = newTile["Constraints"].SelectNodes("Constraint");
            foreach (XmlNode c in constraints)
            {
                if (!c.Attributes["Type"].Value.Equals(oppositeConstraintType)) continue;
                constraint = c;
                break;
            }
            if (constraint == null) constraint = CreateConstraintNode();

            constraint.Attributes["Type"].Value = oppositeConstraintType;
            constraint["Neighbours"].AppendChild(newNeighbour);
            newTile["Constraints"].AppendChild(constraint);

            _popupIndexesPerNeighbour[_currentLevel][newNeighbour] = _tilesPerLevel[_currentLevel].IndexOf(_currentTile);
            _popupIndexesPerConstraint[_currentLevel][constraint] = GetConstraintTypeIndex(oppositeConstraintType);

            // Deleting Old
            if (oldTile == null) return;
        }
    }

    #endregion

    #region ValidateXml

    private bool ValidateXmlDocument()
    {
        bool errorsFound = false;
        foreach (XmlNode level in _xmlDocument["Levels"].SelectNodes("Level"))
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

            foreach (XmlNode tile in level["WFC"]["Tiles"].SelectNodes("Tile"))
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

        if (errorsFound) _message += " (Validation Unsuccessful)";

        return !errorsFound;
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
        Debug.Log(tile["Prefab"].Attributes["Path"].Value);
        if (tile["Prefab"].Attributes["Path"].Value.Equals(string.Empty))
        {
            _message = "prefab asset is missing";
            return false;
        }

        return true;
    }

    #endregion

    #region Utilis

    private string TakeOppositeConstraintType(string contraintType)
    {
        string oppositeConstraintType = string.Empty;

        if (contraintType.Equals("ALL"))
        {
            oppositeConstraintType = "ALL";
        }
        else if (contraintType.Equals("NORTH"))
        {
            oppositeConstraintType = "SOUTH";
        }
        else if (contraintType.Equals("SOUTH"))
        {
            oppositeConstraintType = "NORTH";
        }
        else if (contraintType.Equals("EAST"))
        {
            oppositeConstraintType = "WEST";
        }
        else if (contraintType.Equals("WEST"))
        {
            oppositeConstraintType = "EAST";
        }
        else if (contraintType.Equals("VERTICAL"))
        {
            oppositeConstraintType = "VERTICAL";
        }
        else if (contraintType.Equals("HORIZONTAL"))
        {
            oppositeConstraintType = "HORIZONTAL";
        }
        else if (contraintType.Equals("N_NORTH"))
        {
            oppositeConstraintType = "N_SOUTH";
        }
        else if (contraintType.Equals("N_SOUTH"))
        {
            oppositeConstraintType = "N_NORTH";
        }
        else if (contraintType.Equals("N_EAST"))
        {
            oppositeConstraintType = "N_WEST";
        }
        else if (contraintType.Equals("N_WEST"))
        {
            oppositeConstraintType = "N_EAST";
        }

        return oppositeConstraintType;
    }

    private int GetConstraintTypeIndex(string contraintType)
    {
        int index = -1;

        for (int i = 0; i < _possibleConstraints.Length; i++)
        {
            if (_possibleConstraints[i].Equals(contraintType)) return i;
        }

        return index;
    }

    #endregion
}