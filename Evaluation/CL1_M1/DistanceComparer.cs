using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using Common.Entities;

namespace CL1_M1
{
    
    class DistanceComparer : IComparer<Site>
    {
        readonly Site _current;

        public DistanceComparer(Site current)
        {
            _current = current;
        }

        public int Compare(Site x, Site y)
        {
            int dist1 = Math.Max(Math.Abs(_current.HorizontalPosition - x.HorizontalPosition), Math.Abs(_current.VerticalPosition - x.VerticalPosition));

            int dist2 = Math.Max(Math.Abs(_current.HorizontalPosition - y.HorizontalPosition), Math.Abs(_current.VerticalPosition - y.VerticalPosition));

            if (dist1 == dist2)
                return 0;
            else
                return dist1 > dist2 ? 1 : -1;
        }
    }
}
