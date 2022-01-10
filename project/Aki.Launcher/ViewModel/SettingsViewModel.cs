/* SettingsViewModel.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 * Merijn Hendriks
 */


using Aki.Launcher.Custom_Controls;
using Aki.Launcher.Custom_Controls.Dialogs;
using Aki.Launcher.Generics;
using Aki.Launcher.Generics.AsyncCommand;
using Aki.Launcher.Helpers;
using Aki.Launcher.Models.Launcher;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace Aki.Launcher.ViewModel
{
    public class SettingsViewModel
    {
        public GenericICommand BackCommand { get; set; }
        public GenericICommand RemoveServerCommand { get; set; }
        public GenericICommand SetServerAsDefaultCommand { get; set; }
        public GenericICommand AddServerCommand { get; set; }
        public GenericICommand SaveNewServerCommand { get; set; }
        public GenericICommand ShowServerListCommand { get; set; }
        public GenericICommand CleanTempFilesCommand { get; set; }
        public GenericICommand SelectGameFolderCommand { get; set; }
        public GenericICommand OpenGameFolderCommand { get; set; }
        public GenericICommand RemoveRegistryKeysCommand { get; set; }
        public AwaitableDelegateCommand ClearGameSettingsCommand { get; set; }
        public GenericICommand ReApplyPatchCommand { get; set; }
        public LocaleCollection Locales { get; set; } = new LocaleCollection();
        private NavigationViewModel navigationViewModel { get; set; }

        private GameStarter gameStarter = new GameStarter(new GameStarterFrontend());
        public SettingsViewModel(NavigationViewModel viewModel)
        {
            navigationViewModel = viewModel;

            #region Settings Commands
            //BackCommand = new GenericICommand(OnBackCommand);
            CleanTempFilesCommand = new GenericICommand(OnCleanTempFilesCommand);
            SelectGameFolderCommand = new GenericICommand(OnSelectGameFolderCommand);
            OpenGameFolderCommand = new GenericICommand(OnOpenGameFolderCommand);
            RemoveRegistryKeysCommand = new GenericICommand(OnRemoveRegistryKeysCommand);
            ClearGameSettingsCommand = new AwaitableDelegateCommand(OnClearGameSettingsCommand);
            ReApplyPatchCommand = new GenericICommand(OnReApplyPatchCommand);
            #endregion

            Application.Current.MainWindow.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LauncherSettingsProvider.Instance.SaveSettings();
        }

        #region General Use Methods
        /// <summary>
        /// Get a folder using a folder browse dialog
        /// </summary>
        /// <returns>returns the path to the selected folder or null</returns>
        private string GetFolderPath()
        {
            using (WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog())
            {
                WinForms.DialogResult result = dialog.ShowDialog();

                if (result == WinForms.DialogResult.OK && !String.IsNullOrEmpty(dialog.SelectedPath))
                {
                    return dialog.SelectedPath;
                }
            }

            return null;
        }
        #endregion

        #region Settings Commands

        public void OnReApplyPatchCommand(object parameter)
        {
            navigationViewModel.NotificationQueue.Enqueue("This button doesn't do anything yet.  :(");
        }

        public void OnCleanTempFilesCommand(object parameter)
        {
            bool filesCleared = gameStarter.CleanTempFiles();

            if (filesCleared)
            {
                navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.clean_temp_files_succeeded, true);
            }
            else
            {
                navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.clean_temp_files_failed, true);
            }
        }

        public void OnRemoveRegistryKeysCommand(object parameter)
        {
            bool regKeysRemoved = gameStarter.RemoveRegistryKeys();

            if (regKeysRemoved)
            {
                navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.remove_registry_keys_succeeded, true);
            }
            else
            {
                navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.remove_registry_keys_failed, true);
            }
        }

        public async Task OnClearGameSettingsCommand(object parameter)
        {

            bool BackupAndRemove(string backupFolderPath, FileInfo file)
            {
                file.Refresh();

                //if for some reason the file no longer exists /shrug
                if (!file.Exists)
                {
                    return false;
                }

                //create backup dir and copy file
                Directory.CreateDirectory(backupFolderPath);

                string newFilePath = Path.Combine(backupFolderPath, $"{file.Name}_{DateTime.Now.ToString("MM-dd-yyyy_hh-mm-ss-tt")}.bak");

                File.Copy(file.FullName, newFilePath);

                //copy check
                if (!File.Exists(newFilePath))
                {
                    return false;
                }

                //delete old file
                file.Delete();

                //delete check
                if (file.Exists)
                {
                    return false;
                }

                return true;
            }

            string EFTSettingsFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Escape from Tarkov");
            string backupFolderPath = Path.Combine(EFTSettingsFolder, "Backups");

            if (Directory.Exists(EFTSettingsFolder))
            {
                FileInfo local_ini = new FileInfo(Path.Combine(EFTSettingsFolder, "local.ini"));
                FileInfo shared_ini = new FileInfo(Path.Combine(EFTSettingsFolder, "shared.ini"));

                string Message = string.Format(LocalizationProvider.Instance.clear_game_settings_warning, backupFolderPath);
                ConfirmationDialog confirmDelete = new ConfirmationDialog(Message, LocalizationProvider.Instance.clear_game_settings, LocalizationProvider.Instance.cancel);

                var confirmation = await DialogHost.ShowDialog(confirmDelete);

                if (confirmation is bool proceed && !proceed)
                {
                    navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.clear_game_settings_failed, true);
                    return;
                }

                bool localSucceeded = BackupAndRemove(backupFolderPath, local_ini);
                bool sharedSucceeded = BackupAndRemove(backupFolderPath, shared_ini);

                //if one fails, I'm considering it bad. Send failed notification.
                if (!localSucceeded || !sharedSucceeded)
                {
                    navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.clear_game_settings_failed, true);
                    return;
                }
            }

            navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.clear_game_settings_succeeded, true);
        }

        public void OnSelectGameFolderCommand(object parameter)
        {
            string path = GetFolderPath();

            if (!String.IsNullOrEmpty(path))
            {
                LauncherSettingsProvider.Instance.GamePath = path;
            }
        }

        public void OnOpenGameFolderCommand(object parameter)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.EndsInDirectorySeparator(LauncherSettingsProvider.Instance.GamePath) ? LauncherSettingsProvider.Instance.GamePath : LauncherSettingsProvider.Instance.GamePath + Path.DirectorySeparatorChar,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        #endregion
    }
}
