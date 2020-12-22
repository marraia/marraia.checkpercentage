using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
using System.IO;
using System.Xml;

namespace checkpercentage
{
    class Program
    {
        private static InvocationContext invocationContext;
        private static ConsoleRenderer consoleRenderer;

        static void Main(InvocationContext invocationContext, string[] args)
        {
            Program.invocationContext = invocationContext;
            consoleRenderer = new ConsoleRenderer(
              invocationContext.Console,
              mode: invocationContext.BindingContext.OutputMode(),
              resetAfterRender: true
            );

            var cmd = new RootCommand();
            cmd.AddCommand(CheckPercentage());

            cmd.InvokeAsync(args).Wait();
        }

        private static Command CheckPercentage()
        {
            var command = new Command("check", "Checks the percentage of unit tests");
            command.AddOption(new Option(new[] { "--directory", "-d" }, "Directory where the Summary.xml file is located")
            {
                Argument = new Argument<string>
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            });

            command.AddOption(new Option(new[] { "--percentage", "-p" }, "Percentage value to check on your unit tests")
            {
                Argument = new Argument<string>
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            });


            command.Handler = CommandHandler.Create<string, string>((directory, percentage) => {
                if (string.IsNullOrEmpty(directory) ||
                     string.IsNullOrEmpty(percentage))
                {
                    Console.WriteLine("Usage: check [template] [options]");
                    Console.WriteLine("\n");
                    Console.WriteLine("Options:");
                    Console.WriteLine("-d, --directory <DirectorySummary>");
                    Console.WriteLine("-p, --percentage <Percentage>");
                }
                else
                {
                    PercentageUnitTest(directory, percentage);
                }
            });

            return command;
        }

        private static void PercentageUnitTest(string path, string percentage)
        {
            var fileInfo = new FileInfo(path);

            if (!fileInfo.Exists)
            {
                throw new ArgumentException($"File Summary.xml not found in directory {path}");
            }

            var file = new XmlDocument();
            file.Load(path);

            var nodes = file.SelectNodes("CoverageReport/Summary/Linecoverage");
            var percentageReplace = nodes[0].InnerText.Replace('.', ',');
            var percentageActual = double.Parse(percentageReplace);

            var percentageCheckReplace = percentage.Replace('.', ',');
            var percentageCheck = double.Parse(percentageCheckReplace);

            if (percentageActual >= percentageCheck)
            {
                Console.WriteLine("========== Check Percentage Success =========");
                Console.WriteLine($"Congratulations, unit tests reached {nodes[0].InnerText} % compared to your code");
            }
            else
            {
                Console.Error.WriteLine("========== Check Percentage Error =========");
                Console.Error.WriteLine($"Unfortunately unit tests did not reach more than {percentage} % code coverage! It's at {nodes[0].InnerText} %");
            }
        }
    }
}
