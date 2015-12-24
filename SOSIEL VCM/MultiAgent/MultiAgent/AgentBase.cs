using System;

namespace MultiAgent
{
    public abstract class AgentBase<T>
    {
        public string Name { get; set; }

        public AgentType AgentType { get; set; }

        public T Endowment { get; set; }

        public Strategy<T> Strategy { get; set; }

        public T LastPayoff { get; set; }

        public Guid Identifier { get; set; }

        public abstract void Update(T calculateNewPayoff, T averageContribution);

        public abstract bool IsContributionValid();
    }
}