// This file is part of the Social Human extension for LANDIS-II.

using Landis.SpatialModeling;

namespace Landis.Extension.SocialHuman.LandCover
{
    /// <summary>
    /// For land uses that do not trigger any change in the land cover.
    /// </summary>
    public class NoChange
        : IChange
    {
        public const string TypeName = "NoChange";

        public string Type
        {
            get { return TypeName; }
        }

        public void ApplyTo(ActiveSite site)
        {
            // Do nothing to the site.
        }
    }
}
