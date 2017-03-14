using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    using Environments;

    public interface ICloneableAgent<T> : IAgent, ICloneable<T> where T : class
    {

    }
}
