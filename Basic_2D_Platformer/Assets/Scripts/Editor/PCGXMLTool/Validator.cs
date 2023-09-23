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
                }

                if (errorsFound)
                {
                    break;
                }
            }

            if (errorsFound) _view.Message += " (Validation Unsuccessful)";

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
            
            if (!int.TryParse(Utils.GetNodeAttributeValue(tile, "Frequency"), out int frequency))
            {
                _view.Message = "frequency must be an integer";
                return false;
            }

            if (frequency <= 0)
            {
                _view.Message = "frequency must be above 0";
                return false;
            }
            
            if (Utils.IsNodeAttributeEqual(tile["Prefab"], "Path", string.Empty))
            {
                _view.Message = "prefab asset is missing";
                return false;
            }

            return true;
        }

        #endregion
    }
}
