/* ProfileViewModel.cs
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
using Aki.Launcher.MiniCommon;
using Aki.Launcher.Models.Launcher;
using System.Threading.Tasks;
using System.Windows;

namespace Aki.Launcher.ViewModel
{
    public class ProfileViewModel
    {
        public string CurrentUsername { get; set; }
        public string CurrentEdition { get; set; }
        public string CurrentID { get; set; }
        public GenericICommand LogoutCommand { get; set; }
        public ProfileInfo ProfileInfo { get; set; }
        public AwaitableDelegateCommand StartGameCommand { get; set; }
        private NavigationViewModel navigationViewModel { get; set; }
        private GameStarter gameStarter = new GameStarter(new GameStarterFrontend());

        private ProcessMonitor monitor { get; set; }
        public ProfileViewModel(NavigationViewModel viewModel)
        {
            navigationViewModel = viewModel;
            LogoutCommand = new GenericICommand(OnLogoutCommand);
            StartGameCommand = new AwaitableDelegateCommand(OnStartGameCommand);

            monitor = new ProcessMonitor("EscapeFromTarkov", 1000, aliveCallback: GameAliveCallBack, exitCallback: GameExitCallback);

            CurrentUsername = AccountManager.SelectedAccount.username;
            CurrentEdition = AccountManager.SelectedAccount.edition;
            CurrentID = AccountManager.SelectedAccount.id;

            if (AccountManager.SelectedProfileInfo != null && AccountManager.SelectedProfileInfo.Side != null)
            {
                ImageRequest.CacheSideImage(AccountManager.SelectedProfileInfo.Side);
            }

            ProfileInfo tempProfileInfo = AccountManager.SelectedProfileInfo;

            ProfileInfo = tempProfileInfo;
        }

        public void OnLogoutCommand(object parameter)
        {
            AccountManager.Logout();
            navigationViewModel.SelectedViewModel = new ConnectServerViewModel(navigationViewModel, true);
        }

        public async Task OnStartGameCommand()
        {
            LauncherSettingsProvider.Instance.AllowSettings = false;

            int status = await AccountManager.LoginAsync(AccountManager.SelectedAccount.username, AccountManager.SelectedAccount.password);

            LauncherSettingsProvider.Instance.AllowSettings = true;

            switch (status)
            {
                case -1:
                    navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.incorrect_login);
                    return;

                case -2:
                    navigationViewModel.NotificationQueue.Enqueue(LocalizationProvider.Instance.login_failed);
                    navigationViewModel.SelectedViewModel = new ConnectServerViewModel(navigationViewModel);
                    return;
            }


            LauncherSettingsProvider.Instance.GameRunning = true;

            GameStarterResult gameStartResult = await gameStarter.LaunchGame(ServerManager.SelectedServer, AccountManager.SelectedAccount);

            if (gameStartResult.Succeeded)
            {
                monitor.Start();

                switch (LauncherSettingsProvider.Instance.LauncherStartGameAction)
                {
                    case LauncherAction.MinimizeAction:
                        {
                            Application.Current.MainWindow.WindowState = WindowState.Minimized;
                            break;
                        }
                    case LauncherAction.ExitAction:
                        {
                            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                            Application.Current.MainWindow.Close();
                            break;
                        }
                }
            }
            else
            {
                navigationViewModel.NotificationQueue.Enqueue(gameStartResult.Message);
                LauncherSettingsProvider.Instance.GameRunning = false;
            }
        }

        private void UpdateProfileInfo()
        {
            AccountManager.UpdateProfileInfo();
            ImageRequest.CacheSideImage(AccountManager.SelectedProfileInfo.Side);
            ProfileInfo.UpdateDisplayedProfile(AccountManager.SelectedProfileInfo);
        }


        //pull profile every x seconds
        private int aliveCallBackCountdown = 60;
        private void GameAliveCallBack(ProcessMonitor monitor)
        {
            aliveCallBackCountdown--;

            if (aliveCallBackCountdown <= 0)
            {
                aliveCallBackCountdown = 60;
                UpdateProfileInfo();
            }
        }

        private void GameExitCallback(ProcessMonitor monitor)
        {
            monitor.Stop();

            LauncherSettingsProvider.Instance.GameRunning = false;

            //Make sure the call to MainWindow happens on the UI thread.
            switch (LauncherSettingsProvider.Instance.LauncherStartGameAction)
            {
                case LauncherAction.MinimizeAction:
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Application.Current.MainWindow.WindowState = WindowState.Normal;
                        });

                        break;
                    }
            }

            UpdateProfileInfo();
        }
    }
}
