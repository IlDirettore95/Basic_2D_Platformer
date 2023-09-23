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
        public int GetNumberOfLevels()
        {
            // Loading XmlDocument
            XmlDocument xmlDocument = new XmlDocument();
            TextAsset textAsset = Resources.Load<TextAsset>("XML/WorldGenerationData");
            xmlDocument.LoadXml(textAsset.text);
            XmlNodeList xmlLevels = xmlDocument.SelectNodes("Root/Levels/Level");

            return xmlLevels.Count;
        }

        public PCGData LoadData(int currentLevel, MonoBehaviour caller)
        {
            PCGData data = new PCGData();

            // Loading XmlDocument
            XmlDocument xmlDocument = new XmlDocument();
            TextAsset textAsset = Resources.Load<TextAsset>("XML/WorldGenerationData");
            xmlDocument.LoadXml(textAsset.text);
            XmlNodeList xmlLevels = xmlDocument.SelectNodes("Root/Levels/Level");

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

            // PassageCell
            int xPassageCell = int.Parse(settings["PassageCell"].Attributes["x"].Value);
            int yPassageCell = int.Parse(settings["PassageCell"].Attributes["y"].Value);
            data.PassageCell = new Vector2Int(xPassageCell, yPassageCell);

            // Tiles
            ExtractWFCTiles(data, level, caller);
#if UNITY_EDITOR
            Debug.Log(data);
#endif

            if (!ValidateData(data))
            {
#if UNITY_EDITOR
                Debug.LogError(string.Format("Level ID:{0} data is not valid!", currentLevel));
#endif
            }

            return data;
        }

        private void ExtractWFCTiles(PCGData data, XmlNode level, MonoBehaviour caller)
        {
            XmlNodeList tilesList = level["WFC"]["Tiles"].SelectNodes("Tile");

            Dictionary<string, WFCTile> visitedWFCTiles = new Dictionary<string, WFCTile>();
            int totalFrequency = 0;

            // Getting Tiles
            for (int i = 0; i < tilesList.Count; i++)
            {
                XmlNode node = tilesList[i];

                string id = node.Attributes["ID"].Value;
                bool rh = bool.Parse(node.Attributes["RH"].Value);
                bool rv = bool.Parse(node.Attributes["RV"].Value);
                int frequency = int.Parse(node.Attributes["Frequency"].Value);
                string prefabPath = node["Prefab"].Attributes["Path"].Value;

                WFCTile tile = new WFCTile();
                tile.Name = id;
                tile.RelativeFrequency = frequency;
                tile.Log2RelativeFrequency = Mathf.Log(tile.RelativeFrequency, 2);
                tile.Prefab = (GameObject)Resources.Load(string.Format("{0}", prefabPath));

                visitedWFCTiles[tile.Name] = tile;
                totalFrequency += frequency;

                if (rh)
                {
                    GameObject rhGO = GameObject.Instantiate(tile.Prefab, caller.transform);
                    rhGO.name = tile.Name + "_RH";
                    for (int j = 0; j < rhGO.transform.childCount; j++)
                    {
                        Transform transformChild = rhGO.transform.GetChild(j);
                        Vector3 oldPosition = transformChild.position;
                        transformChild.position = new Vector3(-oldPosition.x, oldPosition.y, oldPosition.z);
                    }
                    rhGO.SetActive(false);

                    WFCTile tileRH = new WFCTile();
                    tileRH.Name = id + "_RH";
                    tileRH.RelativeFrequency = frequency;
                    tileRH.Log2RelativeFrequency = Mathf.Log(tile.RelativeFrequency, 2);
                    tileRH.Prefab = rhGO;

                    visitedWFCTiles[tileRH.Name] = tileRH;
                    totalFrequency += frequency;
                }

                if (rv)
                {
                    GameObject rvGO = GameObject.Instantiate(tile.Prefab, caller.transform);
                    rvGO.name = tile.Name + "_RV";
                    for (int j = 0; j < rvGO.transform.childCount; j++)
                    {
                        Transform transformChild = rvGO.transform.GetChild(j);
                        Vector3 oldPosition = transformChild.position;
                        transformChild.position = new Vector3(oldPosition.x, -oldPosition.y, oldPosition.z);
                    }
                    rvGO.SetActive(false);

                    WFCTile tileRV = new WFCTile();
                    tileRV.Name = id + "_RV";
                    tileRV.RelativeFrequency = frequency;
                    tileRV.Log2RelativeFrequency = Mathf.Log(tile.RelativeFrequency, 2);
                    tileRV.Prefab = (GameObject)Resources.Load(string.Format("{0}", prefabPath));

                    visitedWFCTiles[tileRV.Name] = tileRV;
                    totalFrequency += frequency;
                }
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

            foreach (string name in visitedWFCTiles.Keys)
            {
                if (!name.Equals("PASSAGE")) continue;

                data.WFCTiles.Add(visitedWFCTiles[name]);
                break;
            }

            //Other Tiles
            foreach (WFCTile tile in visitedWFCTiles.Values)
            {
                if (data.WFCTiles.Contains(tile)) continue;
                data.WFCTiles.Add(tile);
            }

            // Getting Constraints
            for (int i = 0; i < tilesList.Count; i++)
            {
                XmlNode tile = tilesList[i];
                string id = tile.Attributes["ID"].Value;
                bool rh = bool.Parse(tile.Attributes["RH"].Value);
                bool rv = bool.Parse(tile.Attributes["RV"].Value);

                string rhID = rh ? id + "_RH" : null;
                string rvID = rv ? id + "_RV" : null;

                XmlNode constraintsNode = tile["Constraints"];
                if (constraintsNode == null) continue;

                XmlNodeList constraintsList = constraintsNode.SelectNodes("Constraint");

                for (int j = 0; j < constraintsList.Count; j++)
                {
                    XmlNode constraint = constraintsList[j];
                    XmlNodeList neighbours = constraint["Neighbours"].ChildNodes;
                    string directionType = constraint.Attributes["Type"].Value;

                    List<Direction2D> directions = AddPossibleDirection(directionType);

                    for (int k = 0; k < directions.Count; k++)
                    {
                        Direction2D direction = directions[k];
                        foreach (XmlNode neighbour in neighbours)
                        {
                            string neighbourID = neighbour.Attributes["ID"].Value;
                            visitedWFCTiles[id].PossibleNeighbours[direction].Add(data.WFCTiles.IndexOf(visitedWFCTiles[neighbourID]));

                            if (rhID != null )
                            {
                                if (direction == Direction2D.EAST || direction == Direction2D.WEST)
                                {
                                    visitedWFCTiles[rhID].PossibleNeighbours[OppositeDirections[direction]].Add(data.WFCTiles.IndexOf(visitedWFCTiles[neighbourID]));
                                    visitedWFCTiles[neighbourID].PossibleNeighbours[direction].Add(data.WFCTiles.IndexOf(visitedWFCTiles[rhID]));
                                }
                                else
                                {
                                    visitedWFCTiles[rhID].PossibleNeighbours[direction].Add(data.WFCTiles.IndexOf(visitedWFCTiles[neighbourID]));
                                    visitedWFCTiles[neighbourID].PossibleNeighbours[OppositeDirections[direction]].Add(data.WFCTiles.IndexOf(visitedWFCTiles[rhID]));
                                }
                            }

                            if (rvID != null)
                            {
                                if (direction == Direction2D.NORTH || direction == Direction2D.SOUTH)
                                {
                                    visitedWFCTiles[rvID].PossibleNeighbours[OppositeDirections[direction]].Add(data.WFCTiles.IndexOf(visitedWFCTiles[neighbourID]));
                                    visitedWFCTiles[neighbourID].PossibleNeighbours[direction].Add(data.WFCTiles.IndexOf(visitedWFCTiles[rvID]));
                                }
                                else
                                {
                                    visitedWFCTiles[rvID].PossibleNeighbours[direction].Add(data.WFCTiles.IndexOf(visitedWFCTiles[neighbourID]));
                                    visitedWFCTiles[neighbourID].PossibleNeighbours[OppositeDirections[direction]].Add(data.WFCTiles.IndexOf(visitedWFCTiles[rvID]));
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<Direction2D> AddPossibleDirection(string direction)
        {
            List<Direction2D> directions = new List<Direction2D>();
            
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

            return directions;
        }

        private bool ValidateData(PCGData data)
        {
            return true;
        }
    }

}

