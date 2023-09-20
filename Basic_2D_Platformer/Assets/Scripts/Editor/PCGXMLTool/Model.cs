using log4net.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEditor.Rendering.LookDev;
using UnityEngine;


namespace GMDG.Basic2DPlatformer.Tools.XML
{
    public class Model
    {
        private View _view;
        private Validator _validator;

        private string _path = "XML/WorldGenerationData";
        private XmlDocument _xmlDocument;

        // Data Structure
        private XmlNode _currentLevel;
        private XmlNode _currentTile;
        private XmlNode _currentConstraint;
        private XmlNode _currentNode;
        private List<XmlNode> _hierarchy; 
        private Dictionary<XmlNode, List<XmlNode>> _tilesPerLevel;
        private Dictionary<XmlNode, string[]> _neighbourChoicesPerLevel;
        private Dictionary<XmlNode, Dictionary<XmlNode, int>> _indexesNeighbourPerLevel;
        private string[] _possibleConstraintTypes = { "ALL", "NORTH", "EAST", "SOUTH", "WEST", "HORIZONTAL", "VERTICAL", "N_NORTH", "N_EAST", "N_SOUTH", "N_WEST" };
        private Dictionary<XmlNode, Dictionary<XmlNode, int>> _indexesConstraintPerLevel;

        #region Init

        public void Init(View view)
        {
            _view = view;
            _validator = new Validator(_view);

            InitXml();
            InitDataStructures();
        }

        private void InitXml()
        {
            TextAsset textAsset = Resources.Load<TextAsset>(_path);

            if (textAsset == null)
            {
                _view.Message = string.Format("File {0} not found!", _path);
                return;
            }

            _xmlDocument = new XmlDocument();
            try
            {
                _xmlDocument.LoadXml(textAsset.text);
            }
            catch (XmlException)
            {
                _xmlDocument.AppendChild(Utils.CreateRootElement(_xmlDocument));
                Save();
                _xmlDocument.LoadXml(textAsset.text);
            }

            _view.Message = string.Format("File {0} loaded!", _path);
        }

        private void InitDataStructures()
        {
            _currentNode = _xmlDocument.DocumentElement;

            _hierarchy= new List<XmlNode>();
            _hierarchy.Add(_xmlDocument.DocumentElement);

            _tilesPerLevel = new Dictionary<XmlNode, List<XmlNode>>();
            foreach (XmlNode level in Utils.GetLevelNodes(_xmlDocument))
            {
                _tilesPerLevel[level] = new List<XmlNode>();
                foreach (XmlNode tile in Utils.GetTileNodes(level))
                {
                    _tilesPerLevel[level].Add(tile);
                }
            }

            _neighbourChoicesPerLevel = new Dictionary<XmlNode, string[]>();
            foreach (XmlNode level in Utils.GetLevelNodes(_xmlDocument))
            {
                _neighbourChoicesPerLevel[level] = new string[_tilesPerLevel[level].Count];
                for (int i = 0; i < _neighbourChoicesPerLevel[level].Length; i++)
                {
                    _neighbourChoicesPerLevel[level][i] = Utils.GetNodeAttributeValue(_tilesPerLevel[level][i], "ID");
                }
            }

            _indexesNeighbourPerLevel = new Dictionary<XmlNode, Dictionary<XmlNode, int>>();
            foreach (XmlNode level in Utils.GetLevelNodes(_xmlDocument))
            {
                _indexesNeighbourPerLevel[level] = new Dictionary<XmlNode, int>();
                foreach (XmlNode tile in Utils.GetTileNodes(level))
                {
                    foreach (XmlNode constraint in Utils.GetConstraintNodes(tile))
                    {
                        foreach (XmlNode neighbour in Utils.GetNeighbourNodes(constraint))
                        {
                            string neighbourID = Utils.GetNodeAttributeValue(neighbour, "ID"); 
                            for (int i = 0; i < _tilesPerLevel[level].Count; i++)
                            {
                                if (!Utils.IsNodeAttributeEqual(_tilesPerLevel[level][i], "ID", neighbourID)) continue;
                                _indexesNeighbourPerLevel[level][neighbour] = i;
                                break;
                            }
                        }
                    }
                }
            }

            _indexesConstraintPerLevel = new Dictionary<XmlNode, Dictionary<XmlNode, int>>();
            foreach (XmlNode level in Utils.GetLevelNodes(_xmlDocument))
            {
                _indexesConstraintPerLevel[level] = new Dictionary<XmlNode, int>();
                foreach (XmlNode tile in Utils.GetTileNodes(level))
                {
                    foreach (XmlNode constraint in Utils.GetConstraintNodes(tile))
                    {
                        string constraintType = Utils.GetNodeAttributeValue(constraint, "Type");
                        for (int i = 0; i < _possibleConstraintTypes.Length; i++)
                        {
                            if (!Utils.IsNodeAttributeEqual(constraint, "Type", constraintType)) continue;
                            _indexesConstraintPerLevel[level][constraint] = i;
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Hierarchy

        public List<XmlNode> GetHierarchy()
        {
            return _hierarchy;
        }

        public void HierarchyButtonClicked(int i)
        {
            for (int j = i + 1; j < _hierarchy.Count;)
            {
                if (_hierarchy[j] == _currentLevel) _currentLevel = null;
                else if (_hierarchy[j] == _currentTile) _currentTile = null;
                else if (_hierarchy[j] == _currentConstraint) _currentConstraint = null;

                _hierarchy.RemoveAt(j);
            }

            _currentNode = _hierarchy[i];
        }

        #endregion

        #region CurrentChildren

        public XmlNode GetCurrentNode()
        {
            return _currentNode;
        }

        public void CurrentChildrenButtonClicked(XmlNode child)
        {
            _currentNode = child;

            if (Utils.IsNodeNameEqual(child, "Level")) _currentLevel = child;
            else if (Utils.IsNodeNameEqual(child, "Tile")) _currentTile = child;
            else if (Utils.IsNodeNameEqual(child, "Constraint")) _currentConstraint = child;
            else if (Utils.HasNodeAttribute(child, "List")) return;

            _hierarchy.Add(child);
        }

        public string[] GetNeighbourChoices()
        {
            for (int i = 0; i < _tilesPerLevel[_currentLevel].Count; i++)
            {
                _neighbourChoicesPerLevel[_currentLevel][i] = Utils.GetNodeAttributeValue(_tilesPerLevel[_currentLevel][i], "ID");
            }
            return _neighbourChoicesPerLevel[_currentLevel];
        }

        public int GetNeighbourIndex(XmlNode neighbour)
        {
           return _indexesNeighbourPerLevel[_currentLevel][neighbour];
        }

        public void SetNeighbourIndex(XmlNode neighbour, int newIndex)
        {
            _indexesNeighbourPerLevel[_currentLevel][neighbour] = newIndex;
        }

        public void SetNeighbourValue(XmlNode neighbour, int index)
        {
            string newValue = Utils.GetNodeAttributeValue(_tilesPerLevel[_currentLevel][index], "ID");
            _neighbourChoicesPerLevel[_currentLevel][index] = newValue;
            Utils.SetNodeAttributeValue(neighbour, "ID", newValue);
        }

        public string[] GetConstraintChoices()
        {
            return _possibleConstraintTypes;
        }

        public int GetConstraintIndex(XmlNode constraint)
        {
            return _indexesConstraintPerLevel[_currentLevel][constraint];
        }

        public void SetConstraintIndex(XmlNode constraint, int newIndex)
        {
            _indexesConstraintPerLevel[_currentLevel][constraint] = newIndex;
        }

        public void SetConstraintValue(XmlNode constraint, int index)
        {
            string type = _possibleConstraintTypes[index];
            Utils.SetNodeAttributeValue(constraint, "Type", type);
        }


        #endregion

        #region FileActions

        public void Reload()
        {
            AssetDatabase.Refresh();
            InitXml();
            InitDataStructures();
        }

        public void Validate()
        {
            _validator.ValidateXmlDocument(_xmlDocument);
        }

        public void Save()
        {
            _xmlDocument.Save(Application.dataPath + "/Resources/" + _path + ".xml");
            _view.Message = string.Format("File {0} saved!", _path);
            AssetDatabase.Refresh();
        }

        public void ValidateAndSave()
        {
            if (_validator.ValidateXmlDocument(_xmlDocument))
            {
                _xmlDocument.Save(Application.dataPath + "/Resources/" + _path + ".xml");
                _view.Message = string.Format("File {0} saved!", _path);
                AssetDatabase.Refresh();
            }
        }

        #endregion

        #region WriteXML

        public void CreateNode(XmlNode node)
        {
            if (Utils.IsNodeNameEqual(node, "Levels"))
            {
                XmlNode level = node.AppendChild(Utils.CreateLevelElement(_xmlDocument));

                // Update DataStructures
                _tilesPerLevel[level] = new List<XmlNode>();

                _neighbourChoicesPerLevel[level] = new string[0];

                _indexesNeighbourPerLevel[level] = new Dictionary<XmlNode, int>();

                _indexesConstraintPerLevel[level] = new Dictionary<XmlNode, int>();
            }
            else if (Utils.IsNodeNameEqual(node, "Tiles"))
            {
                XmlNode tile = node.AppendChild(Utils.CreateTileElement(_xmlDocument));

                // Update DataStructures
                _tilesPerLevel[_currentLevel].Add(tile);

                string[] newChoices = new string[_tilesPerLevel[_currentLevel].Count];
                for (int i = 0; i < newChoices.Length - 1; i++)
                {
                    newChoices[i] = _neighbourChoicesPerLevel[_currentLevel][i];
                    _neighbourChoicesPerLevel[_currentLevel][i] = Utils.GetNodeAttributeValue(_tilesPerLevel[_currentLevel][i], "ID");
                }
                newChoices[newChoices.Length - 1] = Utils.GetNodeAttributeValue(tile, "ID");
                _neighbourChoicesPerLevel[_currentLevel] = newChoices;
            }
            else if (Utils.IsNodeNameEqual(node, "Constraints"))
            {
                XmlNode constraint = node.AppendChild(Utils.CreateConstraintElement(_xmlDocument));

                // Update DataStructures
                _indexesConstraintPerLevel[_currentLevel][constraint] = -1;
            }
            else if (Utils.IsNodeNameEqual(node, "Neighbours"))
            {
                XmlNode neighbour = node.AppendChild(Utils.CreateNeighbourElement(_xmlDocument));

                // Update DataStructures
                _indexesNeighbourPerLevel[_currentLevel][neighbour] = -1;
            }
        }

        public void DeleteNode(XmlNode parent, XmlNode child)
        {
            parent.RemoveChild(child);

            if (Utils.IsNodeNameEqual(parent, "Levels"))
            {
                // Update DataStructures
                _tilesPerLevel.Remove(child);
                _neighbourChoicesPerLevel.Remove(child);
                _indexesNeighbourPerLevel.Remove(child);
            }
            else if (Utils.IsNodeNameEqual(parent, "Tiles"))
            {
                // Update DataStructures
                UpdateDataStructureAfterTileRemoved(child);
            }
            else if (Utils.IsNodeNameEqual(parent, "Constraints"))
            {
                // Update DataStructures
                _indexesConstraintPerLevel[_currentLevel].Remove(child);
            }
            else if (Utils.IsNodeNameEqual(parent, "Neighbours"))
            {
                // Update DataStructures
                _indexesNeighbourPerLevel[_currentLevel].Remove(child);
            }
        }

        private void UpdateDataStructureAfterTileRemoved(XmlNode tile)
        {
            int indexTileToRemove = _tilesPerLevel[_currentLevel].IndexOf(tile);
            _tilesPerLevel[_currentLevel].Remove(tile);

            string[] newChoices = new string[_tilesPerLevel[_currentLevel].Count];
            for (int i = 0; i < newChoices.Length; i++)
            {
                newChoices[i] = Utils.GetNodeAttributeValue(_tilesPerLevel[_currentLevel][i], "ID");
            }
            _neighbourChoicesPerLevel[_currentLevel] = newChoices;

            List<XmlNode> neighboursToRemove = new List<XmlNode>();
            foreach (XmlNode neighbour in new List<XmlNode>(_indexesNeighbourPerLevel[_currentLevel].Keys))
            {
                if (_indexesNeighbourPerLevel[_currentLevel][neighbour] > indexTileToRemove)
                {
                    _indexesNeighbourPerLevel[_currentLevel][neighbour]--;
                }
                else if (Utils.IsNodeAttributeEqual(neighbour, "ID", Utils.GetNodeAttributeValue(tile, "ID")))
                {
                    neighboursToRemove.Add(neighbour);
                }
            }
            for (int i = 0; i < neighboursToRemove.Count; i++)
            {
                XmlNode constraint = neighboursToRemove[i].ParentNode;
                constraint.RemoveChild(neighboursToRemove[i]);
                if (constraint.ChildNodes.Count == 0)
                {
                    constraint.ParentNode.ParentNode.RemoveChild(constraint.ParentNode);
                }
                _indexesNeighbourPerLevel[_currentLevel].Remove(neighboursToRemove[i]);
            }
        }

        #endregion

        #region Debug

        public string Debug()
        {
            string text = string.Empty;

            text = string.Concat(text, DebugCurrentNodes());
            text = string.Concat(text, DebugHierarchy());
            text = string.Concat(text, DebugTilesPerLevel());
            text = string.Concat(text, DebugNeighbourChoicesPerLevel());
            text = string.Concat(text, DebugIndexesNeighbourPerLevel());
            text = string.Concat(text, DebugIndexesConstraintPerLevel());

            return text;
        }

        private string DebugCurrentNodes()
        {
            string text = string.Empty;

            text = string.Concat(text, string.Format("CurrentLevel: {0}\n", _currentLevel == null ? "" : Utils.GetNodeAttributeValue(_currentLevel, "ID")));
            text = string.Concat(text, string.Format("CurrentTile: {0}\n", _currentTile == null ? "" : Utils.GetNodeAttributeValue(_currentTile, "ID")));
            text = string.Concat(text, string.Format("CurrentConstraint: {0}\n", _currentConstraint == null ? "" : Utils.GetNodeAttributeValue(_currentConstraint, "Type")));
            text = string.Concat(text, string.Format("CurrentNode: {0}\n", _currentNode == null ? "" : Utils.GetHierarchyButtonText(_currentNode)));

            return text;
        }

        private string DebugHierarchy()
        {
            string text = string.Empty;

            text = string.Concat(text, "Hierarchy\n");
            for (int i = 0; i < _hierarchy.Count; i++)
            {
                XmlNode node = _hierarchy[i];

                text = string.Concat(text, string.Format("\tNode: {0}\n", Utils.GetHierarchyButtonText(node)));
            }

            return text;
        }

        private string DebugTilesPerLevel()
        {
            string text = string.Empty;

            text = string.Concat(text, "TilesPerLevel\n");
            foreach (XmlNode level in _tilesPerLevel.Keys)
            {
                text = string.Concat(text, string.Format("\tLevel: {0}\n", Utils.GetHierarchyButtonText(level)));
                for (int i = 0; i < _tilesPerLevel[level].Count; i++)
                {
                    XmlNode tile = _tilesPerLevel[level][i];
                    text = string.Concat(text, string.Format("\t\tTile: {0}\n", Utils.GetHierarchyButtonText(tile)));
                }
            }

            return text;
        }

        private string DebugNeighbourChoicesPerLevel()
        {
            string text = string.Empty;

            text = string.Concat(text, "NeighbourChoicesPerLevel\n");
            foreach (XmlNode level in _neighbourChoicesPerLevel.Keys)
            {
                text = string.Concat(text, string.Format("\tLevel: {0}\n", Utils.GetHierarchyButtonText(level)));
                for (int i = 0; i < _neighbourChoicesPerLevel[level].Length; i++)
                {
                    text = string.Concat(text, string.Format("\t\tChoice: {0}\n", _neighbourChoicesPerLevel[level][i]));
                }
            }

            return text;
        }

        private string DebugIndexesNeighbourPerLevel()
        {
            string text = string.Empty;

            text = string.Concat(text, "IndexesNeighbourPerLevel\n");
            foreach (XmlNode level in _indexesNeighbourPerLevel.Keys)
            {
                text = string.Concat(text, string.Format("\tLevel: {0}\n", Utils.GetHierarchyButtonText(level)));
                foreach (XmlNode neighbour in _indexesNeighbourPerLevel[level].Keys)
                {
                    int index = _indexesNeighbourPerLevel[level][neighbour];
                    XmlNode tile = index == -1 ? null : _tilesPerLevel[level][index];
                    text = string.Concat(text, string.Format("\t\tNeighbour: {0} ({1})\n", index, index == -1 ? null : Utils.GetHierarchyButtonText(tile)));
                }
            }

            return text;
        }

        private string DebugIndexesConstraintPerLevel()
        {
            string text = string.Empty;

            text = string.Concat(text, "IndexesConstraintPerLevel\n");
            foreach (XmlNode level in _indexesConstraintPerLevel.Keys)
            {
                text = string.Concat(text, string.Format("\tLevel: {0}\n", Utils.GetHierarchyButtonText(level)));
                foreach (XmlNode constraint in _indexesConstraintPerLevel[level].Keys)
                {
                    int index = _indexesConstraintPerLevel[level][constraint];
                    string type = index == -1 ? null : _possibleConstraintTypes[index];
                    text = string.Concat(text, string.Format("\t\tConstraint: {0} ({1})\n", index, index == -1 ? null : type));
                }
            }

            return text;
        }

        #endregion
    }
}