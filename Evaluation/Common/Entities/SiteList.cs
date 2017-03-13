using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Enums;

    public class SiteList
    {
        public Site[][] Sites { get; private set; }

        private SiteList() { }

        private static int CalculateMatrixSize(int agentsCount, double vacantProportion)
        {
            return (int)Math.Round(Math.Sqrt(agentsCount / (1 - vacantProportion)));
        }

        private static Site CreateSite(int horizontalPosition, int verticalPosition, int startIndex, int size)
        {
            Site site = new Site
            {
                HorizontalPosition = horizontalPosition,
                VerticalPosition = verticalPosition
            };

            if (horizontalPosition == verticalPosition && (horizontalPosition == startIndex || horizontalPosition == size))
            {
                site.Type = SiteType.Corner;
                site.GroupSize = 3;

                return site;
            }

            if (horizontalPosition == startIndex || horizontalPosition == size
                || verticalPosition == startIndex || verticalPosition == size)
            {
                site.Type = SiteType.Edge;
                site.GroupSize = 5;

                return site;
            }

            site.Type = SiteType.Center;
            site.GroupSize = 8;

            return site;
        }

        public IEnumerable<Site> AsSiteEnumerable()
        {
            return Sites.SelectMany(s => s);
        }

        public IEnumerable<Site> AdjacentSites(Site centerSite, bool includeCenter = false)
        {
            List<Site> temp = new List<Site>(centerSite.GroupSize);

            for (int i = centerSite.VerticalPosition > 0 ? centerSite.VerticalPosition - 1 : 0; i <= centerSite.VerticalPosition + 1 && i < Sites.Length; i++)
                for (int j = centerSite.HorizontalPosition > 0 ? centerSite.HorizontalPosition - 1 : 0; j <= centerSite.HorizontalPosition + 1 && j < Sites.Length; j++)
                {
                    Site site = Sites[i][j];

                    if (site.Equals(centerSite) == false || includeCenter)
                        temp.Add(site);
                }

            return temp;
        }

        public static SiteList Generate(int agentCount, double vacantProportion)
        {
            SiteList siteList = new SiteList();

            int size = CalculateMatrixSize(agentCount, vacantProportion);
            int startIndex = 0;

            siteList.Sites = new Site[size][];

            for (int i = startIndex; i < size; i++)
            {
                siteList.Sites[i] = new Site[size];

                for (int j = startIndex; j < size; j++)
                {
                    siteList.Sites[i][j] = CreateSite(j, i, startIndex, size - 1);
                }
            }
            return siteList;
        }


    }
}
