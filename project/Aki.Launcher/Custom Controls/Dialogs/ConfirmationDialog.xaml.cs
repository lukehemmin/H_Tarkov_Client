/* ConfirmationDialog.xaml.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 */

using Aki.Launcher.Interfaces;
using System;
using System.Windows.Controls;

namespace Aki.Launcher.Custom_Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : UserControl, IReturnDialogResult
    {
        /// <summary>
        /// A simple Yes/No Style dialog.
        /// </summary>
        /// <param name="Message">The message to display</param>
        /// <param name="ConfirmButtonText">The confirm button text. Default is Yes.</param>
        /// <param name="DenyButtonText">The deny button text. Default is No</param>
        /// <remarks>This dialog returns true if the confirm button is pressed, or false if the deny button is pressed.</remarks>
        public ConfirmationDialog(string Message, string ConfirmButtonText = "Yes", string DenyButtonText = "No")
        {
            InitializeComponent();
            MessageLabel.Content = Message;
            ConfirmButton.Content = ConfirmButtonText;
            DenyButton.Content = DenyButtonText;
        }

        public event EventHandler<object> ResultsReady;

        protected virtual void RaiseResultsReady(object results)
        {
            ResultsReady?.Invoke(this, results);
        }

        private void ConfirmButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RaiseResultsReady(true);
        }

        private void DenyButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RaiseResultsReady(false);
        }
    }
}
