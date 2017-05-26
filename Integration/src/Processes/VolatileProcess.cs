using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Processes
{
    public abstract class VolatileProcess
    {
        protected abstract void EqualToOrAboveFocalValue();
        protected abstract void BelowFocalValue();
        protected abstract void Maximize();


        protected void SpecificLogic(string tendency)
        {
            switch(tendency)
            {
                case "EqualToOrAboveFocalValue":
                    EqualToOrAboveFocalValue();
                    break;
                case "BelowFocalValue":
                    BelowFocalValue();
                    break;
                case "Maximize":
                    Maximize();
                    break;
                default:
                    throw new Exception("Unknown managing of goal");
            }
        }
    }
}
