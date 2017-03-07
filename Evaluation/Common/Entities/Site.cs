using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Enums;

    public class Site
    {
        public SiteType Type { get; set; }

        public int HorizontalPosition { get; set; }

        public int VerticalPosition { get; set; }

        public int GroupSize { get; set; }


        public Agent OccupiedBy { get; set; }

        public bool IsOccupied
        {
            get
            {
                return OccupiedBy == null ? true : false;
            }
        }
    }
}
