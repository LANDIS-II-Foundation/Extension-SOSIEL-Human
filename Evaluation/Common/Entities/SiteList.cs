using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Enums;

    public class SiteList
    {
        public List<Site> Sites { get; private set; }

        private SiteList() { }

        private static int CalculateMatrixSize(int agentsCount, double vacantProportion)
        {
            return (int)Math.Round(Math.Sqrt(agentsCount / (1 - vacantProportion)));
        }

        private static Site CreateSite(int horizontalPosition, int verticalPosition, int startIndex, int size)
        {
            Site cell = new Site
            {
                HorizontalPosition = horizontalPosition,
                VerticalPosition = verticalPosition
            };

            if (horizontalPosition == verticalPosition && (horizontalPosition == startIndex || horizontalPosition == size))
            {
                cell.Type = SiteType.Corner;
                cell.GroupSize = 3;

                return cell;
            }

            if (horizontalPosition == startIndex || horizontalPosition == size
                || verticalPosition == startIndex || verticalPosition == size)
            {
                cell.Type = SiteType.Edge;
                cell.GroupSize = 5;

                return cell;
            }

            cell.Type = SiteType.Center;
            cell.GroupSize = 8;

            return cell;
        }

        public static SiteList Generate(int agentCount, double vacantProportion)
        {
            SiteList siteList = new SiteList();

            int size = CalculateMatrixSize(agentCount, vacantProportion);
            int startIndex = 1;

            siteList.Sites = new List<Site>(size ^ 2);

            for (int i = 1; i <= size; i++)
                for (int j = 1; j <= size; j++)
                {
                    siteList.Sites.Add(CreateSite(i, j, startIndex, size));
                }

            return siteList;
        }
    }
}
