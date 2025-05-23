using Polyglot.Core;
using System.Windows;

namespace Polyglot
{
    public partial class ConflictResolutionWindow : Window
    {
        public ConflictResolutionWindow()
        {
            InitializeComponent();
        }

        internal void SetConflictData(Difference conflict)
        {
            txtKey.Content = conflict.Id + " / " + conflict.Path;
            txtbxTheirsText.Text = conflict.Backend.Text;
            txtbxTheirsLocaleOriginal.Text = conflict.Backend.Original;
            txtbxTheirsLocaleTranslation.Text = conflict.Backend.Translation;
            txtbxMineLocaleOriginal.Text = conflict.Frontend.Original;
            txtbxMineLocaleTranslation.Text = conflict.Frontend.Translation;
            chkRemember.IsChecked = false;
        }

        internal bool RememberChoice
        {
            get
            {
                return chkRemember.IsChecked ?? false;
            }
        }

        internal string MineLocaleTranslation
        {
            get
            {
                return txtbxMineLocaleTranslation.Text;
            }
        }

        private void btnUseTheirs_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnUseMine_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
