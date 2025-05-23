using NLog;
using Polyglot.Core.Utils;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using Polyglot.Core;

namespace Polyglot
{
    public partial class AuthenticationWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private PolySettings settings;
        private CancellationTokenSource cancellationTokenSource;

        public AuthenticationWindow(PolySettings settings)
        {
            InitializeComponent();
            this.settings = settings;

            cbbxUpdateChannel.ItemsSource = Enum.GetNames(typeof(ApplicationUpdateChannel));
            cbbxUpdateChannel.SelectedItem = settings.AppUpdateChannel.ToString();

            txtbxServerUrl.Text = settings.ServerUrl;
            txtbxLogin.Text = settings.AuthUsername;
            txtbPassword.Password = settings.AuthPassword;
        }

        private void CouchSaveHandlerClick(object sender, RoutedEventArgs e)
        {
            if (!Validation(
                new Dictionary<string, string>
                {
                    { "serverUrl", txtbxServerUrl.Text },
                    { "login", txtbxLogin.Text },
                    { "password", txtbPassword.Password },
                    { "cdbName", txtbxDataBaseName.Text },
                    { "locale", txtbxLocal.Text }
                }))
                return;

            settings.ServerUrl = txtbxServerUrl.Text;
            settings.AuthUsername = txtbxLogin.Text;
            settings.AuthPassword = txtbPassword.Password;
            settings.DatabaseName = txtbxDataBaseName.Text;
            settings.LocaleTranslation = txtbxLocal.Text;

            settings.WriteConfigFile();
            this.DialogResult = true;
        }

        private void UpdateChannelHandlerClick(object sender, RoutedEventArgs e)
        {
            var selectedAppUpdateChannel = (ApplicationUpdateChannel)Enum.Parse(typeof(ApplicationUpdateChannel), (string)cbbxUpdateChannel.SelectedItem);
            if (settings.AppUpdateChannel != selectedAppUpdateChannel)
            {
                settings.AppUpdateChannel = selectedAppUpdateChannel;
                settings.WriteConfigFile();
            }

            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadAvailableDatabaseNames(txtbxLogin.Text, txtbPassword.Password, txtbxServerUrl.Text);
            if (!string.IsNullOrWhiteSpace(settings.DatabaseName) 
                && txtbxDataBaseName.Items.Contains(settings.DatabaseName))
            {
                txtbxDataBaseName.Text = settings.DatabaseName;
            }
            else
                return;
                

            await ReloadAvailableLocales(txtbxLogin.Text, txtbPassword.Password, txtbxServerUrl.Text, txtbxDataBaseName.Text);
            if (txtbxLocal.Items.Contains(settings.LocaleTranslation))
            {
                txtbxLocal.Text = settings.LocaleTranslation;
                return;
            }
        }

        #region Form control event handlers
        /// <summary>
        /// Button click
        /// </summary>
        private async void DataBaseNamesReload(object sender, RoutedEventArgs e)
        {
            await ReloadAvailableDatabaseNames(txtbxLogin.Text, txtbPassword.Password, txtbxServerUrl.Text);
        }

        /// <summary>
        /// Button click
        /// </summary>
        private async void LocalesReload(object sender, RoutedEventArgs e)
        {
            await ReloadAvailableLocales(txtbxLogin.Text, txtbPassword.Password, txtbxServerUrl.Text, txtbxDataBaseName.Text);
        }

        private async void DataBaseNameChangedHandler(object sender, EventArgs e)
        {
            await ReloadAvailableLocales(txtbxLogin.Text, txtbPassword.Password, txtbxServerUrl.Text, txtbxDataBaseName.Text);
        }

        private async void DataBaseNameSelectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            var candidate = e.AddedItems.OfType<string>().FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(candidate))
                await ReloadAvailableLocales(txtbxLogin.Text, txtbPassword.Password, txtbxServerUrl.Text, candidate);
        }
        #endregion

        #region cdb interraction
        private async Task ReloadAvailableDatabaseNames(string login, string password, string serverUrl)
        {
            if (!Validation(
                new Dictionary<string, string>
                {
                    { "serverUrl", txtbxServerUrl.Text },
                    { "login", txtbxLogin.Text },
                    { "password", txtbPassword.Password }
                }))
                return;

            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                var databaseNames =
                    await CdbServerUtils.GetDatabaseNamesAsync(
                        login,
                        password,
                        serverUrl,
                        cancellationTokenSource.Token);

                txtbxDataBaseName.ItemsSource = databaseNames;
            }
            catch (WebException ex)
            {
                txtErrors.Text = "Can not load available database names";
                logger.Error(ex);
                return;
            }
            catch (Exception ex)
            {
                txtErrors.Text = "Unexpected exception";
                logger.Error(ex);
                return;
            }
        }

        private async Task ReloadAvailableLocales(string login, string password, string serverUrl, string cdbName)
        {
            if (!Validation(
                new Dictionary<string, string>
                {
                    { "serverUrl", txtbxServerUrl.Text },
                    { "login", txtbxLogin.Text },
                    { "password", txtbPassword.Password },
                    { "cdbName", txtbxDataBaseName.Text }
                }))
                return;

            try
            {
                cancellationTokenSource = new CancellationTokenSource();

                var locales =
                    await CdbServerUtils.GetAvailableLocaleAsync(
                        login,
                        password,
                        serverUrl,
                        cdbName,
                        cancellationTokenSource.Token);

                txtbxLocal.ItemsSource = locales;
            }
            catch (WebException ex)
            {
                txtErrors.Text = "Can not load available locales";
                logger.Error(ex);
            }
            catch (Exception ex)
            {
                txtErrors.Text = "Unexpected exception";
                logger.Error(ex);
            }
        }
        #endregion

        private bool Validation(Dictionary<string, string> parametrs)
        {
            foreach (var parametr in parametrs)
            {
                bool isError = string.IsNullOrWhiteSpace(parametr.Value);

                if (isError)
                {
                    switch (parametr.Key)
                    {
                        case "serverUrl":
                            txtErrors.Text = "Server URL is not specified";
                            break;
                        case "password":
                            txtErrors.Text = "Password is not specified";
                            break;
                        case "login":
                            txtErrors.Text = "Login is not specified";
                            break;
                        case "cdbName":
                            txtErrors.Text = "Data base name is empty";
                            break;
                        case "locale":
                            txtErrors.Text = "Local is empty";
                            break;
                        default:
                            break;
                    }

                    return false;
                }
            }

            txtErrors.Text = string.Empty;
            return true;
        }
    }
}