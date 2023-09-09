using GMDG.NoProduct.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using static GMDG.NoProduct.Utility.Utility2D;

namespace GMDG.Basic2DPlatformer.PCG.WFC
{
    public class WFCData
    {
        public Grid2D<int> Grid;
        public Dictionary<string, WFCTile> Tiles;

        public WFCData()
        {
            Tiles = new Dictionary<string, WFCTile>();
        }

        public void ReadData()
        {
            XmlDocument xmlDocument = new XmlDocument();
            TextAsset textAsset = Resources.Load<TextAsset>("XML/WorldGenerationData");
            xmlDocument.LoadXml(textAsset.text);

            XmlNodeList levels = xmlDocument.DocumentElement.SelectNodes("/Levels/Level");

            XmlNode level = levels[1];

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

            Grid = new Grid2D<int>(gridSize, cellSize, Vector2.zero);

            // Tiles
            XmlNodeList tilesList = level["Tiles"].SelectNodes("Tile");
            int totalFrequency = 0;
            foreach (XmlNode xmlTile in tilesList)
            {
                CreateTile(xmlTile, ref totalFrequency);
            }
            foreach (WFCTile tile in Tiles.Values)
            {
                tile.RelativeFrequency /= totalFrequency;
            }

            // Constraints
            XmlNodeList constraintsList = level["Constraints"].SelectNodes("Constraint");

            foreach (XmlNode xmlTile in tilesList)
            {
                foreach (XmlNode xmlConstraint in constraintsList)
                {
                    CreateConstraint(xmlConstraint);
                }
            }

        }

        private void CreateTile(XmlNode xmlTile, ref int totalFrequency)
        {
            string id = xmlTile["ID"].InnerText;
            string frequency = xmlTile["Frequency"].InnerText;
            string prefab = xmlTile["Prefab"].InnerText;

            WFCTile tile = new WFCTile();
            tile.Prefab = (GameObject)Resources.Load(string.Format("Prefabs/{0}", prefab));
            tile.RelativeFrequency = int.Parse(frequency);
            totalFrequency += int.Parse(frequency);
            Tiles.Add(id, tile);
        }

        private void CreateConstraint(XmlNode xmlConstraint)
        {
            string tile = xmlConstraint["Tile"].InnerText;
            XmlNodeList neighbours = xmlConstraint["Neighbours"].ChildNodes;
            string direction = xmlConstraint["Direction"].InnerText;

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

            for (int i = 0; i < directions.Count; i++)
            {
                foreach (XmlNode neighbour in neighbours)
                {
                    string id = neighbour.InnerText;
                    //Tiles[tile].PossibleNeighbours[directions[i]].Add(Tiles[id]);
                    //Tiles[id].PossibleNeighbours[OppositeDirections[directions[i]]].Add(Tiles[tile]);
                }
            }
        }

        public override string ToString()
        {
            string text = string.Empty;

            text = string.Concat(text, "Settings\n");
            text = string.Concat(text, string.Format("\tGridSize: {0}\n", Grid.GridSize));
            text = string.Concat(text, string.Format("\tCellSize: {0}\n", Grid.CellSize));
            text = string.Concat(text, string.Format("\tGridPosition: {0}\n", Grid.GridPosition));
            text = string.Concat(text, "Tiles\n");
            foreach (string id in Tiles.Keys)
            {
                text = string.Concat(text, string.Format("\tID: {0}\n", id));
                //text = string.Concat(text, string.Format("\tPrefabName: {0}\n", Tiles[id].Prefab.name));
                text = string.Concat(text, string.Format("\tRelative Frequency: {0}\n", Tiles[id].RelativeFrequency));
                text = string.Concat(text, "\tConstraints\n");
                foreach (Direction2D direction in Tiles[id].PossibleNeighbours.Keys)
                {
                    text = string.Concat(text, string.Format("\t\tDirection: {0}\n", direction));
                    text = string.Concat(text, "\t\tNeighbours:\n");
                    foreach (int neighbour in Tiles[id].PossibleNeighbours[direction])
                    {
                        //string tileId = Tiles.FirstOrDefault(x => x.Value == tile).Key;
                        //text = string.Concat(text, string.Format("\t\t\tID: {0}\n", tileId));
                    }
                    text = string.Concat(text, "\n");
                }
            }

            return text;
        }
    }

    public class WFCTile
    {
        public string Name;
        public GameObject Prefab;
        public float RelativeFrequency;
        public Dictionary<Direction2D, HashSet<int>> PossibleNeighbours = new Dictionary<Direction2D, HashSet<int>>()
        {
            { Direction2D.NORTH,  new HashSet<int>()},
            { Direction2D.EAST,  new HashSet<int>()},
            { Direction2D.SOUTH,  new HashSet<int>()},
            { Direction2D.WEST,  new HashSet<int>()},
        };
    }
}
