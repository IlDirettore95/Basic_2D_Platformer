using System.Collections.Generic;
using System.Xml;

namespace GMDG.Basic2DPlatformer.Tools.XML
{
    public class Validator
    {
        private View _view;

        #region Init

        public Validator(View view)
        {
            _view = view;
        }

        #endregion

        #region ValidateXML

        public bool ValidateXmlDocument(XmlDocument document)
        {
            bool errorsFound = false;

            _view.Message = string.Empty;

            foreach (XmlNode level in Utils.GetLevelNodes(document))
            {
                if (!ValidateLevel(level))
                {
                    errorsFound = true;
                    break;
                }

                XmlNode settings = Utils.GetSettingsNode(level);

                if (!ValidateSettings(settings))
                {
                    errorsFound = true;
                    break;
                }

                foreach (XmlNode tile in Utils.GetTileNodes(level))
                {
                    if (!ValidateTile(tile))
                    {
                        errorsFound = true;
                        break;
                    }

                    foreach (XmlNode constraint in Utils.GetConstraintNodes(tile))
                    {
                        foreach (XmlNode neighbour in Utils.GetNeighbourNodes(constraint))
                        {
                            if (!ValidateNeighbour(neighbour, level, tile, constraint))
                            {
                                errorsFound = true;
                                break;
                            }
                        }

                        if (errorsFound)
                        {
                            break;
                        }

                    }

                    if (errorsFound)
                    {
                        break;
                    }

                }

                if (errorsFound)
                {
                    break;
                }
            }

            if (errorsFound) _view.Message += " (Validation Unsuccessful)";
            else _view.Message += " (Validation Successful)";

            return !errorsFound;
        }

        private bool ValidateLevel(XmlNode level)
        {
            if (Utils.IsNodeAttributeEqual(level, "ID", string.Empty))
            {
                _view.Message = "All Levels must have a string ID";
                return false;
            }

            return true;
        }

        private bool ValidateSettings(XmlNode settings)
        {
            // GridSize

            if (!int.TryParse(Utils.GetNodeAttributeValue(settings["GridSize"], "x"), out int xGridSize))
            {
                _view.Message = "GridSize X must be an integer";
                return false;
            }
            if (!int.TryParse(Utils.GetNodeAttributeValue(settings["GridSize"], "y"), out int yGridSize))
            {
                _view.Message = "GridSize Y must be an integer";
                return false;
            }

            // CellSize
            if (!float.TryParse(Utils.GetNodeAttributeValue(settings["CellSize"], "x"), out float xCellSize))
            {
                _view.Message = "CellSize X must be a float";
                return false;
            }
            if (!float.TryParse(Utils.GetNodeAttributeValue(settings["CellSize"], "y"), out float yCellSize))
            {
                _view.Message = "CellSize Y must be a float";
                return false;
            }


            // StartingCell
            if (!int.TryParse(Utils.GetNodeAttributeValue(settings["StartingCell"], "x"), out int xStartingCell))
            {
                _view.Message = "StartingCell X must be an integer";
                return false;
            }
            if (!int.TryParse(Utils.GetNodeAttributeValue(settings["StartingCell"], "y"), out int yStartingCell))
            {
                _view.Message = "StartingCell Y must be an integer";
                return false;
            }

            // EndingCell
            if (!int.TryParse(Utils.GetNodeAttributeValue(settings["EndingCell"], "x"), out int xEndingCell))
            {
                _view.Message = "EndingCell X must be an integer";
                return false;
            }
            if (!int.TryParse(Utils.GetNodeAttributeValue(settings["EndingCell"], "y"), out int yEndingCell))
            {
                _view.Message = "EndingCell Y must be an integer";
                return false;
            }

            // EndingCell
            if (!int.TryParse(Utils.GetNodeAttributeValue(settings["PassageCell"], "x"), out int xPassageCell))
            {
                _view.Message = "PassageCell X must be an integer";
                return false;
            }
            if (!int.TryParse(Utils.GetNodeAttributeValue(settings["PassageCell"], "y"), out int yPassageCell))
            {
                _view.Message = "PassageCell Y must be an integer";
                return false;
            }

            bool gridSizeOk = xGridSize > 0 && yGridSize > 0;
            bool cellSizeOk = xCellSize > 0 && yCellSize > 0;
            bool startingCellOk = xStartingCell >= 0 && xStartingCell < xGridSize && yStartingCell >= 0 && yStartingCell < yGridSize;
            bool endingCellOk = xEndingCell >= 0 && xEndingCell < xGridSize && yEndingCell >= 0 && yEndingCell < yGridSize;
            bool passageCellOk = xPassageCell >= 0 && xPassageCell < xGridSize && yPassageCell >= 0 && yPassageCell < yGridSize;
            bool startingEndingCellOk = xStartingCell != xEndingCell || yStartingCell != yEndingCell;

            if (!gridSizeOk)
            {
                _view.Message = "GridSize must be above 0!";
                return false;
            }

            if (!cellSizeOk)
            {
                _view.Message = "CellSize must be above 0!";
                return false;
            }

            if (!startingCellOk)
            {
                _view.Message = "Starting cell must be in GridSize";
                return false;
            }

            if (!endingCellOk)
            {
                _view.Message = "Ending cell must be in GridSize";
                return false;
            }


            if (!passageCellOk)
            {
                _view.Message = "Passage cell must be in GridSize";
                return false;
            }

            if (!startingEndingCellOk)
            {
                _view.Message = "Starting and ending cell cannot be on the same";
                return false;
            }

            return true;
        }

        private bool ValidateTile(XmlNode tile)
        {
            string tileName = Utils.GetNodeAttributeValue(tile, "ID");
            
            if (!int.TryParse(Utils.GetNodeAttributeValue(tile, "Frequency"), out int frequency))
            {
                _view.Message = string.Format("Frequency must be an integer on Tile ({0})", tileName);
                return false;
            }

            if (frequency <= 0)
            {
                _view.Message = string.Format("Frequency must be above 0 on Tile ({0})", tileName);
                return false;
            }
            
            if (Utils.IsNodeAttributeEqual(tile["Prefab"], "Path", string.Empty))
            {
                //_view.Message = string.Format("Prefab is missing on Tile ({0})", tileName);
                //return false;
            }

            return true;
        }

        private bool ValidateNeighbour(XmlNode neighbour, XmlNode currentLevel, XmlNode currentTile, XmlNode currentConstraint)
        {
            XmlNode correspondingTile = null;

            foreach (XmlNode tile in Utils.GetTileNodes(currentLevel))
            {
                if (!Utils.IsNodeAttributeEqual(tile, "ID", Utils.GetNodeAttributeValue(neighbour, "ID"))) continue;
                correspondingTile = tile;
            }

            if (correspondingTile == null) 
            {
                _view.Message = string.Format("Neighbour ({0}) is not found in Tiles of Level ({1})", Utils.GetNodeAttributeValue(neighbour, "ID"), Utils.GetNodeAttributeValue(currentLevel, "ID"));
                return true;
            }

            string currentTileName = Utils.GetNodeAttributeValue(currentTile, "ID");
            string correspondingTileName = Utils.GetNodeAttributeValue(correspondingTile, "ID");
            string constraintType = Utils.GetNodeAttributeValue(currentConstraint, "Type");

            if (!ValidateAdjacenciesPerConstarintType(currentTile, constraintType, correspondingTile))
            {
                _view.Message = string.Format("Tile ({0}) cannot be near Tile ({1}) with constraint ({2})", currentTileName, correspondingTileName, constraintType);
                return false;
            }

            return true;
        }

        public bool ValidateAdjacenciesPerConstarintType(XmlNode currentTile, string constraintType, XmlNode otherTile)
        {
            bool currentNIn = bool.Parse(Utils.GetNodeAttributeValue(currentTile, "N_IN"));
            bool currentNOut = bool.Parse(Utils.GetNodeAttributeValue(currentTile, "N_OUT"));
            bool currentEIn = bool.Parse(Utils.GetNodeAttributeValue(currentTile, "E_IN"));
            bool currentEOut = bool.Parse(Utils.GetNodeAttributeValue(currentTile, "E_OUT"));
            bool currentSIn = bool.Parse(Utils.GetNodeAttributeValue(currentTile, "S_IN"));
            bool currentSOut = bool.Parse(Utils.GetNodeAttributeValue(currentTile, "S_OUT"));
            bool currentWIn = bool.Parse(Utils.GetNodeAttributeValue(currentTile, "W_IN"));
            bool currentWOut = bool.Parse(Utils.GetNodeAttributeValue(currentTile, "W_OUT"));

            bool otherNIn = bool.Parse(Utils.GetNodeAttributeValue(otherTile, "N_IN"));
            bool otherNOut = bool.Parse(Utils.GetNodeAttributeValue(otherTile, "N_OUT"));
            bool otherEIn = bool.Parse(Utils.GetNodeAttributeValue(otherTile, "E_IN"));
            bool otherEOut = bool.Parse(Utils.GetNodeAttributeValue(otherTile, "E_OUT"));
            bool otherSIn = bool.Parse(Utils.GetNodeAttributeValue(otherTile, "S_IN"));
            bool otherSOut = bool.Parse(Utils.GetNodeAttributeValue(otherTile, "S_OUT"));
            bool otherWIn = bool.Parse(Utils.GetNodeAttributeValue(otherTile, "W_IN"));
            bool otherWOut = bool.Parse(Utils.GetNodeAttributeValue(otherTile, "W_OUT"));

            if (constraintType.Equals("ALL"))
            {
                if(!ValidateDirectionFlags(currentNIn, currentNOut, otherSIn, otherSOut))
                {
                    return false;
                }
                if (!ValidateDirectionFlags(currentEIn, currentEOut, otherWIn, otherWOut))
                {
                    return false;
                }
                if (!ValidateDirectionFlags(currentSIn, currentSOut, otherNIn, otherNOut))
                {
                    return false;
                }
                if (!ValidateDirectionFlags(currentWIn, currentWOut, otherEIn, otherEOut))
                {
                    return false;
                }
            }
            else if (constraintType.Equals("NORTH"))
            {
                if (!ValidateDirectionFlags(currentNIn, currentNOut, otherSIn, otherSOut))
                {
                    return false;
                }
            }
            else if (constraintType.Equals("SOUTH"))
            {
                if (!ValidateDirectionFlags(currentSIn, currentSOut, otherNIn, otherNOut))
                {
                    return false;
                }
            }
            else if (constraintType.Equals("EAST"))
            {
                if (!ValidateDirectionFlags(currentEIn, currentEOut, otherWIn, otherWOut))
                {
                    return false;
                }
            }
            else if (constraintType.Equals("WEST"))
            {
                if (!ValidateDirectionFlags(currentWIn, currentWOut, otherEIn, otherEOut))
                {
                    return false;
                }
            }
            else if (constraintType.Equals("VERTICAL"))
            {
                if (!ValidateDirectionFlags(currentNIn, currentNOut, otherSIn, otherSOut))
                {
                    return false;
                }
                if (!ValidateDirectionFlags(currentSIn, currentSOut, otherNIn, otherNOut))
                {
                    return false;
                }
            }
            else if (constraintType.Equals("HORIZONTAL"))
            {
                if (!ValidateDirectionFlags(currentEIn, currentEOut, otherWIn, otherWOut))
                {
                    return false;
                }
                if (!ValidateDirectionFlags(currentWIn, currentWOut, otherEIn, otherEOut))
                {
                    return false;
                }
            }
            else if (constraintType.Equals("N_NORTH"))
            {
                if (!ValidateDirectionFlags(currentEIn, currentEOut, otherWIn, otherWOut))
                {
                    return false;
                }
                if (!ValidateDirectionFlags(currentSIn, currentSOut, otherNIn, otherNOut))
                {
                    return false;
                }
                if (!ValidateDirectionFlags(currentWIn, currentWOut, otherEIn, otherEOut))
                {
                    return false;
                }
            }
            else if (constraintType.Equals("N_SOUTH"))
            {
                if (!ValidateDirectionFlags(currentNIn, currentNOut, otherSIn, otherSOut))
                {
                    return false;
                }
                if (!ValidateDirectionFlags(currentEIn, currentEOut, otherWIn, otherWOut))
                {
                    return false;
                }
                if (!ValidateDirectionFlags(currentWIn, currentWOut, otherEIn, otherEOut))
                {
                    return false;
                }
            }
            else if (constraintType.Equals("N_EAST"))
            {
                if (!ValidateDirectionFlags(currentNIn, currentNOut, otherSIn, otherSOut))
                {
                    return false;
                }
                if (!ValidateDirectionFlags(currentSIn, currentSOut, otherNIn, otherNOut))
                {
                    return false;
                }
                if (!ValidateDirectionFlags(currentWIn, currentWOut, otherEIn, otherEOut))
                {
                    return false;
                }
            }
            else if (constraintType.Equals("N_WEST"))
            {
                if (!ValidateDirectionFlags(currentNIn, currentNOut, otherSIn, otherSOut))
                {
                    return false;
                }
                if (!ValidateDirectionFlags(currentEIn, currentEOut, otherWIn, otherWOut))
                {
                    return false;
                }
                if (!ValidateDirectionFlags(currentSIn, currentSOut, otherNIn, otherNOut))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidateDirectionFlags(bool currentIn, bool currentOut, bool otherIn, bool otherOut)
        {
            return ((currentIn == otherOut) && (currentOut == otherIn));
        }

        #endregion
    }
}
