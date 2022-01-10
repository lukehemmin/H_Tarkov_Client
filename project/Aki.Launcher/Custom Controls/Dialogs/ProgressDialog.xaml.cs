/* ProgressDialog.xaml.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 */

using Aki.Launcher.Interfaces;
using Aki.Launcher.Models.Launcher;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Aki.Launcher.Custom_Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : UserControl, IReturnDialogResult
    {
        IUpdateProgress TaskInfo;

        public ProgressDialog(IUpdateProgress TaskInfo)
        {
            InitializeComponent();

            this.TaskInfo = TaskInfo;
            TaskInfo.ProgressChanged += TaskInfo_ProgressChanged;
            TaskInfo.TaskCancelled += TaskInfo_TaskCancelled;

            if(TaskInfo is IUpdateSubProgress subProgressInfo)
            {
                SubProgress_Bar.Visibility = Visibility.Visible;
                subProgressInfo.SubProgressChanged += SubProgressInfo_SubProgressChanged;
            }
        }

        private void SubProgressInfo_SubProgressChanged(object sender, ProgressInfo e)
        {
            UpdateProgress(SubProgress_Bar, SubInfoText_Label, e.Percentage, e.Message);
        }

        private void TaskInfo_TaskCancelled(object sender, object e)
        {
            RaiseResultsReady(e);
        }

        private void TaskInfo_ProgressChanged(object sender, ProgressInfo e)
        {
            UpdateProgress(Progress_Bar, InfoText_Label, e.Percentage, e.Message);
        }

        /// <summary>
        /// Updates the progress dialog
        /// </summary>
        /// <param name="Percentage">The percentage the progress bar should display</param>
        /// <param name="NewMessage">The text to display in the dialog</param>
        private void UpdateProgress(ProgressBar progressBar, Label infoLabel, int Percentage, string NewMessage = "")
        {
            //update progress bar (optionally update the message)
            if (!String.IsNullOrWhiteSpace(NewMessage)) Application.Current.Dispatcher.Invoke(() => { infoLabel.Content = NewMessage; });

            Application.Current.Dispatcher.Invoke(() => { progressBar.Value = Percentage; });

            //if the main progress bar percentage is 100%, return our dialog results (to close the dialog)
            if (progressBar == Progress_Bar && Percentage >= 100)
            {
                RaiseResultsReady(null);
                return;
            }
        }

        /// <summary>
        /// Set the progress bar's indeterminate value
        /// </summary>
        /// <param name="IsIndeterminate"></param>
        public void UpdateIndeterminateProgress(bool IsIndeterminate) => Application.Current.Dispatcher.Invoke(() => { Progress_Bar.IsIndeterminate = IsIndeterminate; });

        public event EventHandler<object> ResultsReady;

        protected virtual void RaiseResultsReady(object results)
        {
            ResultsReady?.Invoke(this, results);
        }

        private void ProgressDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Task.Factory.StartNew(TaskInfo.ProgressableTask);
        }
    }
}
