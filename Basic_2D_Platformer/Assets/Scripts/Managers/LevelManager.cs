using GMDG.NoProduct.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace GMDG.Basic2DPlatformer.System
{
    public class LevelManager : MonoBehaviour
    {
        private int _currentLevel;
        private Utility2D.Grid2D<int> _currentGrid;

        // Xml document
        private XmlDocument _xmlDocument;
        private XmlNodeList _xmlLevels;

        #region UnityMessages
        private void Awake()
        {
            _currentLevel = 0;

            // Loading XmlDocument
            _xmlDocument = new XmlDocument();
            TextAsset textAsset = Resources.Load<TextAsset>("XML/WorldGenerationData");
            _xmlDocument.LoadXml(textAsset.text);
            _xmlLevels = _xmlDocument.DocumentElement.SelectNodes("/Levels/Level");

            enabled = false;

            EventManager.Instance.Subscribe(Event.OnSystemsLoaded, Activate);
        }

        private void OnEnable()
        {
            EventManager.Instance.Subscribe(Event.OnGameplay, GenerateLevel);
        }

        private void OnDisable()
        {
            EventManager.Instance.Unsubscribe(Event.OnGameplay, GenerateLevel);
        }

        private void OnDestroy()
        {
            EventManager.Instance.Unsubscribe(Event.OnSystemsLoaded, Activate);
        }

        #endregion

        #region Listeners

        private void Activate(object[] args)
        {
            enabled = true;
        }
        private void GenerateLevel(object[] args)
        {
            LoadLevelData();
        }

        #endregion


        private void LoadLevelData()
        {
            XmlNode level = _xmlLevels[_currentLevel];

            // Settings
            XmlNode settings = level["Settings"];

            // GridSize
            int xGridSize = int.Parse(settings["GridSize"]["x"].InnerText);
            int yGridSize = int.Parse(settings["GridSize"]["y"].InnerText);
            Vector2Int gridSize = new Vector2Int(xGridSize, yGridSize);

            // CellSize
            float xCellSize = float.Parse(settings["CellSize"]["x"].InnerText);
            float yCellSize = float.Parse(settings["CellSize"]["y"].InnerText);
            Vector2 cellSize = new Vector2(xCellSize, yCellSize);

            _currentGrid = new Utility2D.Grid2D<int>(gridSize, cellSize, Vector3.zero);

            EventManager.Instance.Publish(Event.OnGridUpdated, _currentGrid);
        }
    }
}

