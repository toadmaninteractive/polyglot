using NLog;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Polyglot.WpfLogger
{
    public class LoggerListBox : ListBox, IWpfLoggerControl
    {
        public void AddMessageToControl(LogEventInfo logEvent)
        {
            var wpfLogMessage = new WpfLogMessage(logEvent);

            Icon systemIcon;
            switch (logEvent.Level.Name)
            {
                case "Warn":
                    systemIcon = SystemIcons.Warning;
                    break;
                case "Error":
                    systemIcon = SystemIcons.Error;
                    break;
                default:
                    systemIcon = SystemIcons.Warning;
                    break;
            }

            wpfLogMessage.Image = Imaging.CreateBitmapSourceFromHIcon(systemIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            

            this.Items.Add(wpfLogMessage);
            this.ScrollIntoView(wpfLogMessage);
        }
    }
}
