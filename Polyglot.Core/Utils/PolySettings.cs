using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Polyglot.Core.Utils
{
    public class PolySettings
    {
        public string ServerUrl;
        public string DatabaseName;
        public bool UseAuthentication;
        public string AuthUsername;
        public string AuthPassword;
        public string LocaleTranslation;
        public bool AskCredentials;
        public string LastFolder;
        public bool Advance;
        public bool XmlReadable;
        public bool SuppressCdata;
        public List<string> CardCategoryFilter = new List<string>();
        public List<string> DocumentNameFilter = new List<string>();
        public ApplicationUpdateChannel AppUpdateChannel;

        private string configFileName;
        private string defaultServerUrl = "https://cdb.yourcompany.com";
        private string defaultDatabaseName = string.Empty;
        private bool defaultUseAuthentication = true;
        private string defaultAuthUsername = string.Empty;
        private string defaultAuthPassword = string.Empty;
        private string defaultLocaleTranslation = "ru";
        private string defaultLastFolder = ".\\";
        private bool defaultAskCredentials = true;
        private bool defaultAdvance = false;
        private bool defaultXmlReadable = true;
        private bool defaultSuppressCdata = true;
        public ApplicationUpdateChannel defaultAppUpdateChannel = ApplicationUpdateChannel.Stable;

        public PolySettings(string config_fileName)
        {
            configFileName = config_fileName;

            if (File.Exists(configFileName))
            {
                ReadConfigFile();
                WriteConfigFile();
            }
            else
            {
                ResetToDefault();
                WriteConfigFile();
            }
        }

        public void ReadConfigFile()
        {
            // Reset settings
            Reset();

            // Load XML document
            XmlReader xmlReader = XmlReader.Create(configFileName);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlReader);

            // Define node
            XmlNode xn;

            // Get parameters
            xn = xmlDoc.SelectSingleNode("/settings/serverUrl");
            if (xn != null) ServerUrl = xn.InnerText;

            xn = xmlDoc.SelectSingleNode("/settings/databaseName");
            if (xn != null) DatabaseName = xn.InnerText;

            xn = xmlDoc.SelectSingleNode("/settings/useAuthentication");
            if (xn != null) UseAuthentication = Convert.ToBoolean(xn.InnerText);

            xn = xmlDoc.SelectSingleNode("/settings/authUsername");
            if (xn != null) AuthUsername = xn.InnerText;

            xn = xmlDoc.SelectSingleNode("/settings/authPassword");
            if (xn != null) AuthPassword = xn.InnerText;

            xn = xmlDoc.SelectSingleNode("/settings/localeTranslation");
            if (xn != null) LocaleTranslation = xn.InnerText;

            xn = xmlDoc.SelectSingleNode("/settings/askCredentials");
            if (xn != null) AskCredentials = Convert.ToBoolean(xn.InnerText);

            xn = xmlDoc.SelectSingleNode("/settings/advance");
            if (xn != null) Advance = Convert.ToBoolean(xn.InnerText);

            xn = xmlDoc.SelectSingleNode("/settings/lastFolder");
            if (xn != null) LastFolder = xn.InnerText;

            xn = xmlDoc.SelectSingleNode("/settings/xmlReadable");
            if (xn != null) XmlReadable = Convert.ToBoolean(xn.InnerText);

            xn = xmlDoc.SelectSingleNode("/settings/suppressCdata");
            if (xn != null) SuppressCdata = Convert.ToBoolean(xn.InnerText);

            xn = xmlDoc.SelectSingleNode("/settings/cardCategoryFilter");
            if (xn != null)
                foreach (XmlNode cardCategoryName in xn.ChildNodes)
                    CardCategoryFilter.Add(cardCategoryName.InnerText);

            xn = xmlDoc.SelectSingleNode("/settings/documentNameFilter");
            if (xn != null)
                foreach (XmlNode documentNameFilter in xn.ChildNodes)
                    DocumentNameFilter.Add(documentNameFilter.InnerText);

            xn = xmlDoc.SelectSingleNode("/settings/appUpdateChannel");
            ApplicationUpdateChannel updateChannel;
            if (xn != null && Enum.TryParse(xn.InnerText, out updateChannel))
                AppUpdateChannel = updateChannel;

            xmlReader.Close();
        }

        public void WriteConfigFile()
        {
            // Do not add UTF-8 BOM mark
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.NewLineChars = "\r\n";
            writerSettings.Indent = true;
            writerSettings.IndentChars = "\t";
            writerSettings.Encoding = new UTF8Encoding(false);

            using (XmlWriter xml = XmlWriter.Create(configFileName, writerSettings))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("settings"); // start settings

                xml.WriteElementString("serverUrl", ServerUrl);
                xml.WriteElementString("databaseName", DatabaseName);
                xml.WriteElementString("useAuthentication", UseAuthentication.ToString());
                xml.WriteElementString("authUsername", AuthUsername);
                xml.WriteElementString("authPassword", AuthPassword);
                xml.WriteElementString("localeTranslation", LocaleTranslation);
                xml.WriteElementString("askCredentials", AskCredentials.ToString());
                xml.WriteElementString("advance", Advance.ToString());
                xml.WriteElementString("lastFolder", LastFolder);
                xml.WriteElementString("xmlReadable", XmlReadable.ToString());
                xml.WriteElementString("suppressCdata", SuppressCdata.ToString());

                if (CardCategoryFilter.Count > 0)
                {
                    xml.WriteStartElement("cardCategoryFilter"); // start cardCategoryFilter
                    foreach (var cardCategoryName in CardCategoryFilter)
                        xml.WriteElementString("cardCategoryName", cardCategoryName);
                    xml.WriteEndElement(); // end cardCategoryFilter
                }

                if (DocumentNameFilter.Count > 0)
                {
                    xml.WriteStartElement("documentNameFilter"); // start documentNameFilter
                    foreach (var documentName in DocumentNameFilter)
                        xml.WriteElementString("documentName", documentName);
                    xml.WriteEndElement(); // end documentNameFilter
                }
                xml.WriteElementString("appUpdateChannel", AppUpdateChannel.ToString());

                xml.WriteEndElement(); // end settings
                xml.WriteEndDocument();
                xml.Close();
            }
        }

        public bool HasDefaultAuthenticationValue()
        {
            return DatabaseName == defaultDatabaseName
                || AuthUsername == defaultAuthUsername
                || AuthPassword == defaultAuthPassword;
        }

        private void Reset()
        {
            ServerUrl = null;
            DatabaseName = null;
            UseAuthentication = false;
            AuthUsername = null;
            AuthPassword = null;
            LocaleTranslation = null;
            AskCredentials = true;
            Advance = false;
            LastFolder = null;
            XmlReadable = true;
            SuppressCdata = true;
            CardCategoryFilter.Clear();
            DocumentNameFilter.Clear();
            AppUpdateChannel = ApplicationUpdateChannel.Stable;
        }

        private void ResetToDefault()
        {
            ServerUrl = defaultServerUrl;
            DatabaseName = defaultDatabaseName;
            UseAuthentication = defaultUseAuthentication;
            AuthUsername = defaultAuthUsername;
            AuthPassword = defaultAuthPassword;
            LocaleTranslation = defaultLocaleTranslation;
            AskCredentials = defaultAskCredentials;
            Advance = defaultAdvance;
            XmlReadable = true;
            SuppressCdata = defaultSuppressCdata;
            LastFolder = defaultLastFolder;
            CardCategoryFilter.Clear();
            DocumentNameFilter.Clear();
            AppUpdateChannel = defaultAppUpdateChannel;
        }
    }
}