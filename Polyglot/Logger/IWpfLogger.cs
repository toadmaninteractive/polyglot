using NLog;

namespace Polyglot.WpfLogger
{
    interface IWpfLoggerControl
    {
        void AddMessageToControl(LogEventInfo logEvent);
    }
}
