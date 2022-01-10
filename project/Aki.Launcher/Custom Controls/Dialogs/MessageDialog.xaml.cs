using Aki.Launcher.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Aki.Launcher.Custom_Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog : UserControl, IReturnDialogResult
    {
        public MessageDialog(string Message, string CloseButtonText = "OK")
        {
            InitializeComponent();
            MessageLabel.Content = Message;
            CloseButton.Content = CloseButtonText;
        }

        public event EventHandler<object> ResultsReady;
        protected virtual void RaiseResultsReady()
        {
            ResultsReady?.Invoke(this, null);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseResultsReady();
        }
    }
}
