/* GameStarterFrontend.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Erica Taylor
 * AUTHORS:
 * Erica Taylor
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aki.ByteBanger;
using Aki.Launcher.Custom_Controls;
using Aki.Launcher.Custom_Controls.Dialogs;
using Aki.Launcher.Interfaces;
using Aki.Launcher.Models.Launcher;

namespace Aki.Launcher.Helpers
{
    public class GameStarterFrontend : IGameStarterFrontend
    {
        class PatchTask : IUpdateProgress
        {
            private readonly IAsyncEnumerator<PatchResultInfo> _enumerator;
            public Action ProgressableTask => RunTask;
            public event EventHandler<object> TaskCancelled;
            public event EventHandler<ProgressInfo> ProgressChanged;

            public PatchTask(IAsyncEnumerator<PatchResultInfo> enumerator)
            {
                _enumerator = enumerator;
            }

            private async Task RunAsync()
            {
                while (await _enumerator.MoveNextAsync())
                {
                    var info = _enumerator.Current;

                    if (info.OK)
                    {
                        ProgressChanged?.Invoke(this,
                            new ProgressInfo(info.PercentComplete, LocalizationProvider.Instance.patching));
                    }
                    else
                    {
                        TaskCancelled?.Invoke(this, info);
                        return;
                    }
                }

                ProgressChanged?.Invoke(this, new ProgressInfo(100, LocalizationProvider.Instance.ok));
                TaskCancelled?.Invoke(this, new PatchResultInfo(PatchResultType.Success, 1, 1));
            }

            private void RunTask()
            {
                ProgressChanged?.Invoke(this, new ProgressInfo(0, LocalizationProvider.Instance.patching));

                RunAsync()
                    .ContinueWith(task =>
                    {
                        if (task.Exception != null)
                            TaskCancelled?.Invoke(this, task.Exception);
                    })
                    .Start();
            }
        }

        private async Task<PatchResultInfo> ShowPatchProgress(IAsyncEnumerator<PatchResultInfo> enumerator)
        {
            var dialog = new ProgressDialog(new PatchTask(enumerator));
            dialog.UpdateIndeterminateProgress(true);
            
            var result = await DialogHost.ShowDialog(dialog);
            if (result is Exception ex)
            {
                var msgDialog = new MessageDialog(ex.Message);
                await DialogHost.ShowDialog(msgDialog);
                throw new TaskCanceledException();
            }

            if (result is PatchResultInfo patchResultInfo)
            {
                return patchResultInfo;
            }

            if (result != null)
            {
                throw new Exception("ProgressDialog returned unexpected result");
            }

            return new PatchResultInfo(PatchResultType.Success, 1, 1);
        }

        public async Task CompletePatchTask(IAsyncEnumerable<PatchResultInfo> task)
        {
            var enumerator = task.GetAsyncEnumerator();
            var result = await ShowPatchProgress(enumerator);

            if (result.Status == PatchResultType.InputChecksumMismatch)
            {
                var confirmContinuePatching = new ConfirmationDialog(
                    LocalizationProvider.Instance.file_mismatch_dialog_message, 
                    LocalizationProvider.Instance.yes,
                    LocalizationProvider.Instance.no);

                var confirmResult = await DialogHost.ShowDialog(confirmContinuePatching);
                if (confirmResult is true)
                {
                    result = await ShowPatchProgress(enumerator);
                    if (!result.OK)
                    {
                        throw new TaskCanceledException();
                    }
                }
                else
                {
                    throw new TaskCanceledException();
                }
            }
            else if (!result.OK)
            {
                throw new TaskCanceledException();
            }
        }
    }
}