using System.Xml;
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

    private List<object> _currentHierarchy = new List<object>();

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

        _currentHierarchy.Add("Hello");
        _currentHierarchy.Add("World");
        _currentHierarchy.Add("!");
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

        XmlNodeList xmlLevels = _xmlDocument.DocumentElement.SelectNodes("/Levels/Level");
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

        _bodyRect.x = 0;
        _bodyRect.y = 50;
        _bodyRect.width = Screen.width;
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

        GUILayout.Label("Current Hierarchy: ");
        DrawHierarchy();

        GUILayout.EndArea();
    }

    private void DrawHierarchy()
    {
        GUILayout.BeginHorizontal();
        for (int i = 0; i < _currentHierarchy.Count; i++) 
        { 
            object element = _currentHierarchy[i];

            if (GUILayout.Button("Element", GUILayout.ExpandWidth(false)))
            {
                for (int j = i + 1; j <  _currentHierarchy.Count; j++)
                {
                    _currentHierarchy.RemoveAt(j);
                }
            }

            if (i == _currentHierarchy.Count - 1) break;
            GUILayout.Label("->", GUILayout.ExpandWidth(false));
        }

        GUILayout.EndHorizontal();
    }

    private void DrawBody()
    {
        GUILayout.BeginArea(_bodyRect);
        GUILayout.Space(10);
        GUILayout.Label("Body");

        GUILayout.EndArea();
    }
    private void DrawMessage()
    {
        GUILayout.BeginArea(_messageRect);
        GUILayout.Space(10);
        GUILayout.Label(_message);
        GUILayout.EndArea();
    }
}
