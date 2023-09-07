using GMDG.Basic2DPlatformer.PCG;
using GMDG.NoProduct.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace GMDG.Basic2DPlatformer.System
{
    public class LevelManager : MonoBehaviour
    {
        private int _currentLevel;
        private LevelGenerator _generator;

        // Xml document
        private XmlDocument _xmlDocument;
        private XmlNodeList _xmlLevels;

        #region UnityMessages
        private void Awake()
        {
            _currentLevel = 0;
            _generator = new LevelGenerator();

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
            PCGData data = LoadData();
            if (!ValidateData(data)) return;
            StartCoroutine(_generator.Generation?.Invoke(this, data));
        }

        #endregion


        private PCGData LoadData()
        {
            PCGData data = new PCGData();

            XmlNode level = _xmlLevels[_currentLevel];

            // Settings
            XmlNode settings = level["Settings"];

            // GridSize
            int xGridSize = int.Parse(settings["GridSize"]["x"].InnerText);
            int yGridSize = int.Parse(settings["GridSize"]["y"].InnerText);
            data.GridSize = new Vector2Int(xGridSize, yGridSize);

            // CellSize
            float xCellSize = float.Parse(settings["CellSize"]["x"].InnerText);
            float yCellSize = float.Parse(settings["CellSize"]["y"].InnerText);
            data.CellSize = new Vector2(xCellSize, yCellSize);

            // StartingCell
            int xStartingCell = int.Parse(settings["StartingCell"]["x"].InnerText);
            int yStartingCell = int.Parse(settings["StartingCell"]["y"].InnerText);
            data.StartingCell = new Vector2Int(xStartingCell, yStartingCell);

            // EndingCell
            int xEndingCell = int.Parse(settings["EndingCell"]["x"].InnerText);
            int yEndingCell = int.Parse(settings["EndingCell"]["y"].InnerText);
            data.EndingCell = new Vector2Int(xEndingCell, yEndingCell);

            return data;
        }

        private bool ValidateData(PCGData data)
        {
            return true;
        }
    }
}

