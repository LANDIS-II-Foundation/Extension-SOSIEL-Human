using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Landis.Extension.Sosiel.Entities
{
    using Environments;

    public interface ICloneableAgent<T> : IAgent, ICloneable<T>
    {

    }
}
