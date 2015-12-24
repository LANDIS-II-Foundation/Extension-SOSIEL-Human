using System;

namespace MultiAgent
{
    public class StrategyModifier<T>
    {
        private readonly Func<T, T> _conditionalCooperationAction;
        private readonly Func<T, T> _freeRiderAction;
        private readonly Func<T, T> _trendFollowerAction;

        public StrategyModifier(Func<T, T> conditionalCooperationAction, Func<T, T> trendFollowerAction,
            Func<T, T> freeRiderAction)
        {
            _conditionalCooperationAction = conditionalCooperationAction;
            _trendFollowerAction = trendFollowerAction;
            _freeRiderAction = freeRiderAction;
        }

        public Strategy<T> Execute(AgentType agentType, T ifCondition, T elseCondition)
        {
            var strategy = new Strategy<T> {IfCondition = ifCondition};

            switch (agentType)
            {
                case AgentType.ConditionalCooperator:
                    if (_conditionalCooperationAction != null)
                    {
                        strategy.ThenCondition = _conditionalCooperationAction(ifCondition);
                    }
                    break;
                case AgentType.TrendFollower:
                    if (_trendFollowerAction != null)
                    {
                        strategy.ThenCondition = _trendFollowerAction(ifCondition);
                    }
                    break;
                case AgentType.FreeRider:
                    if (_freeRiderAction != null)
                    {
                        strategy.ThenCondition = _freeRiderAction(ifCondition);
                    }
                    break;
                default:
                    strategy.ThenCondition = elseCondition;
                    break;
            }
            return strategy;
        }
    }
}