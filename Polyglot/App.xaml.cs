using NLog;
using NLog.Config;
using NLog.Targets.Wrappers;
using Polyglot.Core;
using Polyglot.Core.Utils;
using Polyglot.Couch;
using Polyglot.WpfLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Polyglot
{
    public partial class App : Application
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        private PolySettings settings;
        private ApplicationUpdater updater = new ApplicationUpdater();
        private Timer updateTimer;
        private const int checkUpdateInterval = 10000;
        private bool isFirstRun;


        void App_Startup(object sender, StartupEventArgs e)
        {
            logger.Trace("Starting...");
            var polyIo = new PolyIO();
            polyIo.CreateDirectoryStructure();
            isFirstRun = !File.Exists(polyIo.ConfigFilePath);

            settings = new PolySettings(polyIo.ConfigFilePath);

            var logWindow = new LogWindow();
            logWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            MainWindow = new ManagementWindow
            {
                Processor = new LocalizationProcessor
                            {
                                Advance = settings.Advance,
                                Backend = new CouchDbBackend(settings),
                                Exporter = new XmlExporter
                                {
                                    XmlReadable = settings.XmlReadable,
                                    SuppressCdata = settings.SuppressCdata
                                },
                                Importer = new XmlImporter()
                            },  
                locale = settings.LocaleTranslation,
                LogWindow = logWindow,
                Title = $"Polyglot {Utils.GetAppCaptionRevosionFormat()}"
        };

            SetLoggerTargets(MainWindow, "TextLog", new List<LogLevel> { LogLevel.Info, LogLevel.Error });
            SetLoggerTargets(logWindow, "lbLog", new List<LogLevel> { LogLevel.Warn });
            LogManager.ReconfigExistingLoggers();
            
            LogManager.GetCurrentClassLogger().Info("Polyglot started");
            MainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            MainWindow.Show();

            updateTimer = new Timer((state) => 
            {
                updater.CheckForUpdates(settings.AppUpdateChannel, false).ContinueWith(UpdateGui);
            }, null, Timeout.Infinite, checkUpdateInterval);
            updateTimer.Change(0, checkUpdateInterval);

            if (isFirstRun)
                OpenAuthenticationWindow();
        }

        private void UpdateGui(Task<bool> obj)
        {
            Dispatcher.Invoke(() => 
            {
                if (obj.Result)
                    ((ManagementWindow)MainWindow).ShowAppUpdateAvailable();
            });
        }

        public bool CheckAuthenticationFileOnDefaultValues()
        {
            var authenticationWindow = new AuthenticationWindow(settings);
            if (settings.HasDefaultAuthenticationValue())
                return authenticationWindow.ShowDialog() ?? false;

            return true;
        }

        public void OpenAuthenticationWindow()
        {
            new AuthenticationWindow(settings).ShowDialog();
        }

        public async void CheckForUpdate(bool notify = false)
        {
            await updater.CheckForUpdates(settings.AppUpdateChannel, true, notify);
        }

        /// <summary>
        /// config logger for WPF textBox targets. Set targets and rules of logger
        /// </summary>
        private static void SetLoggerTargets(Window window, string controlName, List<LogLevel> enableLevels)
        {
            var target = new WpfTarget
                {
                    Name = string.Format("{0}Target", controlName),
                    Layout = "${message}",
                    ControlName = controlName,
                    TargetForm = window
                };

            var rule = new LoggingRule("*", LogLevel.Trace, target);

            for (int i = LogLevel.Trace.Ordinal; i < LogLevel.Off.Ordinal; i++)
            {
                var logLevel = LogLevel.FromOrdinal(i);
                if (enableLevels.Contains(logLevel))
                    continue;

                rule.DisableLoggingForLevel(logLevel);
            }

            LogManager.Configuration.LoggingRules.Add(rule);
            LogManager.Configuration.AddTarget(target.Name, target);
        }
    }
}