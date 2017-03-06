using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Algorithm
{
    public interface IAlgorithm
    {
        string Name { get; }

        void Run();
    }
}
