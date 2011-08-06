using System;
using System.Xml;
using System.Windows;

using FocusedGames;
using FocusedGames.Xml;
using FocusedGames.Data;
using FocusedGames.Globalization;
using System.IO;

namespace Korkboard
{
    public class AppSettings : NotifyPropertyChangedBase
    {
        public static AppSettings Load(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);

            XmlConverter xmlConverter = new XmlConverter();
            ConversionContext context = new ConversionContext { Culture = Culture.Invariant, Device = xmlDocument };

            AppSettings settings = xmlConverter.ConvertFrom(xmlDocument, context) as AppSettings;

            return settings;
        }

        private bool alwaysOnTop = false;
        private int itemTimeLimit = 0;
        private int itemNumberLimit = 25;

        private bool enableTextCopy = true;
        private bool enableFileCopy = false;
        private bool enableImagCopy = true;

        public AppSettings GetCopy()
        {
            System.Diagnostics.Debug.WriteLine("Copying AppSettings");

            AppSettings settings = new AppSettings();

            settings.IsDirtyEnabled = false;

            CopyTo(settings);

            settings.IsDirtyEnabled = true;

            return settings;
        }

        public void CopyTo(AppSettings settings)
        {
            System.Diagnostics.Debug.WriteLine("    AlwaysOnTop: " + AlwaysOnTop.ToString());

            settings.AlwaysOnTop = AlwaysOnTop;
            settings.TimeLimitInMinutes = TimeLimitInMinutes;
            settings.ItemNumberLimit = ItemNumberLimit;
            settings.IsTextCopyEnabled = IsTextCopyEnabled;
            settings.IsFileCopyEnabled = IsFileCopyEnabled;
            settings.IsImageCopyEnabled = IsImageCopyEnabled;
        }

        public void Save(string filename)
        {
            string folder = Path.GetDirectoryName(filename);

            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            XmlConverter xmlConverter = new XmlConverter();
            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", null, null));

            ConversionContext context = new ConversionContext { Culture = Culture.Invariant, Device = xmlDocument };

            XmlNode xmlNode = xmlConverter.ConvertTo(this, context) as XmlNode;

            if (xmlNode != null)
            {
                xmlDocument.AppendChild(xmlNode);

                xmlDocument.Save(filename);
            }
        }

        /// <summary>
        /// Gets or Sets whether or not the main window remains on top of other windows.
        /// </summary>
        public bool AlwaysOnTop
        {
            get { return alwaysOnTop; }
            set
            {
                alwaysOnTop = value;

                OnPropertyChanged("AlwaysOnTop");
            }
        }

        /// <summary>
        /// Gets or Sets the time limit of items on the list.
        /// </summary>
        public int TimeLimitInMinutes
        {
            get { return itemTimeLimit; }
            set
            {
                itemTimeLimit = value;

                OnPropertyChanged("TimeLimitInMinutes");
            }
        }

        /// <summary>
        /// Gets or Sets the limit of items on the list.
        /// </summary>
        public int ItemNumberLimit
        {
            get { return itemNumberLimit; }
            set
            {
                itemNumberLimit = value;

                OnPropertyChanged("ItemNumberLimit");
            }
        }

        public bool IsTextCopyEnabled
        {
            get { return enableTextCopy; }
            set
            {
                enableTextCopy = value;

                OnPropertyChanged("IsTextCopyEnabled");
            }
        }

        public bool IsFileCopyEnabled
        {
            get { return enableFileCopy; }
            set
            {
                enableFileCopy = value;

                OnPropertyChanged("IsFileCopyEnabled");
            }
        }

        public bool IsImageCopyEnabled
        {
            get { return enableImagCopy; }
            set
            {
                enableImagCopy = value;

                OnPropertyChanged("IsImageCopyEnabled");
            }
        }
    }
}
