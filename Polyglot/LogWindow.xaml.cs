using System.Linq;
using System.Windows;
using System.Collections.Generic;
using Polyglot.WpfLogger;

namespace Polyglot
{
    public partial class LogWindow : Window
    {
        public int LogMessageCount
        {
            get
            {
                return lbLog.Items.Count;
            }
        }

        public LogWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Clear(object sender, RoutedEventArgs e)
        {
            lbLog.Items.Clear();
        }

        private void MenuItem_Copy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(string.Join(System.Environment.NewLine, lbLog.SelectedItems.Cast<WpfLogMessage>().Select(s => s.MessageText).ToArray()));
        }

        private void MenuItem_CopyAll(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(string.Join(System.Environment.NewLine, lbLog.Items.Cast<WpfLogMessage>().Select(s => s.MessageText).ToArray()));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            lbLog.Items.Clear();

            foreach (var window in Application.Current.Windows)
		        if (window is ManagementWindow)
                {
                    e.Cancel = true;
                    this.Hide();
                    return;
                }

            base.OnClosing(e);   
        }
    }
}
