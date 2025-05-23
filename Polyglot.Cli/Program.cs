using NLog;
using Polyglot.Core;
using Polyglot.Core.Utils;
using Polyglot.Couch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConsoleView
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var polyIo = new PolyIO();
            polyIo.CreateDirectoryStructure();
            var settings = new PolySettings(polyIo.ConfigFilePath);

            var processor = new LocalizationProcessor
            {
                Advance = settings.Advance,
                CardCategoryFilter = settings.CardCategoryFilter,
                Backend = new CouchDbBackend(settings),
                Exporter = new XmlExporter
                {
                    XmlReadable = settings.XmlReadable,
                    SuppressCdata = settings.SuppressCdata
                },
                Importer = new XmlImporter()
            };

            string locale = GetParameterValue(args, "locale");
            string fileName = GetParameterValue(args, "filename");

            if (string.IsNullOrWhiteSpace(locale) || string.IsNullOrWhiteSpace(fileName))
            {
                logger.Error("Invalid command line. \"locale\" and \"fileName\" are required parameters.");
                return;
            }

            switch (args[0])
            {
                case "fetch":
                    bool fresh = GetParameterValue(args, "--disable-fresh") == null;
                    bool outdate = GetParameterValue(args, "--disable-outdated") == null;
                    bool untranslated = GetParameterValue(args, "--disable-untranslated") == null;

                    Console.WriteLine("Please wait...");
                    try
                    {
                        processor.FetchAndSave(fileName, locale, fresh, outdate, untranslated);
                    }
                    catch (WebException ex)
                    {
                        logger.Error("Failed to get data from backend. Check user authorization credentials.", ex);
                        return;
                    }

                    logger.Info("Fetch operation completed successfull.");
                    break;
                case "submit":
                    string resolveConflict = GetParameterValue(args, "--resolve-conflict") ?? "use-mine";                    

                    Console.WriteLine("Please wait...");

                    try
                    {
                        var differences = processor.LoadAndCompare(fileName, locale);
                        foreach (var conflict in differences.Diff.Where(x => x.Resolution == ConflictResolution.Manual))
                            conflict.Resolution =
                                resolveConflict == "use-mine" ? ConflictResolution.UseTranslatedFrontend : ConflictResolution.UseTranslatedBackend;

                        processor.Submit(differences);
                    }
                    catch (WebException ex)
                    {
                        logger.Error("Failed to submit data to backend. Check user authorization credentials.", ex);
                        return;
                    }
                    catch (FileNotFoundException ex)
                    {
                        logger.Error(string.Format("File \"{0}\" not found.", fileName), ex);
                        return;
                    }
                    catch (XmlException ex)
                    {
                        logger.Error(string.Format("File \"{0}\" contains invalid text in line \"{1}\", position \"{2}\"",
                             fileName, ex.LineNumber, ex.LinePosition));
                        return;
                    }

                    logger.Info("Submit operation completed successfull.");
                    break;
                default:
                    logger.Error("Invalid command line. Operation must be either \"fetch\" or \"submit\".");
                    return;
            }
        }

        private static string GetParameterValue(string[] args, string paramName)
        {            
            var param = args.Select(x => x.Split('=')).SingleOrDefault(x => x[0] == paramName);
            if (param != null)
                return param.Length == 1 ? string.Empty : param[1];
            return null; 
        }
    }
}
