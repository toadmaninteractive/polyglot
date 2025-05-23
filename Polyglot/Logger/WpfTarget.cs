using NLog;
using NLog.Targets;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

#if !NET_CF && !MONO && !SILVERLIGHT
namespace Polyglot.WpfLogger
{
    [Target("RichTextBox")]
    public sealed class WpfTarget : TargetWithLayout
    {
        public string ControlName { get; set; }

        internal Window TargetForm { get; set; }

        internal IWpfLoggerControl TargetControl { get; set; }

        protected override void InitializeTarget()
        {
            TargetControl = Application.Current.MainWindow.FindName(ControlName) as IWpfLoggerControl ??
                TargetForm.FindName(ControlName) as IWpfLoggerControl;
        }

        protected override void CloseTarget() //TODO exception happening
        {
	        try
	        {
                if (TargetForm != null)
				    TargetForm.Dispatcher.Invoke(() =>
				    {
					    TargetForm.Close();
					    TargetForm = null;
				    });
	        }
	        catch (Exception ex)
	        {
	        }
        }

        protected override void Write(LogEventInfo logEvent)
        {
	        if (Application.Current == null) return;

	        try
	        {
				if (Application.Current.Dispatcher.CheckAccess() == false)
				{
                    Application.Current.Dispatcher.Invoke(() => SendMessage(logEvent));
				}
				else
				{
                    SendMessage(logEvent);
				}
	        }
	        catch(Exception ex)
	        {
		        Debug.WriteLine(ex);
	        }
		}

        private void SendMessage(LogEventInfo logEvent)
        {
            if (TargetControl != null)
                TargetControl.AddMessageToControl(logEvent);
        }
    }
}
#endif