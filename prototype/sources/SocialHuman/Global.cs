using System;

namespace SocialHuman
{
    using Models;

    public class Global
    {
        private static Global instance;

        //public double HistoricalTotalBiomassMin { get; private set; }
        public int MaxHeuristicInLayer { get; private set; }
        public int PowerOfDistribution { get; private set; }

        public static Global Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                throw new NullReferenceException("Instance wasn't initialized");
            }
        }

        private Global(GlobalParameters parameters)
        {
            //HistoricalTotalBiomassMin = parameters.HistoricalTotalBiomassMin;
            MaxHeuristicInLayer = parameters.MaxHeuristicInLayer;
            PowerOfDistribution = parameters.PowerOfDistribution;
        }

        public static void Init(GlobalParameters parameters)
        {
            if(instance == null)
            {
                instance = new Global(parameters);
                return;
            }

            throw new InvalidOperationException("Instance was initialized");
        }
    }
}
