// Copyright 2005 University of Wisconsin

using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

// Although this utility class was designed for output path templates, it
// can also be used for input path templates.  Should rename it in the library
// eventually.
using FilePath = Edu.Wisc.Forest.Flel.Util.OutputPath;

namespace SocialHuman
{
    /// <summary>
    /// Methods for working with the template for the names of input maps.
    /// </summary>
    public static class MapNames
    {
        public const string TimestepVar = "timestep";

        private static IDictionary<string, bool> knownVars;
        private static IDictionary<string, string> varValues;

        //---------------------------------------------------------------------

        static MapNames()
        {
            knownVars = new Dictionary<string, bool>();
            knownVars[TimestepVar] = true;  // true --> required

            varValues = new Dictionary<string, string>();
        }

        //---------------------------------------------------------------------

        public static void CheckTemplateVars(string template)
        {
            FilePath.CheckTemplateVars(template, knownVars);
        }

        //---------------------------------------------------------------------

        public static string ReplaceTemplateVars(string template,
                                                 int    timestep)
        {
            varValues[TimestepVar] = timestep.ToString();
            return FilePath.ReplaceTemplateVars(template, varValues);
        }
    }
}
