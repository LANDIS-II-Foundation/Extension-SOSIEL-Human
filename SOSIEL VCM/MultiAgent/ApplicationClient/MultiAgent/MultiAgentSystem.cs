//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Xml;
//using Calculator;
//using MultiAgentLibrary;
//using MultiAgentLibrary.Agent;
//using MultiAgentLibrary.CommunicationMediator;
//using MultiAgentLibrary.Strategy;

//namespace ApplicationClient.MultiAgent
//{
//    public class MultiAgentSystem
//    {
//        public List<Agent<double>> _agents = new List<Agent<double>>();
//        public Calculator<double> _calculator = new Calculator<double>();
//        public List<Strategy<double>> _strategies = new List<Strategy<double>>();
//        public CommunicationMediator<double> _communicationMediator = new CommunicationMediator<double>();

//        public void InitializeService(int iterations, string configurationFile, Func<List<CustomData<double>>, CustomData<double>> processData)
//        {
//            //InitializeStrategies(configurationFile);
//            //InitializeAgents(agentsNumber, configurationFile);
//            //InitializeCommunicationMap();
//            InitializeCalculatorService(iterations, processData);
//        }

//        private void InitializeCalculatorService(int iterations, Func<List<CustomData<double>>, CustomData<double>> processData)
//        {
//            _calculator.Iterations = iterations;
//            _calculator.ProcessData = processData;

//            foreach (var agent in _agents)
//            {
//                _calculator.AddAgent(agent);
//            }
//        }

//        public void InitializeCommunicationMap(int[][] matrix)
//        {
//            Dictionary<Guid, List<Agent<double>>> res = new Dictionary<Guid, List<Agent<double>>>();
//            //todo: build communication map here
//            for (int i = 0; i < _agents.Count; i++)
//            {
//                for (int j = 0; j < matrix.GetLength(0); j++)
//                {
//                    List<Agent<double>> agentsToAdd = new List<Agent<double>>();
//                    for (int k = 0; k < matrix[j].GetLength(0); k++)
//                    {
//                        if(matrix[j][k]==1 && k!=i)
//                            agentsToAdd.Add(_agents[i]);
//                    }
//                    if(agentsToAdd.Any())
//                        res.Add( Guid.NewGuid(), agentsToAdd);
//                }
//            }
//            if(res.Any())
//                _communicationMediator.Register(res);
//        }

//        public void InitializeAgents(List<Agent<double>> agents)
//        {
//            _agents = agents;
//        }

//        private void InitializeAgents(int agentsNumber, string configurationFile)
//        {
//            var document = new XmlDocument();

//            var fs = new FileStream(configurationFile, FileMode.Open, FileAccess.Read, FileShare.Read);

//            using (var sr = new StreamReader(fs))
//            {
//                document.Load(sr);

//                if (!string.IsNullOrEmpty(document.InnerXml))
//                {
//                    var rootElement = document.GetElementsByTagName("agents");
//                    for (var i = 0; i < rootElement[0].ChildNodes.Count; i++)
//                    {
//                        var agent = new Agent<double>
//                        {
//                            Resource =
//                                new CustomData<double>
//                                {
//                                    Data = Convert.ToInt32(rootElement[0].ChildNodes[i].Attributes["resource"].Value)
//                                },
//                            Name = rootElement[0].ChildNodes[i].Attributes["name"].Value
//                        };

//                        foreach (XmlNode node in rootElement[0].ChildNodes[i].ChildNodes)
//                        {
//                            foreach (XmlNode childNode in node.ChildNodes)
//                            {
//                               agent.Strategies.Add(_strategies.FirstOrDefault(x=>x.ID == Convert.ToInt32(childNode.Attributes["id"].Value)));
//                            }
//                        }
                        
//                        _agents.Add(agent);
//                    }
//                }
//            }
//        }

//        public void InitializeStrategies(List<Strategy<double>> strategies)
//        {
//            _strategies = strategies;
//        }

//        private void InitializeStrategies(string configurationFile)
//        {
//            var document = new XmlDocument();

//            var fs = new FileStream(configurationFile, FileMode.Open, FileAccess.Read, FileShare.Read);

//            using (var sr = new StreamReader(fs))
//            {
//                document.Load(sr);

//                if (!string.IsNullOrEmpty(document.InnerXml))
//                {
//                    var rootElement = document.GetElementsByTagName("strategies");
//                    for (var i = 0; i < rootElement[0].ChildNodes.Count; i++)
//                    {
//                        var strategy = new Strategy<double>
//                        {
//                            ID = Convert.ToInt32(rootElement[0].ChildNodes[i].Attributes["id"].Value),
//                            IfCondition = 
//                                new CustomData<double>
//                                {
//                                    Data = Convert.ToDouble(rootElement[0].ChildNodes[i].Attributes["if"].Value)
//                                },
//                            ThenCondition = 
//                                new CustomData<double>
//                                {
//                                    Data = Convert.ToDouble(rootElement[0].ChildNodes[i].Attributes["else"].Value)
//                                }
//                        };

//                        _strategies.Add(strategy);
//                    }
//                }
//            }
//        }

//        public void RunService(Action<string> updatingStatus)
//        {
//            for (int i = 0; i < _calculator.Iterations; i++)
//            {
//                updatingStatus("Making contribution...");
//                // do something like contribution
//                _calculator.CollectData();

//                updatingStatus("Contribution data:");
//                foreach (var data in _calculator.DataToProceed)
//                {
//                    updatingStatus(string.Format("{0}",data.Data));
//                }

//                _calculator.Proceed();
//            }
//        }
//    }
//}