using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazorConnect4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("./Data");
            }
            BigBrainBootCamp();
            //CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void BigBrainBootCamp()
        {
            Console.WriteLine("Let's get working!");
            AIModels.QAgent red;
            AIModels.QAgent yellow;

            if (!File.Exists("Data/RedQ1.bin"))
            {
                red = new AIModels.QAgent(Model.CellColor.Red);
                red.ToFile("Data/RedQ1.bin");
            }
            else
            {
                red = AIModels.QAgent.ConstructFromFile("Data/RedQ1.bin");
            }
            if (!File.Exists("Data/YellowQ1.bin"))
            {
                yellow = new AIModels.QAgent(Model.CellColor.Yellow);
                yellow.ToFile("Data/YellowQ1.bin");
            }
            else
            {
                yellow = AIModels.QAgent.ConstructFromFile("Data/YellowQ1.bin");
            }
            red.brainTrainingCamp(yellow, iterations: 1000);
            
            red.ToFile("Data/RedQ1");
        }
    }
}
