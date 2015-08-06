// This file is part of the Social Human extension for LANDIS-II.

using Landis.Core;
using Landis.SpatialModeling;

namespace Landis.Extension.SocialHuman
{
    public class SiteVars
    {
        private static ISiteVar<LandUse> landUse;
        private static AllowHarvestSiteVar allowHarvest;

        //---------------------------------------------------------------------

        public static void Initialize(ICore modelCore)
        {
            landUse = modelCore.Landscape.NewSiteVar<LandUse>();
            allowHarvest = new AllowHarvestSiteVar();
            Model.Core.RegisterSiteVar(allowHarvest, "LandUse.AllowHarvest");
        }

        //---------------------------------------------------------------------

        public static ISiteVar<LandUse> LandUse
        {
            get {
                return landUse;
            }
        }
    }
}
