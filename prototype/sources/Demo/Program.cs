using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

using SocialHuman;
using SocialHuman.Models;

namespace Demo
{
    using Models.Output;

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
            if (iis != null)
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
            catch (Exception ex)
            {
                Console.WriteLine("\n\nERROR! " + ex.Message);
                WaitKeyPress();
            }
        }

        void ConfigureMapper()
        {
            Console.WriteLine("Configurating....");



        }

        void Initialize()
        {
            //todo: clear
            Console.WriteLine("Initializing....");



            algorithm = Algorithm.Initialize(inputFilePath);
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
                    Information = g.Value.OrderBy(s => s.Site?.Id ?? 0).Select(s => new SiteOutput
                    {
                        Name = s.Site != null ? $"site{s.Site.Id}" : "-",
                        ActivatedHeuristics = s.Activated.GroupBy(h => h.Layer.Set).Select(hg => new SetOutput
                        {
                            SetName = $"set{hg.Key.PositionNumber}",
                            ActivatedHeuristics = hg.OrderBy(h => h.Layer.PositionNumber).Select(h => new LayerOutput
                            {
                                LayerName = $"layer{h.Layer.PositionNumber}",
                                Heuristics = new HeuristicOutput[] { new HeuristicOutput
                                    {
                                        HeuristicName = h.Id,
                                        Antecedents = h.Antecedent.Select(ap => new AntecedentOutput
                                        {
                                            AntecedentConst = ap.Const,
                                            AntecedentParam = ap.Param,
                                            AntecedentSign = ap.Sign
                                        }).ToArray(),
                                        ConsequentParam = h.Consequent.Param,
                                        ConsequentValue = h.Consequent.Value,
                                        IsAction = h.IsAction,
                                        isCollective = h.IsCollectiveAction
                                    }
                                }
                            }).ToArray(),
                            TakeActions = s.TakeActions.Select(ta => new TakeActionOutput
                            {
                                Param = ta.VariableName,
                                Value = ta.Value
                            }).ToArray()
                        }).ToArray()
                    }).ToArray()
                }).ToArray()
            }).ToArray();

            output.MentalModels = algorithm.Actors.Select(a => new
            {
                name = a.ActorName,
                mental = a.AssignedHeuristics.GroupBy(h => h.Layer.Set).Select(hs => new SetOutput
                {
                    SetName = $"set{hs.Key.PositionNumber}",
                    Layers = hs.GroupBy(h => h.Layer).Select(hl => new LayerOutput
                    {
                        LayerName = $"Layer{hl.Key.PositionNumber}",
                        Heuristics = hl.Select(h => new HeuristicOutput
                        {
                            HeuristicName = h.Id,
                            Antecedents = h.Antecedent.Select(ap => new AntecedentOutput
                            {
                                AntecedentConst = ap.Const,
                                AntecedentParam = ap.Param,
                                AntecedentSign = ap.Sign
                            }).ToArray(),
                            ConsequentParam = h.Consequent.Param,
                            ConsequentValue = h.Consequent.Value,
                            IsAction = h.IsAction,
                            isCollective = h.IsCollectiveAction
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
