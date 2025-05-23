using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using SWF = System.Windows.Forms;
using NLog;
using Polyglot.Core;
using Polyglot.Core.Utils;
using Polyglot.Couch;
using Json;
using SJson;

namespace Polyglot
{
    public partial class ManagementWindow : Window
    {
        public string locale { get; set; }

        public LocalizationProcessor Processor { get; set; }

        public LogWindow LogWindow { get; set; }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public ManagementWindow()
        {
            InitializeComponent();
        }

        private async void FetchAndSave(object sender, RoutedEventArgs e)
        {
            if (!((App)Application.Current).CheckAuthenticationFileOnDefaultValues())
                return;

            var settings = new PolySettings(new PolyIO().ConfigFilePath);
            var options = new OptionsWindow(locale, settings.CardCategoryFilter, settings.DocumentNameFilter);
            options.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (!(options.ShowDialog() ?? false))
                return;

            locale = options.Local.Trim().ToLower();
            var fetchValid = options.chkFetchValid.IsChecked ?? false;
            var fetchInvalid = options.chkFetchInvalid.IsChecked ?? false;
            var fetchUntranslated = options.chkFetchUntranslated.IsChecked ?? false;
            var traverseConfig = new TraverseConfig(locale, fetchValid, fetchInvalid, fetchUntranslated);

            var splitNewLineSeparators = new[] { Environment.NewLine, "\r", "\n" };

            #region Get traverse filter
            if (options.chkIsValueRegularExtension.IsChecked ?? false)
            {
                var filterValue = options.txtbxDocumentFilter.Text.Split(splitNewLineSeparators, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrEmpty(filterValue))
                {
                    RegExFilter regExFilter = null;
                    try
                    {
                        regExFilter = new RegExFilter(filterValue.TrimEnd(Environment.NewLine).TrimEnd('\r', '\n').Trim());
                    }
                    catch (ArgumentException ex)
                    {
                        LoadingIndicator.StopStoryboard();
                        logger.Error("Error", ex);
                        MessageBox.Show($"Invalid regular expression '{filterValue}'. Please fix it and try again", "Operation has failed", MessageBoxButton.OK);
                        return;
                    }
                    
                    traverseConfig.AddFilter(TraverseFilterKind.Document, regExFilter);
                }
            }
            else
            {
                var filterDocumentItems = options.txtbxDocumentFilter.Text.Split(splitNewLineSeparators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.TrimEnd(Environment.NewLine).TrimEnd('\r', '\n').Trim()).ToList();
                if (filterDocumentItems.Any())
                    traverseConfig.AddFilter(TraverseFilterKind.Document, new ListContainsFilter(filterDocumentItems));
                settings.DocumentNameFilter = filterDocumentItems;
            }

            var filterCategoryItems = options.txtbxCategoryFilter.Text.Split(splitNewLineSeparators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.TrimEnd(Environment.NewLine).TrimEnd('\r', '\n').Trim()).ToList();
            if (filterCategoryItems.Any())
                traverseConfig.AddFilter(TraverseFilterKind.Category, new ListContainsFilter(filterCategoryItems));
            settings.CardCategoryFilter = filterCategoryItems;
            settings.WriteConfigFile();
            #endregion

            SWF.SaveFileDialog dlgSave = new SWF.SaveFileDialog();
            dlgSave.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.RestoreDirectory = true;

            if ((dlgSave.ShowDialog() != SWF.DialogResult.OK) || dlgSave.FileName.Length < 1)
                return;

            LoadingIndicator.BeginStoryboard();

            try
            {
                await Task.Run(() =>
                {
                    Processor.FetchAndSave(dlgSave.FileName, traverseConfig);
                    logger.Info("Fetch operation completed successfully");
                });
            }
            catch (WebException ex)
            {
                LoadingIndicator.StopStoryboard();
                logger.Error("Error", ex);
                ProposalUpdateCredentials(ex.Message);
                return;
            }

            LoadingIndicator.StopStoryboard();

            if (LogWindow != null && LogWindow.LogMessageCount > 0)
                LogWindow.Show();

            MessageBox.Show("Fetch operation completed successfully", "Success", MessageBoxButton.OK);
        }

        private async void LoadAndSubmit(object sender, RoutedEventArgs e)
        {
            if (!((App)Application.Current).CheckAuthenticationFileOnDefaultValues())
                return;

            var options = new OptionsWindow(locale);
            options.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (!(options.ShowDialog() ?? false))
                return;

            locale = options.Local.Trim().ToLower();

            SWF.OpenFileDialog dlgOpen = new SWF.OpenFileDialog();
            dlgOpen.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            dlgOpen.FilterIndex = 1;
            dlgOpen.RestoreDirectory = true;

            if ((dlgOpen.ShowDialog() != SWF.DialogResult.OK) || dlgOpen.FileName.Length < 1)
                return;   

            LoadingIndicator.BeginStoryboard();
            Differences differences = null;
            try
            {
                differences = await Task.Run<Differences>(() =>
                {
                    return Processor.LoadAndCompare(dlgOpen.FileName, locale);
                });
            }
            catch (WebException ex)
            {
                logger.Error("Error", ex);
                ProposalUpdateCredentials(ex.Message);
                return;
            }
            catch (FileNotFoundException ex)
            {
                logger.Error("Error", ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                return;
            }
            catch (XmlException ex)
            {
                logger.Error("Error", ex);
                var errorMessage = string.Format("File \"{0}\" contains invalid text in line \"{1}\", position \"{2}\"",
                    dlgOpen.FileName, ex.LineNumber, ex.LinePosition);
                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK);
                return;
            }

            DifferencesResolve(differences.Diff);
            await Task.Run(() => { Processor.Submit(differences); });
            logger.Info("Submit operation completed successfully");
            LoadingIndicator.StopStoryboard();

            if (LogWindow != null && LogWindow.LogMessageCount > 0)
                LogWindow.Show();

            MessageBox.Show("Submit operation completed successfully", "Success", MessageBoxButton.OK);
        }

        private async void ExportToSJSON(object sender, RoutedEventArgs e)
        {
            if (!((App)Application.Current).CheckAuthenticationFileOnDefaultValues())
                return;

            SWF.SaveFileDialog dlgSave = new SWF.SaveFileDialog();
            dlgSave.Filter = "Localized strings files (*.strings)|*.strings|SJSON files (*.sjson)|*.sjson|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.RestoreDirectory = true;

            if ((dlgSave.ShowDialog() != SWF.DialogResult.OK) || dlgSave.FileName.Length < 1)
                return;

            var exportToFilename = dlgSave.FileName;
            LoadingIndicator.BeginStoryboard();

            try
            {
                await Task.Run(() =>
                {
                    // Read CouchDB server configuration and fetch all documents
                    var polyIo = new PolyIO();
                    var settings = new PolySettings(polyIo.ConfigFilePath);
                    var backend = new CouchDbBackend(settings);
                    var backendDocuments = backend.FetchDocuments();

                    // Traverse backend documents and grab all localizable strings
                    var traverser = new JsonSchemaTraverser();
                    var docs = traverser.TraverseAll(backendDocuments);

                    // Try get locales from Locale enum
                    var locales = traverser.GetLocales(backendDocuments);

                    // If no locales found then get all possible locales from localizable strings
                    if (locales.Count == 0 && docs.Count() > 0)
                    {
                        locales = docs.SelectMany(doc => doc.Strings.SelectMany(s => s.Data.AsObject["data"].AsObject.Keys)).Distinct().ToList();
                    }

                    // Omit document name if there is no more than one document available
                    var stripDocumentFromPath = docs.Count() <= 1;
                    var obj = new JsonObject();

                    Func<ImmutableJson, ImmutableJson> fnConvert = item => {
                        var result = new JsonObject();
                        var data = item.AsObject["data"].AsObject;
                        result["en"] = ImmutableJson.Create(item.AsObject["text"].AsString);

                        locales.ForEach(locale => {
                            var localeData = data.ContainsKey(locale) ? data[locale].AsObject : null;

                            result[locale] = (localeData != null && localeData.ContainsKey("translation"))
                                ? localeData["translation"].AsString
                                : string.Empty;
                        });

                        return ImmutableJson.Create(result);
                    };

                    // Add locale marker
                    var marker = new JsonObject();
                    locales.ForEach(locale => marker[locale] = ImmutableJson.Create(string.Empty));
                    obj.Add(string.Empty, ImmutableJson.Create(marker));

                    docs.ToList().ForEach(doc => {
                        doc.Strings.ForEach(s =>
                        {
                            var path = s.Path.ToString();
                            if (stripDocumentFromPath)
                            {
                                path = path.Substring(path.IndexOf('.') + 1);
                            }

                            obj.Add($"{doc.Id}.{path}", fnConvert(s.Data));
                        });
                    });

                    var sjson = SJson.SJson.Stringify(ImmutableJson.Create(obj));
                    File.WriteAllText(exportToFilename, sjson);
                    logger.Info("Export to SJSON operation completed successfully");
                });
            }
            catch (WebException ex)
            {
                LoadingIndicator.StopStoryboard();
                logger.Error("Error", ex);
                ProposalUpdateCredentials(ex.Message);
                return;
            }

            LoadingIndicator.StopStoryboard();

            if (LogWindow != null && LogWindow.LogMessageCount > 0)
                LogWindow.Show();

            MessageBox.Show("Export to SJSON operation completed successfully", "Success", MessageBoxButton.OK);
        }

        private void DifferencesResolve(List<Difference> diff)
        {
            var dlgResult = false;
            var rememberChoice = false;

            foreach (var conflict in diff.Where(x => x.Resolution == ConflictResolution.Manual))
            {
                if (!rememberChoice)
                {
                    var frmConflict = new ConflictResolutionWindow();
                    frmConflict.SetConflictData(conflict);
                    dlgResult = frmConflict.ShowDialog() ?? false;
                    rememberChoice = frmConflict.RememberChoice;

                    if (!dlgResult)
                        conflict.Frontend.Translation = frmConflict.MineLocaleTranslation;
                }

                conflict.Resolution = dlgResult ? ConflictResolution.UseTranslatedBackend : ConflictResolution.UseTranslatedFrontend;
            }
        }

        private void ProposalUpdateCredentials(string message)
        {
            var result = MessageBox.Show("Perhaps not valid autorization data. Do the login window open?",
                "Error", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
                ((App)Application.Current).OpenAuthenticationWindow();
        }

        private void AuthenticationSettingsClick(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).OpenAuthenticationWindow();
        }

        private void CheckForUpdatesClick(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).CheckForUpdate(true);
        }

        private void ExitHandler(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        public void ShowAppUpdateAvailable()
        {
            btnAppUpdateAvailable.Visibility = Visibility.Visible;
        }
    }
}