using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHuman.Algorithm
{
    using Configuration;
    using Entities;
    

    public class LuhyLiteImplementation : SosielAlgorithm, IAlgorithm
    {
        public string Name { get { return "LuhyLiteImplementation"; } }

        public LuhyLiteImplementation(int numberOfIterations, ProcessesConfiguration processConfiguration, ILandscape landscape, ISiteVar<Site> site) 
            : base(numberOfIterations, processConfiguration)
        {

        }

        protected override void InitializeAgents()
        {
            throw new NotImplementedException();
        }

        protected override Dictionary<IAgent, AgentState> InitializeFirstIterationState()
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Initialization of algorithm
        /// </summary>
        public void Initialize()
        {
            InitializeAgents();

            InitializeFirstIterationState();

            AfterInitialization();
        }



        /// <summary>
        /// Runs as many iterations as passed to the constructor
        /// </summary>
        public void RunIteration()
        {

        }
    }
}
