using GMDG.Basic2DPlatformer.PCG;
using GMDG.Basic2DPlatformer.Utility;
using GMDG.NoProduct.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static GMDG.NoProduct.Utility.Utility2D;

namespace GMDG.Basic2DPlatformer.PCG
{
    public class PCGDataLoader
    {
        public PCGData LoadData(int currentLevel)
        {
            PCGData data = new PCGData();


            // Loading XmlDocument
            XmlDocument xmlDocument = new XmlDocument();
            TextAsset textAsset = Resources.Load<TextAsset>("XML/WorldGenerationData");
            xmlDocument.LoadXml(textAsset.text);
            XmlNodeList xmlLevels = xmlDocument.DocumentElement.SelectNodes("/Levels/Level");

            XmlNode level = xmlLevels[currentLevel];

            // Settings
            XmlNode settings = level["Settings"];

            // GridSize
            int xGridSize = int.Parse(settings["GridSize"].Attributes["x"].Value);
            int yGridSize = int.Parse(settings["GridSize"].Attributes["y"].Value);
            data.GridSize = new Vector2Int(xGridSize, yGridSize);

            // CellSize
            float xCellSize = float.Parse(settings["CellSize"].Attributes["x"].Value);
            float yCellSize = float.Parse(settings["CellSize"].Attributes["y"].Value);
            data.CellSize = new Vector2(xCellSize, yCellSize);

            data.Grid = new Grid<HashSet<int>>(data.GridSize, data.CellSize, Vector3.zero);

            // StartingCell
            int xStartingCell = int.Parse(settings["StartingCell"].Attributes["x"].Value);
            int yStartingCell = int.Parse(settings["StartingCell"].Attributes["y"].Value);
            data.StartingCell = new Vector2Int(xStartingCell, yStartingCell);

            // EndingCell
            int xEndingCell = int.Parse(settings["EndingCell"].Attributes["x"].Value);
            int yEndingCell = int.Parse(settings["EndingCell"].Attributes["y"].Value);
            data.EndingCell = new Vector2Int(xEndingCell, yEndingCell);

            // Tiles
            ExtractWFCTiles(data, level);

            Debug.Log(data);
            
            if (!ValidateData(data))
            {
#if UNITY_EDITOR
                Debug.LogError(string.Format("Level ID:{0} data is not valid!", currentLevel));
#endif
            }

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

                string id = node.Attributes["ID"].Value;
                int frequency = int.Parse(node.Attributes["Frequency"].Value);
                string prefabPath = node["Prefab"].Attributes["Path"].Value;

                visitedWFCTiles[id] = tile;
                totalFrequency += frequency;

                tile.Name = id;
                tile.RelativeFrequency = frequency;
                Debug.Log("Loading " + prefabPath);
                tile.Prefab = (GameObject)Resources.Load(string.Format("{0}", prefabPath));
            }

            // Update Relative Frequency
            foreach (WFCTile tile in visitedWFCTiles.Values)
            {
                tile.RelativeFrequency /= totalFrequency;
            }

            // Special Tiles
            foreach (string name in visitedWFCTiles.Keys)
            {
                if (!name.Equals("START")) continue;

                data.WFCTiles.Add(visitedWFCTiles[name]);
                break;
            }

            foreach (string name in visitedWFCTiles.Keys)
            {
                if (!name.Equals("END")) continue;

                data.WFCTiles.Add(visitedWFCTiles[name]);
                break;
            }

            foreach (WFCTile tile in visitedWFCTiles.Values)
            {
                if (data.WFCTiles.Contains(tile)) continue;
                data.WFCTiles.Add(tile);
            }

            // Getting Constraints
            for (int i = 0; i < tilesList.Count; i++)
            {
                XmlNode constraintsNode = tilesList[i]["Constraints"];

                if (constraintsNode == null) continue;

                XmlNodeList constraintsList = constraintsNode.SelectNodes("Constraint");

                for (int j = 0; j < constraintsList.Count; j++)
                {
                    XmlNode node = constraintsList[j];

                    string id = tilesList[i].Attributes["ID"].Value;
                    XmlNodeList neighbours = node["Neighbours"].ChildNodes;
                    string direction = node.Attributes["Direction"].Value;

                    List<Direction2D> directions = new List<Direction2D>();

                    AddPossibleDirection(direction, directions);

                    for (int k = 0; k < directions.Count; k++)
                    {
                        foreach (XmlNode neighbour in neighbours)
                        {
                            string neighbourId = neighbour.Attributes["ID"].Value;
                            visitedWFCTiles[id].PossibleNeighbours[directions[k]].Add(data.WFCTiles.IndexOf(visitedWFCTiles[neighbourId]));
                            visitedWFCTiles[neighbourId].PossibleNeighbours[OppositeDirections[directions[k]]].Add(data.WFCTiles.IndexOf(visitedWFCTiles[id]));
                        }
                    }
                }
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

