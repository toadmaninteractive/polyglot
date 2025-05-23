using System;
using System.IO;
using System.Reflection;

namespace Polyglot.Core.Utils
{
    public class PolyIO
    {
        public string ConfigFilePath { get; }

        private string configFileName = "settings.xml";
        private string appDataFolderName = @"Toadman Interactive\Polyglot";
        
        public PolyIO()
        {
            ConfigFilePath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    appDataFolderName, 
                    configFileName);
        }

        public void CreateDirectoryStructure()
        {
            var configFolder =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    appDataFolderName);

            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);
        }
    }
}