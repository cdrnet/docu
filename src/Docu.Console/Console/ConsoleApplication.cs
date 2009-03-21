using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Docu.Events;
using Docu.Parsing;
using StructureMap;

namespace Docu.Console
{
    public class ConsoleApplication
    {
        private readonly List<string> arguments = new List<string>();
        private readonly IDocumentationGenerator documentationGenerator;
        private readonly IEventAggregator eventAggregator;
        private readonly IScreenWriter screenWriter;
        private bool canRun;

        public ConsoleApplication(IScreenWriter screenWriter, IDocumentationGenerator documentationGenerator, IEventAggregator eventAggregator)
        {
            this.screenWriter = screenWriter;
            this.documentationGenerator = documentationGenerator;
            this.eventAggregator = eventAggregator;

            this.eventAggregator
                .GetEvent<WarningEvent>()
                .Subscribe(Warning);
            this.eventAggregator
                .GetEvent<BadFileEvent>()
                .Subscribe(BadFile);
        }

        void Warning(string message)
        {
            ShowMessage(new WarningMessage(message));
        }

        void BadFile(string path)
        {
            ShowMessage(new BadFileMessage(path));
        }

        public static void Run(IEnumerable<string> args)
        {
            ContainerBootstrapper.BootstrapStructureMap();

            var application = ObjectFactory.GetInstance<ConsoleApplication>();

            application.SetArguments(args);
            application.Run();
        }

        public void SetArguments(IEnumerable<string> args)
        {
            arguments.AddRange(args);

            if (arguments.Count >= 0)
                canRun = true;
        }

        public void Run()
        {
            if (!canRun)
            {
                ShowMessage(Messages.Help);
                return;
            }

            ShowMessage(Messages.Splash);

            string[] assemblies = GetAssembliesFromArgs(arguments);
            string[] xmls = GetXmlsFromArgs(arguments, assemblies);

            if (VerifyArguments(assemblies, xmls))
            {
                ShowMessage(Messages.Start);

                documentationGenerator.SetAssemblies(assemblies);
                documentationGenerator.SetXmlFiles(xmls);
                documentationGenerator.SetTemplatePath(Path.Combine(Path.GetDirectoryName(typeof(ConsoleApplication).Assembly.Location), "templates"));
                documentationGenerator.Generate();

                ShowMessage(Messages.Done);
            }
        }

        private bool VerifyArguments(IEnumerable<string> assemblies, IEnumerable<string> xmls)
        {
            foreach (string argument in arguments)
            {
                if (argument.EndsWith(".dll") || argument.EndsWith(".xml")) continue;

                ShowMessage(new InvalidArgumentMessage(argument));
                return false;
            }

            if (!VerifyAssemblies(assemblies)) return false;
            if (!VerifyXmls(xmls)) return false;

            return true;
        }

        private bool VerifyAssemblies(IEnumerable<string> assemblies)
        {
            if (assemblies.Count() == 0)
            {
                ShowMessage(Messages.NoAssembliesSpecified);
                return false;
            }

            foreach (string assembly in assemblies)
            {
                if (!File.Exists(assembly))
                {
                    ShowMessage(new AssemblyNotFoundMessage(assembly));
                    return false;
                }
            }

            return true;
        }

        private bool VerifyXmls(IEnumerable<string> xmls)
        {
            if (xmls.Count() == 0)
            {
                ShowMessage(Messages.NoXmlsFound);
                return false;
            }

            foreach (string xml in xmls)
            {
                if (!File.Exists(xml))
                {
                    ShowMessage(new XmlNotFoundMessage(xml));
                    return false;
                }
            }

            return true;
        }

        private void ShowMessage(IScreenMessage message)
        {
            screenWriter.WriteMessage(message);
        }

        private static string[] GetXmlsFromArgs(IEnumerable<string> args, IEnumerable<string> assemblies)
        {
            var xmls = new List<string>();

            foreach (string arg in args)
            {
                if (arg.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                    xmls.Add(arg);
            }

            if (xmls.Count == 0)
            {
                // none specified, try to find some
                foreach (string assembly in assemblies)
                {
                    string name = assembly.Replace(".dll", ".xml");

                    if (File.Exists(name))
                        xmls.Add(name);
                }
            }

            return xmls.ToArray();
        }

        private static string[] GetAssembliesFromArgs(IEnumerable<string> args)
        {
            var assemblies = new List<string>();

            foreach (string arg in args)
            {
                if (arg.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                    assemblies.Add(arg);
            }

            return assemblies.ToArray();
        }
    }
}