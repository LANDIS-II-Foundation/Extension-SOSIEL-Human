// This file is part of the Social Human extension for LANDIS-II.

using Landis.Core;
using Landis.Library.Succession;
using Landis.SpatialModeling;
//using log4net;
using System.Collections.Generic;

namespace SocialHuman
{
    public class PlugIn
        : Landis.Core.ExtensionMain
    {

        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:sosiel-human");
        public static readonly string ExtensionName = "Land Use";

        private Parameters parameters;

        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
        }

        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile,
                                            ICore modelCore)
        {
            Model.Core = modelCore;
            Model.Core.UI.WriteLine("  Loading parameters from {0}", dataFile);
            ParameterParser parser = new ParameterParser(Model.Core.Species);
            parameters = Landis.Data.Load<Parameters>(dataFile, parser);
        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {
            Model.Core.UI.WriteLine("Initializing {0}...", Name);
            SiteVars.Initialize();
            
            Timestep = parameters.Timestep;

            //Read in Agent Configuration Json File here:
            // ReadInputFile(parameters.InputJson)

            // Other SHE initializations also here.
        }

        //---------------------------------------------------------------------

        public override void Run()
        {
            int iterations = 1; // Later we can decide if there should be multiple iterations per year.
            for (int i = 0; i < iterations; i++)
            {
                // Step through every site on the landscape
                foreach (ActiveSite site in Model.Core.Landscape)
                {
                    int siteBiomass = SiteVars.Biomass[site];  // total biomass on the site
                    double biomassReduction = 1.0;  // default = no action taken; varies from 0.0 - 1.0.

                    // SHE sub-routines here
                    // biomassReduction = algorithm.Run(siteBiomass);
                    // Etc.

                    // This method uses the biomass reduction calculated from SHE sub-routines to reduce the biomass of every cohort by a percentage.
                    ReduceCohortBiomass(site, biomassReduction);

                }

            }

        }

        private void ReduceCohortBiomass(ActiveSite site, double biomassReduction)
        {
            // This is a placeholder, will be cohort-by-cohort in final implementation.
            SiteVars.Biomass[site] = (int) (biomassReduction * SiteVars.Biomass[site]);
        }

    }
}
