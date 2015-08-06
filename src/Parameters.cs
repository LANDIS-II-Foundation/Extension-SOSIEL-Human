// This file is part of the Social Human extension for LANDIS-II.

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.SocialHuman
{
    public class Parameters
    {
        private int timestep;
        private string inputMapNameTemplate;
        private string siteLogPath;

        //---------------------------------------------------------------------

        public int Timestep
        {
            get
            {
                return timestep;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                                                  "Timestep must be > or = 0");
                timestep = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Template for pathnames for input maps of land use.
        /// </summary>
        public string InputMaps
        {
            get
            {
                return inputMapNameTemplate;
            }
            set
            {
                if (value != null)
                {
                    MapNames.CheckTemplateVars(value);
                }
                inputMapNameTemplate = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Path to the CSV log file with biomass harvested per species at
        /// each timestep by individual sites.
        /// </summary>
        public string SiteLogPath
        {
            get
            {
                return siteLogPath;
            }
            set
            {
                if (value == "")
                    throw new InputValueException(value.ToString(),
                                                  "Site log path is empty string");
                siteLogPath = value;
            }
        }
    }
}
