/* EditProfileViewModel.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 * Merijn Hendriks
 */


using Aki.Launcher.Custom_Controls;
using Aki.Launcher.Custom_Controls.Dialogs;
using Aki.Launcher.Generics.AsyncCommand;
using Aki.Launcher.Helpers;
using Aki.Launcher.Interfaces;
using Aki.Launcher.Models.Launcher;
using System;
using System.Threading.Tasks;

namespace Aki.Launcher.ViewModel
{
    public class EditProfileViewModel
    {
        public LoginModel login { get; set; }

        public AwaitableDelegateCommand UpdateCommand { get; set; }
        public WipeProfileModel ProfileWipe { get; set; }
        public AwaitableDelegateCommand WipeProfileCommand { get; set; }

        private NavigationViewModel navigationViewModel { get; set; }
        public EditProfileViewModel(NavigationViewModel viewModel)
        {
            navigationViewModel = viewModel;

            ServerManager.LoadServer(LauncherSettingsProvider.Instance.Server.Url);

            UpdateCommand = new AwaitableDelegateCommand(OnUpdateCommand);
            WipeProfileCommand = new AwaitableDelegateCommand(OnWipeProfileCommand);

            ServerSetting DefaultServer = LauncherSettingsProvider.Instance.Server;

            LoginModel tmpLogin = new LoginModel();
            tmpLogin.Username = DefaultServer.AutoLoginCreds.Username;
            tmpLogin.Password = DefaultServer.AutoLoginCreds.Password;

            WipeProfileModel tmpWipeProfile = new WipeProfileModel();
            tmpWipeProfile.EditionsCollection.SelectedEdition = AccountManager.SelectedAccount.edition;

            tmpWipeProfile.EditionsCollection.SelectedEditionIndex = tmpWipeProfile.EditionsCollection.AvailableEditions.IndexOf(AccountManager.SelectedAccount.edition);
            ProfileWipe = tmpWipeProfile;

            login = tmpLogin;
        }

        private string GetStatus(int status)
        {
            switch (status)
            {
                case 1:
                    return "OK";

                case -1:

                    return "Login failed";

                case -2:
                    return "CONNECTION_ERROR";
            }

            return "Undefined Response";
        }

        public async Task OnUpdateCommand(object parameter)
        {
            if (parameter is IHavePassword pass)
            {
                if (!string.IsNullOrWhiteSpace(pass.Password))
                {
                    login.Password = pass.Password;
                }
            }
            else
            {
                navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.edit_account_update_error);
                navigationViewModel.SelectedViewModel = new ProfileViewModel(navigationViewModel);
            }

            LauncherSettingsProvider.Instance.AllowSettings = false;


            if (ProfileWipe.EditionsCollection.SelectedEdition != AccountManager.SelectedAccount.edition)
            {
                ConfirmationDialog confirmDialog = new ConfirmationDialog(
                    String.Format(LocalizationProvider.Instance.wipe_warning_format_2, AccountManager.SelectedAccount.edition, ProfileWipe.EditionsCollection.SelectedEdition),
                    LocalizationProvider.Instance.wipe_profile,
                    LocalizationProvider.Instance.cancel);

                var confirmWipe = await DialogHost.ShowDialog(confirmDialog);

                if (confirmWipe is bool confirmation && confirmation == false)
                {
                    navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.edit_account_update_error, true);
                    LauncherSettingsProvider.Instance.AllowSettings = true;
                    return;
                }

                await OnWipeProfileCommand(true);
            }

            string usernameStatus = GetStatus(await AccountManager.ChangeUsernameAsync(login.Username));
            string passStatus = GetStatus(await AccountManager.ChangePasswordAsync(login.Password));

            LauncherSettingsProvider.Instance.AllowSettings = true;

            if (usernameStatus == "OK" && passStatus == "OK")
            {
                navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.account_updated);
            }
            else
            {
                navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.edit_account_update_error);
                navigationViewModel.SelectedViewModel = new ConnectServerViewModel(navigationViewModel);
            }
        }

        public async Task OnWipeProfileCommand(object parameter = null)
        {
            bool bypassCheck;

            if (!bool.TryParse(parameter?.ToString(), out bypassCheck))
            {
                bypassCheck = false;
            }

            if (!bypassCheck)
            {
                ConfirmationDialog confirmDialog = new ConfirmationDialog(
                    LocalizationProvider.Instance.wipe_warning,
                    LocalizationProvider.Instance.wipe_profile,
                    LocalizationProvider.Instance.cancel);

                var confirmWipe = await DialogHost.ShowDialog(confirmDialog);

                if (confirmWipe is bool confirmation && confirmation == false)
                {
                    navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.edit_account_update_error, true);
                    LauncherSettingsProvider.Instance.AllowSettings = true;
                    return;
                }
            }

            int status = await AccountManager.WipeAsync(ProfileWipe.EditionsCollection.SelectedEdition);

            switch (status)
            {
                case 1:
                    navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.account_updated);
                    return;

                case -1:
                    navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.login_failed);
                    navigationViewModel.SelectedViewModel = new ConnectServerViewModel(navigationViewModel);
                    return;

                case -2:
                    navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.edit_account_update_error);
                    navigationViewModel.SelectedViewModel = new ConnectServerViewModel(navigationViewModel);
                    return;
            }
        }
    }
}
