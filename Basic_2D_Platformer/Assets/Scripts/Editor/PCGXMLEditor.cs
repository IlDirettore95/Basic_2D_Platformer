using System.Xml;
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

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
        _currentHierarchy = new List<object>
        {
            _xmlDocument.SelectNodes("//Levels")[0]
        };
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

    #endregion

    #region ReadXml

    private string TakeString(object element)
    {
        string text = string.Empty;

        XmlNode node = element as XmlNode;

        if (node == null) return text;
            
        text += node.Name;

        if (node.Attributes.Count <= 0) return text;

        text += " " + node.Attributes[0].Value;

        return text;
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
        XmlAttribute prefab = _xmlDocument.CreateAttribute("Prefab");
        XmlAttribute frequency = _xmlDocument.CreateAttribute("Frequency");

        tile.Attributes.Append(id);
        tile.Attributes.Append(prefab);
        tile.Attributes.Append(frequency);

        XmlElement constraints = _xmlDocument.CreateElement("Constraints");
        tile.AppendChild(constraints);

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