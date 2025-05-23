using NLog;
using System.Windows.Controls;

namespace Polyglot.WpfLogger
{
    class LoggerTextBox : TextBox, IWpfLoggerControl
    {
        public void AddMessageToControl(LogEventInfo logEvent)
        {
            this.Text = string.Format("[{0}] {1}", logEvent.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), logEvent.Message);
        }
    }
}
