using GMDG.NoProduct.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PCG.WFC
{
    public class WFCData
    {
        public Utility2D.Grid2D Grid;
        public Dictionary<string, Tile> Tiles;
        public int TotalWeight;

        public WFCData()
        {
            Tiles = new Dictionary<string, Tile>();
        }

        public void ReadData()
        {
            XmlDocument xmlDocument = new XmlDocument();
            TextAsset textAsset = Resources.Load<TextAsset>("XML/WorldGenerationData");
            xmlDocument.LoadXml(textAsset.text);

            XmlNodeList levels = xmlDocument.DocumentElement.SelectNodes("/Levels/Level");

            XmlNode level = levels[0];

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

            Grid = new Utility2D.Grid2D(gridSize, cellSize, Vector2.zero);

            // Tiles
            XmlNodeList tilesList = xmlDocument.DocumentElement.SelectNodes("/Levels/Level/Tiles/Tile");
            int totalFrequency = 0;
            foreach (XmlNode xmlTile in tilesList)
            {
                CreateTile(xmlTile, ref totalFrequency);
            }
            foreach (Tile tile in Tiles.Values)
            {
                tile.RelativeFrequency /= totalFrequency;
            }

            // Constraints
            XmlNodeList constraintsList = xmlDocument.DocumentElement.SelectNodes("/Levels/Level/Constraints/Constraint");

            foreach (XmlNode xmlConstraint in constraintsList)
            {
                CreateConstraint(xmlConstraint);
            }
        }

        private void CreateTile(XmlNode xmlTile, ref int totalFrequency)
        {
            string id = xmlTile["ID"].InnerText;
            string frequency = xmlTile["Frequency"].InnerText;
            string prefab = xmlTile["Prefab"].InnerText;

            Tile tile = new Tile();
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

            if (direction.Equals("ALL"))
            {
                foreach (Utility2D.Direction2D direction2D in Utility2D.Directions2D)
                {
                    foreach (XmlNode neighbour in neighbours)
                    {
                        string id = neighbour.InnerText;
                        Tiles[tile].PossibleNeighbours[direction2D].Add(Tiles[id]);
                        Tiles[id].PossibleNeighbours[Utility2D.OppositeDirections[direction2D]].Add(Tiles[tile]);
                    }
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
                text = string.Concat(text, string.Format("\tPrefabName: {0}\n", Tiles[id].Prefab.name));
                text = string.Concat(text, string.Format("\tRelative Frequency: {0}\n", Tiles[id].RelativeFrequency));
                text = string.Concat(text, "\tConstraints\n");
                foreach(Utility2D.Direction2D direction in Tiles[id].PossibleNeighbours.Keys)
                {
                    text = string.Concat(text, string.Format("\t\tDirection: {0}\n", direction));
                    text = string.Concat(text, "\t\tNeighbours:\n");
                    foreach (Tile tile in Tiles[id].PossibleNeighbours[direction])
                    {
                        string tileId = Tiles.FirstOrDefault(x => x.Value == tile).Key;
                        text = string.Concat(text, string.Format("\t\t\tID: {0}\n", tileId));
                    }
                    text = string.Concat(text, "\n");
                }
            }

            return text;
        }
    }

    public class Tile
    {
        public GameObject Prefab;
        public float RelativeFrequency;
        public Dictionary<Utility2D.Direction2D, HashSet<Tile>> PossibleNeighbours = new Dictionary<Utility2D.Direction2D, HashSet<Tile>>()
        {
            { Utility2D.Direction2D.NORTH,  new HashSet<Tile>()},
            { Utility2D.Direction2D.EAST,  new HashSet<Tile>()},
            { Utility2D.Direction2D.SOUTH,  new HashSet<Tile>()},
            { Utility2D.Direction2D.WEST,  new HashSet<Tile>()},
        };
    }
}
