// This file is part of the Social Human extension for LANDIS-II.

using Landis.Library.SiteHarvest;
using Landis.Library.Succession;
using Landis.SpatialModeling;
using log4net;

namespace Landis.Extension.SocialHuman.LandCover
{
    class RemoveTrees
        : IChange
    {
        public const string TypeName = "RemoveTrees";
        private ICohortCutter cohortCutter;
        private Planting.SpeciesList speciesToPlant;
        private static readonly ILog log = LogManager.GetLogger(typeof(RemoveTrees));
        private static readonly bool isDebugEnabled = log.IsDebugEnabled;

        //---------------------------------------------------------------------

        string IChange.Type
        {
            get { return TypeName; }
        }

        //---------------------------------------------------------------------

        public RemoveTrees(ICohortCutter        cohortCutter,
                           Planting.SpeciesList speciesToPlant)
        {
            this.cohortCutter = cohortCutter;
            this.speciesToPlant = speciesToPlant;
        }

        //---------------------------------------------------------------------

        public void ApplyTo(ActiveSite site)
        {
            if (isDebugEnabled)
                log.DebugFormat("    Applying LCC {0} to site {1}",
                                GetType().Name,
                                site.Location);

            // For now, we don't do anything with the counts of cohorts cut.
            CohortCounts cohortCounts = new CohortCounts();
            cohortCutter.Cut(site, cohortCounts);
            if (speciesToPlant != null)
                Reproduction.ScheduleForPlanting(speciesToPlant, site);
        }
    }
}
