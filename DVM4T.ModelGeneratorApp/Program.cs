using DVM4T.ModelGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NDesk.Options;

namespace DVM4T.ModelGeneratorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //args are:
            //Directory for output
            //Info for connecting to Tridion
            //Tridion folder for schemas
            //Namespace for models
            //Config XML Path
            //Optional code provider, CSharp or VB

            string saveDir = null;
            string tridionUsername = null;
            string tridionPassword = null;
            string coreServicesAddress = null;
            string schemaFolderTcmUri = null;
            string ns = null;
            string xmlPath = null;
            string codeProvider = "CSharp";
            bool showHelp = false;
            var p = new OptionSet() {
            { "d|dir=", "required - the directory to save the output in.",
              v => saveDir = v  },
            { "u|username=", "required - the Tridion Core Services username",
              v => tridionUsername = v },
            { "p|password=", "required - the Tridion Core Services password",
              v => tridionPassword = v },
            { "a|address=",  "required - the Tridion Core Services full URL", 
              v => coreServicesAddress = v },
            { "f|folderUri=", "required - the full TCM URI of the folder with the Schemas",
              v => schemaFolderTcmUri = v },
            { "n|namespace=", "required - the namespace for the generated View Models",
              v => ns = v },
            { "c|config=", "required - path the XML Code Gen config file",
              v => xmlPath = v },
            { "r|provider=", "optional - the code provider. Either CSharp or VB",
              v => codeProvider = v},
            { "h|help", "show help.",
              v => { showHelp = v != null; } }
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("An error occurred parsing the parameters: ");
                Console.WriteLine(e.Message);
                ShowHelp(p);
                return;
            }

            if (showHelp)
            {
                ShowHelp(p);
                return;
            }
            if (String.IsNullOrEmpty(saveDir) || String.IsNullOrEmpty(tridionUsername) || String.IsNullOrEmpty(tridionPassword)
               || String.IsNullOrEmpty(coreServicesAddress) || String.IsNullOrEmpty(schemaFolderTcmUri)
               || String.IsNullOrEmpty(ns) || String.IsNullOrEmpty(xmlPath))
            {
                Console.WriteLine("Missing a required input.");
                ShowHelp(p);
                return;
            }

            var models = new CoreServiceModelBuilder(coreServicesAddress, tridionUsername, tridionPassword).CoreCreateModels(schemaFolderTcmUri);
            var config = new XmlModelConfig(xmlPath);
            var generator = new CodeGenerator(config, ns, codeProvider);
            var saveIn = new DirectoryInfo(saveDir);
            foreach (var model in models)
            {
                generator.GenerateModelCode(model, saveIn);
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("\nUsage: DVM4T.ModelGeneratorApp [OPTIONS]+");
            Console.WriteLine("Generates DVM4T View models using schemas directly from Tridion via the Core Services.");
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
