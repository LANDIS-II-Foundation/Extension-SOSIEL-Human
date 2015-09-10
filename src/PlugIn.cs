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
            // LCB additions: 9/9/2015

            // Examine Biophysical Environment 

            // Update Mental Models

            // Determine Actions: we need very explicit rules about which cohort species, age, percentage to remove.
               // We are acting at the stand level; Base Harvest concepts of ManagementArea not applicable - LCB
               // Percentage to remove is percentage of biomass - LCB
               // Do you want to PreventEstablishment or Plant on a site? - LCB.  Yes to both - GS/RS

            // Take Actions
                // - cohorts are thinned or removed
                // - partial removal (thinning) means using biomass-harvest-lib PartialCohortCutter - LCB
                // - therefore need list of cohorts to thin or remove
                // - if provided a list, we can reuse the base-harvest API
                    // These methods: RemoveTrees.cs: RemoveTrees()
                    // Also ParameterParser.cs: Lines 101-102: CohortCutterFactory.
                    // Not seeing where the biomass is actually removed with this approach; These methods just store the objects used. (LCB)
                // - alternatively, we can simply thin or remove cohorts directly.
                    // Could access Biomass Cohort Library directly:
                    // SiteCohorts.cs: ReduceOrKillBiomassCohorts(IDisturbance disturbance) and RemoveMarkedCohorts()
                // - using harvest management libs - LCB
                    // Prescription.cs: Harvest(Stand stand) - In harvest-mgmt-lib; At stand level but shows use of api
                    // PartialCohortCutter.cs Cut(ActiveSite site, CohortCounts cohortCounts) - in biomass-harvest-lib
                    // WholeCohortCutter.cs Cut(ActiveSite site, CohortCounts cohortCounts) - in site-harvest-lib; called by PartialCohortCutter
                // - either way, we need very explicit rules about which cohort species, age, percentage to remove.

            // Log actions
                // - What logging or output maps do you need? Likely can't use BaseHarvest as it aggregates higher than site - LCB
                // What was harvested or planted?
                // How did the actors change?  What are actor properites?
                // Stand maps of activity?  Maps of biomass removed?


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
