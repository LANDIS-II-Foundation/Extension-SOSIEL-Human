using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Entities
{
    using Helpers;

    public class AgentPrototype
    {
        public string NamePrefix { get; set; }

        public Dictionary<string, dynamic> CommonVariables { get; set; }

        public List<Goal> Goals { get; set; }

        public Dictionary<string, RuleSetSettings> SetSettings { get; set; }

        public List<Rule> Rules { get; set; }


        private List<RuleSet> mentalProto;
        /// <summary>
        /// Rule sets transformed from rule list 
        /// </summary>
        public List<RuleSet> MentalProto
        {
            get { return mentalProto == null ? TransformRulesToRuleSets() : mentalProto; }
        }


        public bool UseDoNothing;

        public AgentPrototype()
        {
            CommonVariables = new Dictionary<string, dynamic>();
            SetSettings = new Dictionary<string, RuleSetSettings>();
            Rules = new List<Rule>();
        }



        /// <summary>
        /// Transformation from rule list to rule sets
        /// </summary>
        /// <returns></returns>
        private List<RuleSet> TransformRulesToRuleSets()
        {
            mentalProto = Rules.GroupBy(r => r.RuleSet).OrderBy(g => g.Key).Select(g =>
                   new RuleSet(g.Key, Goals.Where(goal => SetSettings[g.Key.ToString()].AssociatedWith.Contains(goal.Name)).ToArray(),
                       g.GroupBy(r => r.RuleLayer).OrderBy(g2 => g2.Key).Select(g2 => new RuleLayer(SetSettings[g.Key.ToString()].Layer[g2.Key.ToString()], g2)), SetSettings[g.Key.ToString()].IsSequential)).ToList();

            return mentalProto;
        }



        /// <summary>
        /// Add rule to rule scope of current prototype
        /// </summary>
        /// <param name="newRule"></param>
        /// <param name="layer"></param>
        public void AddNewRule(Rule newRule, RuleLayer layer)
        {
            if (mentalProto == null)
                TransformRulesToRuleSets();

            layer.Add(newRule);

            Rules.Add(newRule);
        }



        /// <summary>
        /// Add do nothing rule to each rule set and rule layer
        /// </summary>
        /// <returns>Returns array of rule ids</returns>
        public IEnumerable<string> AddDoNothingRules()
        {
            List<string> temp = new List<string>();

            //todo maybe need add to modifiable layers only 
            MentalProto.ForEach(set =>
            {
                set.Layers.ForEach(layer =>
                {
                    if (!layer.Rules.Any(r => r.IsAction == false))
                    {
                        Rule proto = layer.Rules.First();

                        Rule doNothing = new Rule
                        {
                            Antecedent = new RuleAntecedentPart[] { new RuleAntecedentPart { Param = VariablesUsedInCode.IsActive, Sign = "==", Value = true } },
                            Consequent = new RuleConsequent
                            {
                                Param = proto.Consequent.Param,
                                Value = Activator.CreateInstance(proto.Consequent.Value),
                                CopyToCommon = proto.Consequent.CopyToCommon,
                                SavePrevious = proto.Consequent.SavePrevious
                            },
                            RequiredParticipants = 1,
                            IsAction = false
                        };

                        AddNewRule(doNothing, layer);

                        temp.Add(doNothing.Id);
                    }
                });
            });

            return temp;
        }

    }
}
