﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.SOSIELHuman.Environments
{
    public interface ICloneable<T>
    {
        T Clone();
    }
}
