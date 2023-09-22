using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

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
        private Dictionary<XmlNode, Dictionary<XmlNode, Dictionary<XmlNode, Dictionary<XmlNode, int>>>> _indexesPerNeighbour;
        private string[] _possibleConstraintTypes = { "ALL", "NORTH", "EAST", "SOUTH", "WEST", "HORIZONTAL", "VERTICAL", "N_NORTH", "N_EAST", "N_SOUTH", "N_WEST" };
        private Dictionary<XmlNode, Dictionary<XmlNode, Dictionary<XmlNode, int>>> _indexesPerConstraint;

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

            _indexesPerNeighbour = new Dictionary<XmlNode, Dictionary<XmlNode, Dictionary<XmlNode, Dictionary<XmlNode, int>>>>();
            foreach (XmlNode level in Utils.GetLevelNodes(_xmlDocument))
            {
                _indexesPerNeighbour[level] = new Dictionary<XmlNode, Dictionary<XmlNode, Dictionary<XmlNode, int>>>();
                foreach (XmlNode tile in Utils.GetTileNodes(level))
                {
                    _indexesPerNeighbour[level][tile] = new Dictionary<XmlNode, Dictionary<XmlNode, int>>();
                    foreach (XmlNode constraint in Utils.GetConstraintNodes(tile))
                    {
                        _indexesPerNeighbour[level][tile][constraint] = new Dictionary<XmlNode, int>();
                        foreach (XmlNode neighbour in Utils.GetNeighbourNodes(constraint))
                        {
                            string neighbourID = Utils.GetNodeAttributeValue(neighbour, "ID"); 
                            for (int i = 0; i < _tilesPerLevel[level].Count; i++)
                            {
                                if (!Utils.IsNodeAttributeEqual(_tilesPerLevel[level][i], "ID", neighbourID)) continue;
                                _indexesPerNeighbour[level][tile][constraint][neighbour] = i;
                                break;
                            }
                        }
                    }
                }
            }

            _indexesPerConstraint = new Dictionary<XmlNode, Dictionary<XmlNode, Dictionary<XmlNode, int>>>();
            foreach (XmlNode level in Utils.GetLevelNodes(_xmlDocument))
            {
                _indexesPerConstraint[level] = new Dictionary<XmlNode, Dictionary<XmlNode, int>>();
                foreach (XmlNode tile in Utils.GetTileNodes(level))
                {
                    _indexesPerConstraint[level][tile] = new Dictionary<XmlNode, int>();
                    foreach (XmlNode constraint in Utils.GetConstraintNodes(tile))
                    {
                        string constraintType = Utils.GetNodeAttributeValue(constraint, "Type");
                        for (int i = 0; i < _possibleConstraintTypes.Length; i++)
                        {
                            if (!Utils.IsNodeAttributeEqual(constraint, "Type", _possibleConstraintTypes[i])) continue;
                            _indexesPerConstraint[level][tile][constraint] = i;
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

        public XmlNode GetCurrentLevel()
        {
            return _currentLevel;
        }

        public XmlNode GetCurrentTile()
        {
            return _currentTile;
        }

        public XmlNode GetCurrentConstraint()
        {
            return _currentConstraint;
        }

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
            return _neighbourChoicesPerLevel[_currentLevel];
        }

        public int GetNeighbourIndex(XmlNode neighbour)
        {
           return _indexesPerNeighbour[_currentLevel][_currentTile][_currentConstraint][neighbour];
        }

        public string[] GetConstraintChoices()
        {
            return _possibleConstraintTypes;
        }

        public int GetConstraintIndex(XmlNode constraint)
        {
            return _indexesPerConstraint[_currentLevel][_currentTile][constraint];
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

        public XmlNode CreateNode(XmlNode parent, XmlNode currentLevel, XmlNode currentTile, XmlNode currentConstraint)
        {
            if (Utils.IsNodeNameEqual(parent, "Levels"))
            {
                return CreateLevel(parent);
            }
            else if (Utils.IsNodeNameEqual(parent, "Tiles"))
            {
                return CreateTile(parent, currentLevel);
            }
            else if (Utils.IsNodeNameEqual(parent, "Constraints"))
            {
                return CreateConstraint(parent, currentLevel, currentTile);
            }
            else if (Utils.IsNodeNameEqual(parent, "Neighbours"))
            {
                return CreateNeighbour(parent, currentLevel, currentTile, currentConstraint);
            }

            return null;
        }

        private XmlNode CreateLevel(XmlNode parent)
        {
            XmlNode level = parent.AppendChild(Utils.CreateLevelElement(_xmlDocument));

            _tilesPerLevel[level] = new List<XmlNode>();
            _neighbourChoicesPerLevel[level] = new string[0];
            _indexesPerNeighbour[level] = new Dictionary<XmlNode, Dictionary<XmlNode, Dictionary<XmlNode, int>>>();
            _indexesPerConstraint[level] = new Dictionary<XmlNode, Dictionary<XmlNode, int>>();

            return level;
        }

        private XmlNode CreateTile(XmlNode parent, XmlNode currentLevel)
        {
            XmlNode tile = parent.AppendChild(Utils.CreateTileElement(_xmlDocument));

            _tilesPerLevel[currentLevel].Add(tile);

            _neighbourChoicesPerLevel[currentLevel] = AddNewChoice(tile, _neighbourChoicesPerLevel[currentLevel]);
            _indexesPerNeighbour[currentLevel][tile] = new Dictionary<XmlNode, Dictionary<XmlNode, int>>();
            _indexesPerConstraint[currentLevel][tile] = new Dictionary<XmlNode, int>();

            return tile;
        }

        private string[] AddNewChoice(XmlNode tile, string[] oldChoices)
        {
            string[] newChoices = new string[oldChoices.Length + 1];

            for (int i = 0; i < newChoices.Length - 1; i++)
            {
                newChoices[i] = oldChoices[i];
            }
            newChoices[newChoices.Length - 1] = Utils.GetNodeAttributeValue(tile, "ID");

            return newChoices;
        }

        private XmlNode CreateConstraint(XmlNode parent, XmlNode currentLevel, XmlNode currentTile)
        {
            XmlNode constraint = parent.AppendChild(Utils.CreateConstraintElement(_xmlDocument));

            // Update DataStructures
            _indexesPerNeighbour[currentLevel][currentTile][constraint] = new Dictionary<XmlNode, int>();
            _indexesPerConstraint[currentLevel][currentTile][constraint] = -1;

            return constraint;
        }

        private XmlNode CreateNeighbour(XmlNode parent, XmlNode currentLevel, XmlNode currentTile, XmlNode currentConstraint)
        {
            XmlNode neighbour = parent.AppendChild(Utils.CreateNeighbourElement(_xmlDocument));

            _indexesPerNeighbour[currentLevel][currentTile][currentConstraint][neighbour] = -1;

            return neighbour;
        }

        public void DeleteNode(XmlNode parent, XmlNode child, XmlNode currentLevel, XmlNode currentTile, XmlNode currentConstraint)
        {
            if (Utils.IsNodeNameEqual(parent, "Levels"))
            {
                DeleteLevel(child);
            }
            else if (Utils.IsNodeNameEqual(parent, "Tiles"))
            {
                DeleteTile(child, currentLevel);
            }
            else if (Utils.IsNodeNameEqual(parent, "Constraints"))
            {
                DeleteConstraint(child, currentLevel, currentTile);
            }
            else if (Utils.IsNodeNameEqual(parent, "Neighbours"))
            {
                DeleteNeighbour(child, currentLevel, currentTile, currentConstraint);
            }

            parent.RemoveChild(child);
        }

        private void DeleteLevel(XmlNode level)
        {
            _tilesPerLevel.Remove(level);
            _neighbourChoicesPerLevel.Remove(level);
            _indexesPerNeighbour.Remove(level);
        }

        private void DeleteTile(XmlNode tile, XmlNode currentLevel)
        {
            foreach (XmlNode constraint in Utils.GetConstraintNodes(tile))
            {
                DeleteNode(constraint.ParentNode, constraint, currentLevel, tile, constraint);
            }

            int indexTileToRemove = _tilesPerLevel[_currentLevel].IndexOf(tile);
            _tilesPerLevel[_currentLevel].Remove(tile);

            string[] newChoices = new string[_tilesPerLevel[_currentLevel].Count];
            for (int i = 0; i < newChoices.Length; i++)
            {
                newChoices[i] = Utils.GetNodeAttributeValue(_tilesPerLevel[_currentLevel][i], "ID");
            }
            _neighbourChoicesPerLevel[_currentLevel] = newChoices;

            List<XmlNode> tiles = new List<XmlNode>();
            List<XmlNode> constraints = new List<XmlNode>();
            List<XmlNode> neighbours = new List<XmlNode>();

            foreach (XmlNode t in _indexesPerNeighbour[currentLevel].Keys)
            {
                foreach (XmlNode c in _indexesPerNeighbour[currentLevel][t].Keys)
                {
                    foreach (XmlNode n in _indexesPerNeighbour[currentLevel][t][c].Keys)
                    {
                        tiles.Add(t);
                        constraints.Add(c);
                        neighbours.Add(n);
                    }
                }
            }

            for (int i = 0; i < neighbours.Count; i++)
            {
                if (_indexesPerNeighbour[currentLevel][tiles[i]][constraints[i]][neighbours[i]] <= indexTileToRemove) continue;
                _indexesPerNeighbour[currentLevel][tiles[i]][constraints[i]][neighbours[i]]--;
            }

            _indexesPerNeighbour[currentLevel].Remove(tile);
            _indexesPerConstraint[currentLevel].Remove(tile);
        }

        private void DeleteConstraint(XmlNode constraint, XmlNode currentLevel, XmlNode currentTile)
        {
            foreach (XmlNode neighbour in Utils.GetNeighbourNodes(constraint))
            {
                DeleteNode(neighbour.ParentNode, neighbour, currentLevel, currentTile, constraint);
            }

            _indexesPerNeighbour[currentLevel][currentTile].Remove(constraint);
            _indexesPerConstraint[currentLevel][currentTile].Remove(constraint);
        }

        private void DeleteNeighbour(XmlNode neighbour, XmlNode currentLevel, XmlNode currentTile, XmlNode currentConstraint)
        {
            _indexesPerNeighbour[currentLevel][currentTile][currentConstraint].Remove(neighbour);

            XmlNode correspondingTile = Utils.GetCorrespondingTile(_tilesPerLevel[currentLevel], neighbour);
            string currentTileID = Utils.GetNodeAttributeValue(currentTile, "ID");
            string correspondingTileID = Utils.GetNodeAttributeValue(correspondingTile, "ID");
            string constraintType = Utils.GetNodeAttributeValue(Utils.GetConstraintNodeFromNeighbour(neighbour), "Type");
            string oppositeConstraintType = Utils.GetOppositeConstraintType(constraintType);

            if (correspondingTileID.Equals(currentTileID)) return;
            if (!Utils.HasNodeListAttributeEqual(Utils.GetConstraintNodes(correspondingTile), "Type", oppositeConstraintType, out XmlNode oldConstraint))
            {
                return;
            }
            XmlNodeList neighbours = Utils.GetNeighbourNodes(oldConstraint);
            if (!Utils.HasNodeListAttributeEqual(neighbours, "ID", currentTileID, out XmlNode oldNeighbour))
            {
                return;
            }

            oldNeighbour.ParentNode.RemoveChild(oldNeighbour);
            _indexesPerNeighbour[currentLevel][correspondingTile][oldConstraint].Remove(oldNeighbour);
        }

        public void EditLevel(XmlNode level)
        {
            return;
        }

        public void EditTile(XmlNode tile)
        {
            int index = _tilesPerLevel[_currentLevel].IndexOf(tile);
            _neighbourChoicesPerLevel[_currentLevel][index] = Utils.GetNodeAttributeValue(_tilesPerLevel[_currentLevel][index], "ID");
            return;
        }

        public void EditConstraint(XmlNode currentConstraint, int newIndex, int oldIndex)
        {
            _indexesPerConstraint[_currentLevel][_currentTile][currentConstraint] = newIndex;

            string newType = _possibleConstraintTypes[newIndex];
            string oldType = oldIndex == -1 ? null : _possibleConstraintTypes[oldIndex];
            string oppositeNewType = Utils.GetOppositeConstraintType(newType);
            string oppositeOldType = oldType == null ? null : Utils.GetOppositeConstraintType(oldType);
            int oppositeNewIndex = _possibleConstraintTypes.ToList().IndexOf(oppositeNewType);
            string currentTileID = Utils.GetNodeAttributeValue(_currentTile, "ID");

            Utils.SetNodeAttributeValue(currentConstraint, "Type", newType);

            foreach (XmlNode neighbour in Utils.GetNeighbourNodes(currentConstraint))
            {
                XmlNode tile = Utils.GetCorrespondingTile(_tilesPerLevel[_currentLevel], neighbour);

                // Adding new
                XmlNodeList constraints = Utils.GetConstraintNodes(tile);
                if (!Utils.HasNodeListAttributeEqual(constraints, "Type", oppositeNewType, out XmlNode newConstraint))
                {
                    newConstraint = CreateConstraint(Utils.GetConstraintsNodeList(tile), _currentLevel, tile);
                    Utils.SetNodeAttributeValue(newConstraint, "Type", oppositeNewType);
                    _indexesPerConstraint[_currentLevel][tile][newConstraint] = oppositeNewIndex;
                }
                XmlNodeList neighbours = Utils.GetNeighbourNodes(newConstraint);
                if (!Utils.HasNodeListAttributeEqual(neighbours, "ID", currentTileID, out XmlNode newNeighbour))
                {
                    newNeighbour = CreateNeighbour(Utils.GetNeighboursNodeList(newConstraint), _currentLevel, tile, newConstraint);
                    Utils.SetNodeAttributeValue(newNeighbour, "ID", currentTileID);
                    _indexesPerNeighbour[_currentLevel][tile][newConstraint][newNeighbour] = _tilesPerLevel[_currentLevel].IndexOf(_currentTile);
                }

                if (oldType == null) continue;

                //Deleting old
                if (!Utils.HasNodeListAttributeEqual(constraints, "Type", oppositeOldType, out XmlNode oldConstraint))
                {
                    continue;
                }

                neighbours = Utils.GetNeighbourNodes(oldConstraint);
                if (!Utils.HasNodeListAttributeEqual(neighbours, "ID", currentTileID, out XmlNode oldNeighbour))
                {
                    continue;
                }
                oldNeighbour.ParentNode.RemoveChild(oldNeighbour);
                _indexesPerNeighbour[_currentLevel][tile][oldConstraint].Remove(oldNeighbour);
            }
        }

        public void EditNeighbour(XmlNode neighbour, int newIndex, int oldIndex)
        {
            _indexesPerNeighbour[_currentLevel][_currentTile][_currentConstraint][neighbour] = newIndex;

            string newValue = Utils.GetNodeAttributeValue(_tilesPerLevel[_currentLevel][newIndex], "ID");
            string oldValue = oldIndex == -1 ? null : Utils.GetNodeAttributeValue(_tilesPerLevel[_currentLevel][oldIndex], "ID");
            string currentTileID = Utils.GetNodeAttributeValue(_currentTile, "ID");
            XmlNode newTile = _tilesPerLevel[_currentLevel][newIndex];
            string constraintType = Utils.GetNodeAttributeValue(Utils.GetConstraintNodeFromNeighbour(neighbour), "Type");
            string oppositeConstraintType = Utils.GetOppositeConstraintType(constraintType);
            int oppositeNewIndex = _possibleConstraintTypes.ToList().IndexOf(oppositeConstraintType);
            int currentTileIndex = _tilesPerLevel[_currentLevel].IndexOf(_currentTile);

            Utils.SetNodeAttributeValue(neighbour, "ID", newValue);

            // AddingNew
            if (!Utils.HasNodeListAttributeEqual(Utils.GetConstraintNodes(newTile), "Type", oppositeConstraintType, out XmlNode newConstraint))
            {
                newConstraint = CreateConstraint(Utils.GetConstraintsNodeList(newTile), _currentLevel, newTile);
                Utils.SetNodeAttributeValue(newConstraint, "Type", oppositeConstraintType);
                _indexesPerConstraint[_currentLevel][newTile][newConstraint] = oppositeNewIndex;
            }

            XmlNodeList neighbours = Utils.GetNeighbourNodes(newConstraint);
            if (!Utils.HasNodeListAttributeEqual(neighbours, "ID", currentTileID, out XmlNode newNeighbour))
            {
                newNeighbour = CreateNeighbour(Utils.GetNeighboursNodeList(newConstraint), _currentLevel, newTile, newConstraint);
                Utils.SetNodeAttributeValue(newNeighbour, "ID", currentTileID);
                _indexesPerNeighbour[_currentLevel][newTile][newConstraint][newNeighbour] = currentTileIndex;
            }

            if (oldValue == null) return;

            // DeletingOld
            XmlNode oldTile = _tilesPerLevel[_currentLevel][oldIndex];

            if (!Utils.HasNodeListAttributeEqual(Utils.GetConstraintNodes(oldTile), "Type", oppositeConstraintType, out XmlNode oldConstraint))
            {
                return;
            }
            neighbours = Utils.GetNeighbourNodes(oldConstraint);
            if (!Utils.HasNodeListAttributeEqual(neighbours, "ID", oldValue, out XmlNode oldNeighbour))
            {
                return;
            }
            oldNeighbour.ParentNode.RemoveChild(oldNeighbour);
            _indexesPerNeighbour[_currentLevel][oldTile][oldConstraint].Remove(oldNeighbour);
        }

        #endregion

        #region Debug

        public string PrintDebug()
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
            foreach (XmlNode level in _indexesPerNeighbour.Keys)
            {
                text = string.Concat(text, string.Format("\tLevel: {0}\n", Utils.GetHierarchyButtonText(level)));
                foreach (XmlNode tile in _indexesPerNeighbour[level].Keys)
                {
                    text = string.Concat(text, string.Format("\t\tTile: {0}\n", Utils.GetHierarchyButtonText(tile)));
                    foreach (XmlNode constraint in _indexesPerNeighbour[level][tile].Keys)
                    {
                        text = string.Concat(text, string.Format("\t\t\tConstraint: {0}\n", Utils.GetNodeAttributeValue(constraint, "Type")));
                        foreach (XmlNode neighbour in _indexesPerNeighbour[level][tile][constraint].Keys)
                        {
                            int index = _indexesPerNeighbour[level][tile][constraint][neighbour];
                            text = string.Concat(text, string.Format("\t\t\t\tNeighbour: {0} ({1})\n", index, index == -1 ? null : Utils.GetHierarchyButtonText(_tilesPerLevel[level][index])));
                        }
                    }
                }
            }

            return text;
        }

        private string DebugIndexesConstraintPerLevel()
        {
            string text = string.Empty;

            text = string.Concat(text, "IndexesConstraintPerLevel\n");
            foreach (XmlNode level in _indexesPerConstraint.Keys)
            {
                text = string.Concat(text, string.Format("\tLevel: {0}\n", Utils.GetHierarchyButtonText(level)));
                foreach (XmlNode tile in _indexesPerConstraint[level].Keys)
                {
                    text = string.Concat(text, string.Format("\t\tTile: {0}\n", Utils.GetHierarchyButtonText(tile)));
                    foreach (XmlNode constraint in _indexesPerConstraint[level][tile].Keys)
                    {
                        int index = _indexesPerConstraint[level][tile][constraint];
                        string type = index == -1 ? null : _possibleConstraintTypes[index];
                        text = string.Concat(text, string.Format("\t\t\tConstraint: {0} ({1})\n", index, index == -1 ? null : type));
                    }
                }
            }

            return text;
        }

        #endregion
    }
}