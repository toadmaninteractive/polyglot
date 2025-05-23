using Polyglot.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Polyglot
{
    public class ApplicationUpdater
    {
        public string Rev;

        private const string UpdateUrl = @"https://tdn.yourcompany.com/public/update/polyglot/";
        private HashSet<ApplicationUpdateChannel> updating;

        public ApplicationUpdater()
        {
            updating = new HashSet<ApplicationUpdateChannel>();
        }

        public async Task<bool> CheckForUpdates(ApplicationUpdateChannel targetChannel, bool autoInstall = false, bool notify = false)
        {
            if (File.Exists(".ignoreupdate"))
                return false;

            var updateFileName = await CheckForUpdatesAsync(targetChannel);

            if (string.IsNullOrEmpty(updateFileName)) {
                if (notify)
                    MessageBox.Show("You are using the latest version", "No updates available", MessageBoxButton.OK);

                return false;
            }

            if (autoInstall)
            {
                Process.Start(updateFileName, "/SILENT");
                Application.Current.Shutdown();
            }

            return true;
        }

        public static string GetCurrentRevision()
        {
            return File.Exists("rev.conf") ? File.ReadAllText("rev.conf").Trim() : null;
        }

        async Task<string> CheckForUpdatesAsync(ApplicationUpdateChannel channel)
        {
            if (!updating.Add(channel))
                return string.Empty;

            try
            {
                var tempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\Polyglot");

                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);

                var channelUrl = UpdateUrl + channel.ToString().ToLower();
                Rev = GetCurrentRevision();
                WebClient webClient = new WebClient();
                var remoteRev = (await webClient.DownloadStringTaskAsync(channelUrl + "/rev.conf")).Trim();

                if (remoteRev != Rev)
                {
                    var tempFileName = $"polyglot_setup_{remoteRev}.exe";
                    var tempFilePath = Path.Combine(tempDir, tempFileName);
                    var downloadFilePath = $"{tempFilePath}.download";

                    // Delete existing partially downloaded setup file
                    if (File.Exists(downloadFilePath))
                        File.Delete(downloadFilePath);

                    // Delete all files except setup file
                    Directory
                        .GetFiles(tempDir)
                        .Where(f => f.ToLowerInvariant() != tempFilePath.ToLowerInvariant())
                        .ToList()
                        .ForEach(f => File.Delete(f));

                    if (!File.Exists(tempFilePath))
                    {
                        await webClient.DownloadFileTaskAsync(channelUrl + @"/polyglot_setup.exe", downloadFilePath);
                        File.Move(downloadFilePath, tempFilePath);
                    }

                    return tempFilePath;
                }
                else
                    return string.Empty;
            }
            finally
            {
                updating.Remove(channel);
            }
        }
    }
}