using NLog;
using System;
using System.Windows.Media.Imaging;

namespace Polyglot.WpfLogger
{
    public class WpfLogMessage
    {
        public BitmapSource Image { get; set; }

        public string MessageText
        {
            get
            {
                return source.Message;
            }
        }

        private LogEventInfo source;

        public WpfLogMessage(LogEventInfo source)
        {
            this.source = source;
        }
    }
}
