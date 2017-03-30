using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Processes
{
    public abstract class VolatileProcess
    {
        protected abstract void AboveMin();
        protected abstract void BelowMax();
        protected abstract void Maximize();


        protected void SpecificLogic(string tendency)
        {
            switch(tendency)
            {
                case "AboveMin":
                    AboveMin();
                    break;
                case "BelowMax":
                    BelowMax();
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
