// This file is part of the Social Human extension for LANDIS-II.

using Landis.SpatialModeling;

namespace Landis.Extension.SocialHuman.LandCover
{
    /// <summary>
    /// Interface for all types of land cover change
    /// </summary>
    public interface IChange
    {
        string Type { get; }

        /// <summary>
        /// Apply the change to the land cover at an individual site.
        /// </summary>
        void ApplyTo(ActiveSite site);
    }
}
