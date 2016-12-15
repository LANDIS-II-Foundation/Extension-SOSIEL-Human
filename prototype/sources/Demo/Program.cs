using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AutoMapper;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

using SocialHuman;
using SocialHuman.Enums;
using SocialHuman.Entities;
using SocialHuman.Models;

namespace Demo
{
    using Models.Input;
    using Models.Output;
    using Parsers;
    

    class Program
    {
        static string inputFilePath = Path.Combine(Environment.CurrentDirectory, "input.json");
        static string outputDirectory = Path.Combine(Environment.CurrentDirectory, "OutputTemplate");
        static string outputFilePath = Path.Combine(outputDirectory, "output.json");

        static Algorithm algorithm;

        static IISAgent iis;


        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            //iis killing
            if(iis != null)
            {
                iis.Dispose();
            }

            return true;
        }
        #endregion

        static void Main(string[] args)
        {
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            Program program = new Program();

            program.Start();
        }

        
        void Start()
        {
            ConfigureMapper();

            try
            {
                Initialize();

                Run();

                using (iis = new IISAgent())
                {
                    Random rand = new Random();
                    int port = rand.Next(30000, 40000);


                    iis.Start($"/path:\"{outputDirectory}\" /port:{port} /clr:v2.0");

                    Process.Start($"http://localhost:{port}");

                    WaitKeyPress();
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine("\n\nERROR! Input is incorrect : " + ex.Message);
                WaitKeyPress();
            }
        }

        void ConfigureMapper()
        {
            Console.WriteLine("Configurating....");


            Mapper.Initialize((config) =>
            {
                config.CreateMap<GlobalInput, GlobalParameters>();
                config.CreateMap<HeuristicInput, HeuristicParameters>();
                config.CreateMap<PeriodInitialStateInput, PeriodInitialStateParameters>();
                config.CreateMap<GoalStateInput, GoalStateParameters>();
                config.CreateMap<HeuristicConsequentInput, HeuristicConsequentRule>()
                .ForMember(dest => dest.ConsequentRelationship, opt => opt.ResolveUsing((a) => a.ConsequentRelationshipConverter));
                config.CreateMap<ActorInput, ActorParameters>()
                .ForMember(dest => dest.ActorType, opt => opt.ResolveUsing((a) => (ActorType)a.ActorType));
                config.CreateMap<ActorGoalInput, ActorGoal>();
            });
        }

        void Initialize()
        {
            //todo: clear
            Console.WriteLine("Initializing....");

            IParser parser = new JsonParser(inputFilePath);

            GlobalInput configuration = parser.ParseGlogalConfiguration();
            ActorInput[] actors = parser.ParseActors();
            Dictionary<string, PeriodInitialStateInput> periodState = parser.ParseInitialState();

            algorithm = Algorithm.Initialize(
                Mapper.Map<GlobalParameters>(configuration),
                Mapper.Map<ActorParameters[]>(actors),
                Mapper.Map<Dictionary<string, PeriodInitialStateParameters>>(periodState));
        }

        void Run()
        {
            Console.WriteLine("Algorithm is running....");

            LinkedList<Period> results = algorithm.Run();

            Output output = new Output();

            output.Periods = results.Select(p => new PeriodOutput
            {
                PeriodNumber = p.PeriodNumber,
                Biomass = p.Sites.Select(s => Math.Round(s.BiomassValue, 2)).ToArray(),
                Actors = p.SiteStates.Select(g => new ActorOutput
                {
                    Name = g.Key.ActorName,
                    Information = g.Value.OrderBy(s => s.Site.Id).Select(s => new SiteOutput
                    {
                        Name = $"site{s.Site.Id}",
                        ActivatedHeuristics = s.Activated.GroupBy(h => h.Layer.Set).Select(hg => new SetOutput
                        {
                            SetName = $"set{hg.Key.PositionNumber}",
                            ActivatedHeuristics = hg.OrderBy(h => h.Layer.PositionNumber).Select(h => new LayerOutput
                            {
                                AncetedentConst = h.AncetedentConst,
                                AncetedentSign = h.AntecedentInequalitySign,
                                ConsequentValue = h.ConsequentValue,
                                HeuristicName = h.Id
                            }).ToArray(),
                            Harvested = s.GetTakeActionForSet(hg.Key).HarvestAmount
                        }).ToArray()
                    }).ToArray()
                }).ToArray()
            }).ToArray();

            output.MentalModels = algorithm.Actors.Select(a => new
            {
                name = a.ActorName,
                mental = a.MentalModel.Select(hs => new
                {
                    set = $"Set{hs.PositionNumber}",
                    layers = hs.Layers.Select(hl => new
                    {
                        layer = $"Layer{hl.PositionNumber}",
                        heuristics = hl.Heuristics.Select(h => new
                        {
                            name = h.Id,
                            sign = h.AntecedentInequalitySign,
                            ancetedent = h.AncetedentConst,
                            consequent = h.ConsequentValue,
                            isaction = h.IsAction
                        }).ToArray()
                    }).ToArray()
                }).ToArray()
            }).ToArray();

            string outputString = JsonConvert.SerializeObject(output);

            File.WriteAllText(outputFilePath, outputString);
        }

        void WaitKeyPress()
        {
            Console.WriteLine("{0}Press any key to continue...", Environment.NewLine);
            Console.ReadKey();
        }
    }
}
