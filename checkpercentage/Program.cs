using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace checkpercentage
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var cmd = new RootCommand
            {
                new Command("check", "Checks the percentage of unit tests")
                {
                    new Option<string>(new[] { "--directory", "-d"  }, "Directory where the Summary.xml file is located")
                    {
                        Argument = new Argument<string>
                        {
                            Arity = ArgumentArity.ExactlyOne
                        }
                    },
                    new Option<string>(new[] { "--percentage", "-p" }, "Percentage value to check on your unit tests")
                    {
                        Argument = new Argument<string>
                        {
                            Arity = ArgumentArity.ExactlyOne
                        }
                    },
                }.WithHandler(nameof(PercentageUnitTest)),
            };

            return await cmd.InvokeAsync(args);
        }

        private static int PercentageUnitTest(string directory, string percentage, IConsole console)
        {
            if (string.IsNullOrEmpty(directory) ||
                    string.IsNullOrEmpty(percentage))
            {
                Console.WriteLine("Usage: check [template] [options]");
                Console.WriteLine("\n");
                Console.WriteLine("Options:");
                Console.WriteLine("-d, --directory <DirectorySummary>");
                Console.WriteLine("-p, --percentage <Percentage>");

                return 0;
            }

            var fileInfo = new FileInfo(directory);

            if (!fileInfo.Exists)
            {
                console.Error.WriteLine($"File Summary.xml not found in directory {directory}");
                return 1;
            }

            var file = new XmlDocument();
            file.Load(directory);

            var nodes = file.SelectNodes("CoverageReport/Summary/Linecoverage");
            var percentageReplace = nodes[0].InnerText.Replace('.', ',');
            var percentageActual = double.Parse(percentageReplace);

            var percentageCheckReplace = percentage.Replace('.', ',');
            var percentageCheck = double.Parse(percentageCheckReplace);

            if (percentageActual >= percentageCheck)
            {
                console.Out.WriteLine("========== Check Percentage Success =========");
                console.Out.WriteLine($"Congratulations, unit tests reached {nodes[0].InnerText} % compared to your code");
            }
            else
            {
                console.Error.WriteLine("========== Check Percentage Error =========");
                console.Error.WriteLine($"Unfortunately unit tests did not reach more than {percentage} % code coverage! It's at {nodes[0].InnerText} %");
                
                return 1;
            }

            return 0;
        }

        private static Command WithHandler(this Command command, string methodName)
        {
            var method = typeof(Program).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            var handler = CommandHandler.Create(method!);
            command.Handler = handler;
            return command;
        }
    }
}
