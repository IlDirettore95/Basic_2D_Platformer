using GMDG.Basic2DPlatformer.PCG;
using GMDG.Basic2DPlatformer.PCG.WFC;
using GMDG.Basic2DPlatformer.Utility;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static GMDG.NoProduct.Utility.Utility2D;

namespace GMDG.Basic2DPlatformer.System
{
    public class LevelManager : MonoBehaviour
    {
        private int _currentLevel;
        private LevelGenerator _generator;

        #region UnityMessages
        private void Awake()
        {
            _currentLevel = 0;
            _generator = new LevelGenerator();



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

#if UNITY_EDITOR
        public void GenerateLevel()
        {
            GenerateLevel(null);
        }
#endif

        private PCGData LoadData()
        {
            PCGData data = new PCGData();


            // Loading XmlDocument
            XmlDocument xmlDocument = new XmlDocument();
            TextAsset textAsset = Resources.Load<TextAsset>("XML/WorldGenerationData");
            xmlDocument.LoadXml(textAsset.text);
            XmlNodeList xmlLevels = xmlDocument.DocumentElement.SelectNodes("/Levels/Level");

            XmlNode level = xmlLevels[_currentLevel];

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

            data.Grid = new Grid<int>(data.GridSize, data.CellSize, Vector3.zero);

            // StartingCell
            int xStartingCell = int.Parse(settings["StartingCell"]["x"].InnerText);
            int yStartingCell = int.Parse(settings["StartingCell"]["y"].InnerText);
            data.StartingCell = new Vector2Int(xStartingCell, yStartingCell);

            // EndingCell
            int xEndingCell = int.Parse(settings["EndingCell"]["x"].InnerText);
            int yEndingCell = int.Parse(settings["EndingCell"]["y"].InnerText);
            data.EndingCell = new Vector2Int(xEndingCell, yEndingCell);

            // Tiles
            ExtractWFCTiles(data, level);

            Debug.Log(data);

            return data;
        }

        private void ExtractWFCTiles(PCGData data, XmlNode level)
        {
            XmlNodeList tilesList = level["WFC"]["Tiles"].SelectNodes("Tile");

            Dictionary<string, WFCTile> visitedWFCTiles = new Dictionary<string, WFCTile>();
            int totalFrequency = 0;

            // Getting Tiles
            for (int i = 0; i < tilesList.Count; i++)
            {
                XmlNode node = tilesList[i];

                WFCTile tile = new WFCTile();

                string id = node["ID"].InnerText;
                int frequency = int.Parse(node["Frequency"].InnerText);
                string prefab = node["Prefab"].InnerText;

                visitedWFCTiles[id] = tile;
                totalFrequency += frequency;

                tile.Name = id;
                tile.RelativeFrequency = frequency;
                tile.Prefab = (GameObject)Resources.Load(string.Format("Prefabs/{0}", prefab));
            }

            // Update Relative Frequency
            foreach (WFCTile tile in visitedWFCTiles.Values)
            {
                tile.RelativeFrequency /= totalFrequency;
            }

            // Getting Constraints
            for (int i = 0; i < tilesList.Count; i++)
            {
                XmlNodeList constraintsList = tilesList[i]["Constraints"].SelectNodes("Constraint");

                for (int j = 0; j < constraintsList.Count; j++) 
                {
                    XmlNode node = constraintsList[j];

                    string id = tilesList[i]["ID"].InnerText;
                    XmlNodeList neighbours = node["Neighbours"].ChildNodes;
                    string direction = node["Direction"].InnerText;

                    List<Direction2D> directions = new List<Direction2D>();

                    AddPossibleDirection(direction, directions);

                    for (int k = 0; k < directions.Count; k++)
                    {
                        foreach (XmlNode neighbour in neighbours)
                        {
                            string neighbourId = neighbour.InnerText;
                            visitedWFCTiles[id].PossibleNeighbours[directions[k]].Add(visitedWFCTiles[neighbourId]);
                            visitedWFCTiles[neighbourId].PossibleNeighbours[OppositeDirections[directions[k]]].Add(visitedWFCTiles[id]);
                        }
                    }
                }
            }

            foreach (WFCTile tile in visitedWFCTiles.Values)
            {
                data.WFCTiles.Add(tile);
            }
        }

        private void AddPossibleDirection(string direction, List<Direction2D> directions)
        {
            if (direction.Equals("ALL"))
            {
                directions.Add(Direction2D.NORTH);
                directions.Add(Direction2D.SOUTH);
                directions.Add(Direction2D.EAST);
                directions.Add(Direction2D.WEST);
            }
            else if (direction.Equals("NORTH"))
            {
                directions.Add(Direction2D.NORTH);
            }
            else if (direction.Equals("SOUTH"))
            {
                directions.Add(Direction2D.SOUTH);
            }
            else if (direction.Equals("EAST"))
            {
                directions.Add(Direction2D.EAST);
            }
            else if (direction.Equals("WEST"))
            {
                directions.Add(Direction2D.WEST);
            }
            else if (direction.Equals("VERTICAL"))
            {
                directions.Add(Direction2D.NORTH);
                directions.Add(Direction2D.SOUTH);
            }
            else if (direction.Equals("HORIZONTAL"))
            {
                directions.Add(Direction2D.EAST);
                directions.Add(Direction2D.WEST);
            }
            else if (direction.Equals("N_NORTH"))
            {
                directions.Add(Direction2D.SOUTH);
                directions.Add(Direction2D.EAST);
                directions.Add(Direction2D.WEST);
            }
            else if (direction.Equals("N_SOUTH"))
            {
                directions.Add(Direction2D.NORTH);
                directions.Add(Direction2D.EAST);
                directions.Add(Direction2D.WEST);
            }
            else if (direction.Equals("N_EAST"))
            {
                directions.Add(Direction2D.NORTH);
                directions.Add(Direction2D.SOUTH);
                directions.Add(Direction2D.WEST);
            }
            else if (direction.Equals("N_WEST"))
            {
                directions.Add(Direction2D.NORTH);
                directions.Add(Direction2D.SOUTH);
                directions.Add(Direction2D.EAST);
            }
        }

        private bool ValidateData(PCGData data)
        {
            return true;
        }
    }
}