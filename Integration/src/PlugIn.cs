// This file is part of SHE for LANDIS-II.

using Landis.Core;
using Landis.Library.BiomassCohorts;
using Landis.Library.Succession;
using Landis.SpatialModeling;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Landis.Extension.SOSIELHuman
{
    using Configuration;
    using Algorithm;
    using System;

    public class PlugIn
        : Landis.Core.ExtensionMain
    {

        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:SOSIEL Human");
        public static readonly string ExtensionName = "SOSIEL Human";

        private Parameters parameters;


        private ConfigurationModel configuration;


        private IAlgorithm luhyLite;


        private static ICore modelCore;


        private Dictionary<ActiveSite, double> projectedBiomass;

        private int iteration = 1;


        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }
        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile,
                                            ICore mCore)
        {
            modelCore = mCore;

            ModelCore.UI.WriteLine("  Loading parameters from {0}", dataFile);

            //Parse Landis parameters here
            ParameterParser parser = new ParameterParser(ModelCore.Species);
            parameters = Landis.Data.Load<Parameters>(dataFile, parser);



        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {
            ModelCore.UI.WriteLine("Initializing {0}...", Name);
            SiteVars.Initialize();

            Timestep = parameters.Timestep;

            // Read in (input) Agent Configuration Json File here:
            ModelCore.UI.WriteLine("  Loading agent parameters from {0}", parameters.InputJson);
            configuration = ConfigurationParser.ParseConfiguration(parameters.InputJson);


            //create algorithm instance
            int iterations = 1; // Later we can decide if there should be multiple SHE sub-iterations per LANDIS-II iteration. 
            //create dictionary 
            projectedBiomass = ModelCore.Landscape.ToDictionary(activeSite => activeSite, activeSite => 0d);

            luhyLite = new LuhyLiteImplementation(iterations, configuration, ModelCore.Landscape, projectedBiomass);

            luhyLite.Initialize();


            //remove old output files
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());

            foreach (System.IO.FileInfo fi in di.GetFiles("SOSIELHuman_*.csv"))
            {
                fi.Delete();
            }
        }

        public override void Run()
        {
#if DEBUG
            Debugger.Launch();
#endif

            //convert cohort biomass to a format understandable by the extension
            foreach (ActiveSite activeSite in projectedBiomass.Keys.ToArray())
            {
                projectedBiomass[activeSite] = ComputeLivingBiomass(SiteVars.Cohorts[activeSite]);
            }


            //run SOSIEL algorithm
            luhyLite.RunIteration();

            ModelCore.UI.WriteLine("Reducing site biomass...");

            //projectedBiomass contains updated biomass values. Logic for updating SiteVars here:
            foreach (KeyValuePair<ActiveSite, double> kvp in projectedBiomass)
            {
                UpdateBiomass(kvp.Key, kvp.Value);
            }

            iteration++;
        }

        //private static void ReduceCohortBiomass(ActiveSite site, double biomassReduction)
        //{
        //    // This is a placeholder, will be cohort-by-cohort in final implementation.
        //    // SiteVars.Biomass[site] = (int) (biomassReduction * SiteVars.Biomass[site]);
        //}

        //---------------------------------------------------------------------

        private int ComputeLivingBiomass(ISiteCohorts cohorts)
        {
            int total = 0;
            if (cohorts != null)
                foreach (ISpeciesCohorts speciesCohorts in cohorts)
                    foreach (ICohort cohort in speciesCohorts)
                        total += (int)(cohort.Biomass);
            //total += ComputeBiomass(speciesCohorts);
            return total;
        }

        private void UpdateBiomass(ActiveSite site, double updatedBiomass)
        {
            //update logic here:
            double initialBiomass = ComputeLivingBiomass(SiteVars.Cohorts[site]);

            double reductionPercent = Math.Round(1d - updatedBiomass / initialBiomass, 3); // Change number of fractional digits if needed

            //PartialDisturbance.ReduceCohortBiomass(site, percentBiomassReduction);  // RMS I'm unsure how this method should be called.
            //It should be called like this
            PartialDisturbance.ReduceCohortBiomass(site, reductionPercent);
        }
    }
}
