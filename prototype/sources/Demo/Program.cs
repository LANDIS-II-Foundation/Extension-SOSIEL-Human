using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AutoMapper;
using System.Diagnostics;
using Newtonsoft.Json;

using SocialHuman;
using SocialHuman.Enums;
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
        static string iisPort = "8877";

        static Algorithm algorithm;

        static void Main(string[] args)
        {
            ConfigureMapper();

            try
            {
                Initialize();

                Run();
            }
            catch (JsonException ex)
            {
                Console.WriteLine("\n\nERROR! Incorrect input: "+ex.Message);
                WaitKeyPress();
            }


        }

        static void ConfigureMapper()
        {
            //todo: clear
            Console.WriteLine("Configurating....");


            Mapper.Initialize((config) =>
            {
                config.CreateMap<GlobalInput, GlobalParameters>();
                config.CreateMap<HeuristicInput, HeuristicParameters>();
                config.CreateMap<HeuristicConsequentInput, HeuristicConsequentRule>()
                .ForMember(dest => dest.ConsequentRelationship, opt => opt.ResolveUsing((a) => a.ConsequentRelationshipConverter));
                config.CreateMap<ActorInput, ActorParameters>()
                .ForMember(dest => dest.ActorType, opt => opt.ResolveUsing((a) => (ActorType)a.ActorType))
                .ForMember(dest => dest.Heuristics, opt => opt.ResolveUsing((a) => Mapper.Map<HeuristicParameters[]>(a.HeuristicSet)));
            });
        }

        static void Initialize()
        {
            //todo: clear
            Console.WriteLine("Initializing....");

            IParser parser = new JsonParser(inputFilePath);

            GlobalInput configuration = parser.ParseGlogalConfiguration();
            ActorInput[] actors = parser.ParseActors();

            algorithm = Algorithm.Initialize(Mapper.Map<GlobalParameters>(configuration), Mapper.Map<ActorParameters[]>(actors));
        }

        static void Run()
        {
            Console.WriteLine("Algorithm is running....");

            LinkedList<PeriodModel> results = algorithm.Run();

            PeriodOutput[] data = results.Select(p => new PeriodOutput
            {
                PeriodNumber = p.PeriodNumber,
                Biomass = p.Sites.Select(s => Math.Round(s.GoalValue, 2)).ToArray(),
                Actors = p.PartialData.GroupBy(pd => pd.Actor).Select(g => new ActorOutput
                {
                    Name = g.Key.Name,
                    Information = g.OrderBy(s => s.Site.Id).Select(s => new SiteOutput
                    {
                        Name = $"site{s.Site.Id}",
                        ActivatedHeuristics = s.Activated.GroupBy(h => h.Layer.Set).Select(hg => new SetOutput
                        {
                            SetName = $"set{hg.Key.PositionNumber}",
                            Values = hg.OrderBy(h => h.Layer.PositionNumber).Select(h => Math.Round(h.ConsequentValue, 2)).ToArray()
                        }).ToArray()
                    }).ToArray()
                }).ToArray()
            }).ToArray();

            var result = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            File.WriteAllText(outputFilePath, result);

            IISAgent iis = new IISAgent();
            iis.Start($"/path:{outputDirectory} /port:{iisPort}");

            Process.Start($"http://localhost:{iisPort}");
            WaitKeyPress();

            iis.Stop();
        }

        private static void WaitKeyPress()
        {
            Console.WriteLine("{0}Press any key to continue...", Environment.NewLine);
            Console.ReadKey();
        }
    }
}
