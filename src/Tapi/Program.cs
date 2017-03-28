using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace tapi
{
    public class Program
    {
        public static int Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        public static async Task<int> MainAsync(string[] args)
        {
            var mode = CommandLineParser.Parse(args);
            
            switch (mode)
            {
                case CommandLineParser.ExecuteReports er:
                    Logger.LogLevel = er.LogLevel;
                    PrintWelcome();
                    try
                    {
                        await ExecuteReports(er);
                    }
                    catch(Exception ex)
                    {
                        Logger.LogError(ex);
                        return -1;
                    }
                    break;
                case CommandLineParser.Help lm:
                    PrintHelp();
                    break;
                case CommandLineParser.Error error:
                    PrintErrorMessage(error.Message);
                    break;
                default:
                    break;
            }
            return 0;
        }

        public static async Task ExecuteReports(CommandLineParser.ExecuteReports args)
        {
            var startTime = DateTime.Now;
           
            Logger.Log("Loading metadata");
            var model = Model.Load(args.ModelFile);
            
            Logger.Log("Load audience data");
            if (!Directory.Exists(args.DataDir))
            {
                ZipFile.ExtractToDirectory(args.DataDir + ".zip", "./");
            }
            var loadData = new LoadData
            {
                Model = model,
                Dir = args.DataDir,
                StartDate = model.From,
                EndDate = model.To
            };
            await loadData.ExecuteAsync();

            if(!Directory.Exists(args.OutputDir))
            {
                Directory.CreateDirectory(args.OutputDir);
            }
            foreach(var r in model.Reports)
            {
                Logger.Log($"Calculate report: {r.Name}");
                var report = new CalculateReport();
                var res = report.Execute(r,loadData.Events,args.TemplateDir);

                // Save the report
                File.WriteAllBytes(Path.Combine(args.OutputDir,r.FileName),res.file);
            } 

            Logger.Log($"Done! timespent: {DateTime.Now - startTime}");
        }
        
        public static void PrintErrorMessage(string message)
        {
            Console.WriteLine(message);
        }
        public static void PrintWelcome()
        {
            Logger.Log(@"
________________ __________.___ 
\__    ___/  _  \\______   \   |
  |    | /  /_\  \|     ___/   |
  |    |/    |    \    |   |   |
  |____|\____|__  /____|   |___|
                \/                          
Starting Tapi, parse arguments",false, true);
        }
        public static void PrintHelp()
        {
            Logger.Log(@"

tapi -conf:file.xml -data:datadir -templates:templatedir -out:outdir

-conf        Path to the configuration file
-data        Dir with source data
-templates   Dir with templates
-out         Dir to write output to
            ");
        }
    }
}