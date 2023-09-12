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

    // Texts
    private string _path = "XML/WorldGenerationData";
    private string _message = string.Empty;

    // DataStructure
    private XmlDocument _xmlDocument;
    private XmlNodeList _levelNodes;
    private List<object> _currentHierarchy;

    [MenuItem("Window/PCG/XMLData")]
    private static void Init()
    {
        PCGXMLEditor window = GetWindow<PCGXMLEditor>(true, "XML Data", true);
        window.minSize = new Vector2(280, 400);
    }

    private void OnEnable()
    {
        InitTextures();
        InitXml();
        InitDataStructures();
    }

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
        // Loading XmlDocument
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

    private void OnGUI()
    {
        DrawLayout();
        DrawContent();
    }

    private void DrawLayout()
    {
        _headerRect.x = 0;
        _headerRect.y = 0;
        _headerRect.width = Screen.width;
        _headerRect.height = 50;
        GUI.DrawTexture(_headerRect, _headerTexture);

        _bodyRect.x = 20;
        _bodyRect.y = 50;
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
        DrawHierarchy();

        GUILayout.EndArea();
    }

    private void DrawBody()
    {
        GUILayout.BeginArea(_bodyRect);
        GUILayout.Space(10);
        GUILayout.Label("Current children:");
        GUILayout.BeginVertical();
        DrawCurrentChildren();
        DrawPossibleActions();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawMessage()
    {
        GUILayout.BeginArea(_messageRect);
        GUILayout.Space(10);
        GUILayout.Label(_message);
        GUILayout.EndArea();
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

    private string TakeString(object element)
    {
        string text = string.Empty;

        XmlNode node = element as XmlNode;

        if (node != null) return node.Name;

        return text;
    }

    private void DrawCurrentChildren()
    {
        object lastElement = _currentHierarchy[_currentHierarchy.Count - 1];

        XmlNodeList currentNodeList = TakeChildren(lastElement);

        if (currentNodeList == null) return;

        foreach (XmlNode node in currentNodeList) 
        {
            string buttonText = TakeString(node);

            if (GUILayout.Button(buttonText, GUILayout.ExpandWidth(false)))
            {
                _currentHierarchy.Add(node);
            }
        }
    }

    private XmlNodeList TakeChildren(object element)
    {
        XmlNode node = element as XmlNode;

        if (node != null) return node.ChildNodes;

        return null;    
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
                    XmlElement newLevel = _xmlDocument.CreateElement("Level");
                    XmlElement newSetting = _xmlDocument.CreateElement("Settings");
                    XmlElement newWFC = _xmlDocument.CreateElement("WFC");
                    newLevel.AppendChild(newSetting);
                    newLevel.AppendChild(newWFC);
                    node.AppendChild(newLevel);
                    _xmlDocument.Save(Application.dataPath + "/Resources/" + _path + ".xml");
                }
                break;
        }
    }

}
