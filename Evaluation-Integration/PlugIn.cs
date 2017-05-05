// This file is part of the Social Human extension for LANDIS-II.

using Landis.Core;
using Landis.Library.Succession;
using Landis.SpatialModeling;
using log4net;
using System.Collections.Generic;

namespace SocialHuman
{
    public class PlugIn
        : Landis.Core.ExtensionMain
    {

        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:social-human");
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
            Landis.Library.BiomassHarvest.Main.InitializeLib(Model.Core);
            Model.Core.UI.WriteLine("  Loading parameters from {0}", dataFile);
            ParameterParser parser = new ParameterParser(Model.Core.Species);
            parameters = Landis.Data.Load<Parameters>(dataFile, parser);
        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {
            Model.Core.UI.WriteLine("Initializing {0}...", Name);
            SiteVars.Initialize(Model.Core);
            Timestep = parameters.Timestep;

            //Read in JsonFile here:
            // ReadInputFile(parameters.InputJson)
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
                    int siteBiomass = SiteVars.Biomass[site];

                    // SHE sub-routines here
                    // double biomassReduction = algorithm.Run(siteBiomass);

                    // This method uses the biomass reduction calculated from SHE sub-routines to reduce the biomass of every cohort by a percentage.
                    // ReduceCohortBiomass(biomassReduction);

                }

            }

        }

    }
}
