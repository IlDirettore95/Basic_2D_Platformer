using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Unity.VisualScripting.YamlDotNet.RepresentationModel;

namespace GMDG.Basic2DPlatformer.Tools.XML
{
    public class Utils
    {
        #region Navigation

        public static XmlNodeList GetLevelNodes(XmlDocument document)
        {
            return document["Root"]["Levels"].SelectNodes("Level");
        }

        public static XmlNode GetSettingsNode(XmlNode level)
        {
            return level["Settings"];
        }

        public static XmlNodeList GetTileNodes(XmlNode level)
        {
            return level["WFC"]["Tiles"].SelectNodes("Tile");
        }
    
        public static XmlNodeList GetConstraintNodes(XmlNode tile) 
        {
            return tile["Constraints"].SelectNodes("Constraint");
        }

        public static XmlNodeList GetNeighbourNodes(XmlNode constraint)
        {
            return constraint["Neighbours"].SelectNodes("Neighbour");
        }
    
        public static XmlNode GetLevelsNodeList(XmlDocument document)
        {
            return document["Root"]["Levels"];
        }

        public static XmlNode GetTilesNodeList(XmlNode level)
        {
            return level["WFC"]["Tiles"];
        }

        public static XmlNode GetConstraintsNodeList(XmlNode tile)
        {
            return tile["Constraints"];
        }

        public static XmlNode GetNeighboursNodeList(XmlNode constraint)
        {
            return constraint["Neighbours"];
        }

        public static XmlNode GetLevelNodeFromTile(XmlNode tile)
        {
            return tile.ParentNode.ParentNode.ParentNode;
        }

        public static XmlNode GetTileNodeFromConstraint(XmlNode constraint)
        {
            return constraint.ParentNode.ParentNode;
        }

        public static XmlNode GetConstraintNodeFromNeighbour(XmlNode neighbour)
        {
            return neighbour.ParentNode.ParentNode;
        }

        public static XmlNode GetCorrespondingTile(List<XmlNode> tiles, XmlNode neighbour)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                XmlNode tile = tiles[i];
                if (!IsNodeAttributeEqual(tiles[i], "ID", GetNodeAttributeValue(neighbour, "ID"))) continue;
                return tiles[i];
            }

            return null;
        }

        public static List<XmlNode> GetCorrespondingNeighbours(XmlNode chosenTile, XmlNode level)
        {
            List<XmlNode> neighboursToReturn = new List<XmlNode>();
            foreach (XmlNode tile in GetTileNodes(level))
            {
                foreach (XmlNode constraint in GetConstraintNodes(tile))
                {
                    foreach (XmlNode neighbour in GetNeighbourNodes(constraint))
                    {
                        if (!IsNodeAttributeEqual(neighbour, "ID", GetNodeAttributeValue(chosenTile, "ID"))) continue;
                        neighboursToReturn.Add(neighbour);
                    }
                }
            }
            return neighboursToReturn;
        }

        public static string GetNodeAttributeValue(XmlNode node, string attribute)
        {
            return node.Attributes[attribute].Value;
        }
    
        public static bool IsNodeAttributeEqual(XmlNode node, string attribute, string value)
        { 
            return GetNodeAttributeValue(node, attribute).Equals(value);
        }       
        
        public static bool IsAttributeNameEqual(XmlAttribute attribute, string value)
        {
            return attribute.Name.Equals(value);
        }

        public static bool HasNodeElement(XmlNode node, string element) 
        { 
            return node[element] != null;
        }

        public static bool HasNodeAttribute(XmlNode node, string attribute) 
        { 
            return node.Attributes[attribute] != null;
        }

        public static bool HasNodeAttributes(XmlNode node)
        {
            return node.Attributes.Count > 0;
        }

        public static bool IsNodeNameEqual(XmlNode node, string value)
        {
            return node.Name.Equals(value);
        }

        public static bool HasNodeChildren(XmlNode node)
        {
            return node.ChildNodes.Count > 0;
        }

        public static bool HasNodeListAttributeEqual(XmlNodeList nodes, string attribute, string value, out XmlNode foundNode)
        {
            foundNode = null;
            foreach (XmlNode node in nodes) 
            {
                if (!IsNodeAttributeEqual(node, attribute, value)) continue;
                foundNode = node;
                break;
            }
            return foundNode != null;
        }

        #endregion

        #region Graphics

        public static string GetHierarchyButtonText(XmlNode element)
        {
            string text = string.Empty;

            text += element.Name;

            if (!HasNodeAttributes(element)) return text;
            if (HasNodeAttribute(element, "Path")) return text;
            if (IsNodeNameEqual(element, "GridSize")) return text;
            if (IsNodeNameEqual(element, "CellSize")) return text;
            if (IsNodeNameEqual(element, "StartingCell")) return text;
            if (IsNodeNameEqual(element, "EndingCell")) return text;

            text += " " + element.Attributes[0].Value;

            return text;
        }

        public static string GetCurrentChildString(XmlNode element)
        {
            string text = string.Empty;

            text += element.Name;

            if (!HasNodeAttributes(element)) return text;
            if (HasNodeAttribute(element, "ID")) return element.Attributes["ID"].Value.Equals("") ? "-" : element.Attributes["ID"].Value;
            if (HasNodeAttribute(element, "Path")) return text;
            if (IsNodeNameEqual(element, "GridSize")) return text;
            if (IsNodeNameEqual(element, "CellSize")) return text;
            if (IsNodeNameEqual(element, "StartingCell")) return text;
            if (IsNodeNameEqual(element, "EndingCell")) return text;

            return text;
        }

        public static List<XmlNode> GetCurrentChildren(XmlNode currentFather)
        {
            List<XmlNode> nodesToDraw = new List<XmlNode>();
            foreach (XmlNode childNode in currentFather.ChildNodes)
            {
                nodesToDraw.Add(childNode);
                if (!HasNodeAttribute(childNode, "List")) continue;

                foreach (XmlNode grandChild in childNode.ChildNodes)
                {
                    nodesToDraw.Add(grandChild);
                }
            }
            return nodesToDraw;
        }

        public static string GetResourcesPath(string assetdatabasePath)
        {
            assetdatabasePath = assetdatabasePath.Substring(assetdatabasePath.IndexOf("/") + 1);
            assetdatabasePath = assetdatabasePath.Substring(assetdatabasePath.IndexOf("/") + 1);
            assetdatabasePath = assetdatabasePath.Substring(0, assetdatabasePath.LastIndexOf("."));

            return assetdatabasePath;
        }

        #endregion

        #region WriteXML

        public static XmlElement CreateRootElement(XmlDocument xmlDocument)
        {
            XmlElement root = xmlDocument.CreateElement("Root");
            root.AppendChild(CreateLevelsElement(xmlDocument));

            return root;
        }

        private static XmlElement CreateLevelsElement(XmlDocument xmlDocument)
        {
            XmlElement levels = xmlDocument.CreateElement("Levels");
            XmlAttribute list = xmlDocument.CreateAttribute("List");
            levels.Attributes.Append(list);

            return levels;
        }

        public static XmlElement CreateLevelElement(XmlDocument xmlDocument)
        {
            XmlElement level = xmlDocument.CreateElement("Level");
            XmlAttribute id = xmlDocument.CreateAttribute("ID");
            level.Attributes.Append(id);
            level.AppendChild(CreateSettingsElement(xmlDocument));
            level.AppendChild(CreateWFCElement(xmlDocument));

            //_popupIndexesPerConstraint[level] = new Dictionary<XmlNode, int>();

            return level;
        }

        private static XmlElement CreateSettingsElement(XmlDocument xmlDocument)
        {
            XmlElement settings = xmlDocument.CreateElement("Settings");
            XmlAttribute list = xmlDocument.CreateAttribute("List");
            settings.Attributes.Append(list);
            settings.AppendChild(CreateVector2Element(xmlDocument, "GridSize"));
            settings.AppendChild(CreateVector2Element(xmlDocument, "CellSize"));
            settings.AppendChild(CreateVector2Element(xmlDocument, "StartingCell"));
            settings.AppendChild(CreateVector2Element(xmlDocument, "EndingCell"));
            settings.AppendChild(CreateVector2Element(xmlDocument, "PassageCell"));

            return settings;
        }

        private static XmlElement CreateWFCElement(XmlDocument xmlDocument)
        {
            XmlElement wfc = xmlDocument.CreateElement("WFC");
            wfc.AppendChild(CreateTilesElement(xmlDocument));

            return wfc;
        }

        private static XmlElement CreateTilesElement(XmlDocument xmlDocument)
        {
            XmlElement tiles = xmlDocument.CreateElement("Tiles");
            XmlAttribute list = xmlDocument.CreateAttribute("List");
            tiles.Attributes.Append(list);

            return tiles;
        }

        private static XmlElement CreateVector2Element(XmlDocument xmlDocument, string name)
        {
            XmlElement vector2 = xmlDocument.CreateElement(name);
            XmlAttribute element = xmlDocument.CreateAttribute("Element");
            XmlAttribute x = xmlDocument.CreateAttribute("x");
            XmlAttribute y = xmlDocument.CreateAttribute("y");
            x.Value = "0";
            y.Value = "0";
            vector2.Attributes.Append(element);
            vector2.Attributes.Append(x);
            vector2.Attributes.Append(y);

            return vector2;
        }

        public static XmlElement CreateTileElement(XmlDocument xmlDocument)
        {
            XmlElement tile = xmlDocument.CreateElement("Tile");
            XmlAttribute id = xmlDocument.CreateAttribute("ID");
            XmlAttribute rh = xmlDocument.CreateAttribute("RH");
            XmlAttribute rv = xmlDocument.CreateAttribute("RV");
            XmlAttribute frequency = xmlDocument.CreateAttribute("Frequency");
            tile.Attributes.Append(id);
            tile.Attributes.Append(rh);
            tile.Attributes.Append(rv);
            tile.Attributes.Append(frequency);
            tile.AppendChild(CreatePrefabElement(xmlDocument));
            tile.AppendChild(CreateConstraintsElement(xmlDocument));

            return tile;
        }

        private static XmlElement CreatePrefabElement(XmlDocument xmlDocument)
        {
            XmlElement prefab = xmlDocument.CreateElement("Prefab");
            XmlAttribute element = xmlDocument.CreateAttribute("Element");
            XmlAttribute path = xmlDocument.CreateAttribute("Path");
            prefab.Attributes.Append(element);
            prefab.Attributes.Append(path);

            return prefab;
        }

        private static XmlElement CreateConstraintsElement(XmlDocument xmlDocument)
        {
            XmlElement constraints = xmlDocument.CreateElement("Constraints");
            XmlAttribute list = xmlDocument.CreateAttribute("List");
            constraints.Attributes.Append(list);

            return constraints;
        }

        public static XmlElement CreateConstraintElement(XmlDocument xmlDocument)
        {
            XmlElement constraint = xmlDocument.CreateElement("Constraint");
            XmlAttribute direction = xmlDocument.CreateAttribute("Type");
            constraint.Attributes.Append(direction);
            constraint.AppendChild(CreateNeighboursElement(xmlDocument));

            return constraint;
        }

        private static XmlElement CreateNeighboursElement(XmlDocument xmlDocument)
        {
            XmlElement neighbours = xmlDocument.CreateElement("Neighbours");
            XmlAttribute list = xmlDocument.CreateAttribute("List");
            neighbours.Attributes.Append(list);

            return neighbours;
        }

        public static XmlElement CreateNeighbourElement(XmlDocument xmlDocument)
        {
            XmlElement neighbour = xmlDocument.CreateElement("Neighbour");
            XmlAttribute element = xmlDocument.CreateAttribute("Element");
            XmlAttribute id = xmlDocument.CreateAttribute("ID");
            neighbour.Attributes.Append(element);
            neighbour.Attributes.Append(id);

            return neighbour;
        }

        public static void SetNodeAttributeValue(XmlNode node, string attribute, string value)
        {
            node.Attributes[attribute].Value = value;
        }

        #endregion

        #region General

        public static string GetOppositeConstraintType(string contraintType)
        {
            string oppositeConstraintType = string.Empty;

            if (contraintType.Equals("ALL"))
            {
                oppositeConstraintType = "ALL";
            }
            else if (contraintType.Equals("NORTH"))
            {
                oppositeConstraintType = "SOUTH";
            }
            else if (contraintType.Equals("SOUTH"))
            {
                oppositeConstraintType = "NORTH";
            }
            else if (contraintType.Equals("EAST"))
            {
                oppositeConstraintType = "WEST";
            }
            else if (contraintType.Equals("WEST"))
            {
                oppositeConstraintType = "EAST";
            }
            else if (contraintType.Equals("VERTICAL"))
            {
                oppositeConstraintType = "VERTICAL";
            }
            else if (contraintType.Equals("HORIZONTAL"))
            {
                oppositeConstraintType = "HORIZONTAL";
            }
            else if (contraintType.Equals("N_NORTH"))
            {
                oppositeConstraintType = "N_SOUTH";
            }
            else if (contraintType.Equals("N_SOUTH"))
            {
                oppositeConstraintType = "N_NORTH";
            }
            else if (contraintType.Equals("N_EAST"))
            {
                oppositeConstraintType = "N_WEST";
            }
            else if (contraintType.Equals("N_WEST"))
            {
                oppositeConstraintType = "N_EAST";
            }

            return oppositeConstraintType;
        }

        #endregion
    }
}