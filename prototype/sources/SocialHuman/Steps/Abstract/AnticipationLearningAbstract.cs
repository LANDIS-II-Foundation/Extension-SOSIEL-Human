using System.Collections.Generic;
using System.Linq;

namespace SocialHuman.Steps.Abstract
{
    using Entities;
    using Actors;
    using Models;

    abstract class AnticipationLearningAbstract
    {
        //difference between prior value of goal variable and goal value(t)
        protected double diffPriorAndMin;
        //difference between current value of goal variable and goal min(t)
        protected double diffCurrAndMin;

        protected PeriodModel currentPeriod;

        protected abstract void SpecificLogic();

        public void Execute(Actor actor, LinkedListNode<PeriodModel> periodModel)
        {
            currentPeriod = periodModel.Value;

            PeriodModel priorPeriod = periodModel.Previous.Value;

            //1.Calculate anticipated influence(t) == goal variable value(t) – goal variable value(t-1)
            currentPeriod.AnticipatedInfluence = currentPeriod.TotalBiomass - priorPeriod.TotalBiomass;
            //2.Update the anticipated influence of heuristics implemented in prior period
            IEnumerable<Heuristic> implementedInPriorPeriod = priorPeriod.GetDataForActor(actor).SelectMany(pd=>pd.Activated);

            foreach(Heuristic h in implementedInPriorPeriod)
            {
                h.AnticipatedInfluence = currentPeriod.AnticipatedInfluence;
            }

            //= goal variable value(t-1) – critical goal variable value(t)
            diffPriorAndMin = priorPeriod.TotalBiomass - Global.Instance.HistoricalTotalBiomassMin;
            //= goal variable value(t) – critical goal variable value(t)
            diffCurrAndMin = currentPeriod.TotalBiomass - Global.Instance.HistoricalTotalBiomassMin;

            currentPeriod.DiffCurrAndMin = diffCurrAndMin;

            SpecificLogic();
        }
    }
}
