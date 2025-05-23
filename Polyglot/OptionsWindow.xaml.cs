using Polyglot.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Polyglot
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        /// <summary>
        /// For Fetch
        /// </summary>
        public OptionsWindow(string locale, List<string> cardCategoriesFilter, List<string> documentNamesFilter)
        {
            InitializeComponent();
            txtbxLocale.Text = locale;
            txtbxDocumentFilter.Text = string.Join(Environment.NewLine, documentNamesFilter.Select(x => x.Trim()));
            txtbxCategoryFilter.Text = string.Join(Environment.NewLine, cardCategoriesFilter.Select(x => x.Trim()));
            Title = "Fetching remote strings";
        }

        public OptionsWindow(string locale)
        {
            InitializeComponent();
            txtbxLocale.Text = locale;

            chkFetchValid.Visibility = Visibility.Collapsed;
            chkFetchInvalid.Visibility = Visibility.Collapsed;
            chkFetchUntranslated.Visibility = Visibility.Collapsed;
            groupDocumentFilderControl.Visibility = Visibility.Collapsed;
            groupCategoryFilderControl.Visibility = Visibility.Collapsed;
            Height = 200;
            Title = "Submitting translated strings";
        }

        public string Local
        {
            get
            {
                return txtbxLocale.Text;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void chkIsValueRegularExtensionClickHandler(object sender, RoutedEventArgs e)
        {
            UpdateModeCaption();
        }
        

        private void FilterTextChangedHandler(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateModeCaption();
        }

        private void UpdateModeCaption()
        {
            txtModeCaptionControl.Visibility = string.IsNullOrWhiteSpace(txtbxDocumentFilter.Text) ? Visibility.Collapsed : Visibility.Visible;
            if (chkIsValueRegularExtension.IsChecked ?? false)
            {
                txtModeCaptionControl.Content = "Regex";
                txtModeCaptionControl.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#829460"));
            }
            else
            {
                txtModeCaptionControl.Content = "Regular";
                txtModeCaptionControl.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#537188"));
            }
        }
    }
}
