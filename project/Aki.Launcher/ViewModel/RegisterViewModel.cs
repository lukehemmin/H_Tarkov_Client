/* RegisterViewModel.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 * Merijn Hendriks
 */


using Aki.Launcher.Generics;
using Aki.Launcher.Generics.AsyncCommand;
using Aki.Launcher.Helpers;
using Aki.Launcher.Interfaces;
using Aki.Launcher.Models.Launcher;
using System.Threading.Tasks;

namespace Aki.Launcher.ViewModel
{
    public class RegisterViewModel
    {
        public RegisterModel newProfile { get; set; }
        public GenericICommand ShowLoginCommand { get; set; }
        public AwaitableDelegateCommand RegisterCommand { get; set; }
        private NavigationViewModel navigationViewModel { get; set; }

        public RegisterViewModel(NavigationViewModel viewModel)
        {
            navigationViewModel = viewModel;

            ShowLoginCommand = new GenericICommand(OnShowLoginCommand);
            RegisterCommand = new AwaitableDelegateCommand(OnRegisterCommand);

            RegisterModel tmpProfile = new RegisterModel();

            newProfile = tmpProfile;
        }

        public void OnShowLoginCommand(object parameter)
        {
            navigationViewModel.SelectedViewModel = new LoginViewModel(navigationViewModel);
        }

        public async Task OnRegisterCommand(object parameter)
        {
            if (parameter is IHavePassword pass)
            {
                newProfile.Password = pass.Password;
            }
            else
            {
                navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.registration_failed);
                return;
            }

            LauncherSettingsProvider.Instance.AllowSettings = false;

            int status = await AccountManager.RegisterAsync(newProfile.Username ?? "", newProfile.Password ?? "", newProfile.EditionsCollection.SelectedEdition);

            LauncherSettingsProvider.Instance.AllowSettings = true;


            switch (status)
            {
                case 1:
                    ServerSetting DefaultServer = LauncherSettingsProvider.Instance.Server;

                    DefaultServer.AutoLoginCreds = new LoginModel { Username = newProfile.Username, Password = newProfile.Password };
                    LauncherSettingsProvider.Instance.SaveSettings();


                    navigationViewModel.SelectedViewModel = new ProfileViewModel(navigationViewModel);
                    break;

                case -1:
                    navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.account_exist);
                    return;

                case -2:
                    navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.registration_failed);
                    navigationViewModel.SelectedViewModel = new ConnectServerViewModel(navigationViewModel);
                    return;

                case -3:
                    navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.incorrect_login);
                    return;
            }
        }
    }
}
