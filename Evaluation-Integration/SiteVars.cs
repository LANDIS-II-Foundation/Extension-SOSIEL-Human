// This file is part of the Social Human extension for LANDIS-II.

using Landis.Core;
using Landis.SpatialModeling;

namespace SocialHuman
{
    public class SiteVars
    {
        private static ISiteVar<int> biomass;

        //---------------------------------------------------------------------

        public static void Initialize()
        {
            foreach (ActiveSite site in Model.Core.Landscape)
                SiteVars.Biomass[site] = 200;

        }

        //---------------------------------------------------------------------

        public static ISiteVar<int> Biomass
        {
            get
            {
                return biomass;
            }
        }
    }
}
