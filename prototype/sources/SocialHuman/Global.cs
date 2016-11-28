using System;

namespace SocialHuman
{
    using Models;

    public class Global
    {
        private static Global instance;

        public double HistoricalTotalBiomassMin { get; private set; }
        public int MaxHeuristicInLayer { get; private set; }

        private Global(GlobalParameters parameters)
        {
            HistoricalTotalBiomassMin = parameters.HistoricalTotalBiomassMin;
            MaxHeuristicInLayer = parameters.MaxHeuristicInLayer;
        }

        public static void Init(GlobalParameters parameters)
        {
            if(instance == null)
            {
                instance = new Global(parameters);
                return;
            }

            throw new InvalidOperationException("Instance has initialized");
        }

        public static Global Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                throw new NullReferenceException("Instance didn't initialized");
            }
        } 
    }
}
