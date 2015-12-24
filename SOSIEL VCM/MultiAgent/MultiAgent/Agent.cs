using System;

namespace MultiAgent
{
    public class Agent<T> : AgentBase<T> where T:IComparable<T>
    {
        public override void Update(T calculateNewPayoff, T averageContribution)
        {
            LastPayoff = calculateNewPayoff;
        }

        public override bool IsContributionValid()
        {
            return true;
        }
    }
}