/* WipeProfileViewModel.cs
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
using Aki.Launcher.Models.Launcher;
using System.Threading.Tasks;

namespace Aki.Launcher.ViewModel
{
    public class WipeProfileViewModel
    {
        public AwaitableDelegateCommand WipeCommand { get; set; }
        public GenericICommand BackCommand { get; set; }
        public WipeProfileModel ProfileWipe { get; set; }
        private NavigationViewModel navigationViewModel { get; set; }

        public WipeProfileViewModel(NavigationViewModel viewModel)
        {
            navigationViewModel = viewModel;
            WipeCommand = new AwaitableDelegateCommand(OnWipeCommand);
            BackCommand = new GenericICommand(OnBackCommand);

            WipeProfileModel tmpWipeProfile = new WipeProfileModel();

            ProfileWipe = tmpWipeProfile;

            //LauncherSettingsProvider.Instance.AllowSettings = true;
        }

        public void OnBackCommand(object parameter)
        {
            navigationViewModel.SelectedViewModel = new EditProfileViewModel(navigationViewModel);
        }

        public async Task OnWipeCommand()
        {
            LauncherSettingsProvider.Instance.AllowSettings = false;


            LauncherSettingsProvider.Instance.AllowSettings = true;

            int status = await AccountManager.WipeAsync(ProfileWipe.EditionsCollection.SelectedEdition);

            switch (status)
            {
                case 1:
                    navigationViewModel.SelectedViewModel = new ProfileViewModel(navigationViewModel);
                    break;

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
