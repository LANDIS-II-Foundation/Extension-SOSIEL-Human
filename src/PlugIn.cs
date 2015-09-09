// This file is part of the Social Human extension for LANDIS-II.

using Landis.Core;
using Landis.Library.Succession;
using Landis.SpatialModeling;
using log4net;
using System.Collections.Generic;

namespace Landis.Extension.SocialHuman
{
    public class PlugIn
        : Landis.Core.ExtensionMain
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PlugIn));
        private static readonly bool isDebugEnabled = log.IsDebugEnabled;

        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:social-human");
        public static readonly string ExtensionName = "Land Use";

        private Parameters parameters;
        //private string inputMapTemplate;

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

            // NEED to read in Stand maps (general methods can be borrowed from Base Harvest library)
            // NEED to read in actors and their initial stand assignment(s).
            // Question: What are the initial properties of actors?

            //inputMapTemplate = parameters.InputMaps;
            //if (parameters.SiteLogPath != null)
            //    SiteLog.Initialize(parameters.SiteLogPath);

            // Load initial land uses from input map for timestep 0
            //ProcessInputMap(
            //    delegate(Site site,
            //             LandUse initialLandUse)
            //    {
            //        SiteVars.LandUse[site] = initialLandUse;
            //        return initialLandUse.Name;
            //    });
        }

        //---------------------------------------------------------------------

        public override void Run()
        {
            // RScheller Notes and Thoughts: 8/29/2015

            // Examine Biophysical Environment 

            // Update Mental Models

            // Determine Actions: we need very explicit rules about which cohort species, age, percentage to remove.

            // Take Actions
                // - cohorts are thinned or removed
                // - therefore need list of cohorts to thin or remove
                // - if provided a list, we can reuse the base-harvest API
                    // These methods: RemoveTrees.cs: RemoveTrees()
                    // Also ParameterParser.cs: Lines 101-102: CohortCutterFactory.
                // - alternatively, we can simply thin or remove cohorts directly.
                    // Could access Biomass Cohort Library directly:
                    // SiteCohorts.cs: ReduceOrKillBiomassCohorts(IDisturbance disturbance) and RemoveMarkedCohorts()
                // - either way, we need very explicit rules about which cohort species, age, percentage to remove.


            //if (SiteLog.Enabled)
            //    SiteLog.TimestepSetUp();

            //ProcessInputMap(
            //    delegate(Site site,
            //             LandUse newLandUse)
            //    {
                    //LandUse currentLandUse = SiteVars.LandUse[site];
                    //if (newLandUse != currentLandUse)
                    //{
                    //    //SiteVars.LandUse[site] = newLandUse;
                    //    //string transition = string.Format("{0} --> {1}", currentLandUse.Name, newLandUse.Name);
                    //    //if (!currentLandUse.AllowEstablishment && newLandUse.AllowEstablishment)
                    //    //{
                    //    //    //string message = string.Format("Error: The land-use change ({0}) at pixel {1} requires re-enabling establishment, but that's not currently supported",
                    //    //    //                               transition,
                    //    //    //                               site.Location);
                    //    //    //throw new System.ApplicationException(message);
                    //    //}
                    //    //else if (currentLandUse.AllowEstablishment && !newLandUse.AllowEstablishment)
                    //    //    Reproduction.PreventEstablishment((ActiveSite) site);

                    //    //if (isDebugEnabled)
                    //    //    log.DebugFormat("    LU at {0}: {1}", site.Location, transition);
                    //    newLandUse.LandCoverChange.ApplyTo((ActiveSite)site);
                    //    if (SiteLog.Enabled)
                    //        SiteLog.WriteTotalsFor((ActiveSite)site);
                    //    return transition;
                    //}
                    //else
                    //    return null;
            //    });

            //if (SiteLog.Enabled)
            //    SiteLog.TimestepTearDown();
        }

        //---------------------------------------------------------------------

        // A delegate for processing a land use read from an input map.
        public delegate string ProcessLandUseAt(Site site, LandUse landUse);

        //---------------------------------------------------------------------

        //public void ProcessInputMap(ProcessLandUseAt processLandUseAt)
        //{
        //    string inputMapPath = MapNames.ReplaceTemplateVars(inputMapTemplate, Model.Core.CurrentTime);
        //    Model.Core.UI.WriteLine("  Reading map \"{0}\"...", inputMapPath);
        //    IInputRaster<MapPixel> inputMap;
        //    Dictionary<string, int> counts = new Dictionary<string, int>();
        //    using (inputMap = Model.Core.OpenRaster<MapPixel>(inputMapPath))
        //    {
        //        MapPixel pixel = inputMap.BufferPixel;
        //        foreach (Site site in Model.Core.Landscape.AllSites)
        //        {
        //            inputMap.ReadBufferPixel();
        //            if (site.IsActive)
        //            {
        //                LandUse landUse = LandUseRegistry.LookUp(pixel.LandUseCode.Value);
        //                if (landUse == null)
        //                {
        //                    string message = string.Format("Error: Unknown map code ({0}) at pixel {1}",
        //                                                   pixel.LandUseCode.Value,
        //                                                   site.Location);
        //                    throw new System.ApplicationException(message);
        //                }
        //                string key = processLandUseAt(site, landUse);
        //                if (key != null)
        //                {
        //                    int count;
        //                    if (counts.TryGetValue(key, out count))
        //                        count = count + 1;
        //                    else
        //                        count = 1;
        //                    counts[key] = count;
        //                }
        //            }
        //        }
        //    }
        //    foreach (string key in counts.Keys)
        //        Model.Core.UI.WriteLine("    {0} ({1:#,##0})", key, counts[key]);
        //}

        //---------------------------------------------------------------------

        //public new void CleanUp()
        //{
        //    if (SiteLog.Enabled)
        //        SiteLog.Close();
        //}
    }
}
