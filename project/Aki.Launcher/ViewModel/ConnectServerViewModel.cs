/* ConnectServerViewModel.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 * Merijn Hendriks
 */


using Aki.Launcher.Generics.AsyncCommand;
using Aki.Launcher.Helpers;
using Aki.Launcher.MiniCommon;
using Aki.Launcher.Models.Launcher;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Aki.Launcher.ViewModel
{
    public class ConnectServerViewModel
    {
        private bool UserLoggedOut { get; set; }
        public ConnectServerModel connectInfo { get; set; }
        public AwaitableDelegateCommand RetryAsyncCommand { get; set; }
        private NavigationViewModel navigationViewModel { get; set; }

        public ConnectServerViewModel(NavigationViewModel viewModel, bool logout = false)
        {
            navigationViewModel = viewModel;

            UserLoggedOut = logout;

            RetryAsyncCommand = new AwaitableDelegateCommand(OnRetryAsyncCommand);

            ConnectServerModel tmpConnectInfo = new ConnectServerModel();
            connectInfo = tmpConnectInfo;

            LauncherSettingsProvider.Instance.AllowSettings = true;

            _ = OnRetryAsyncCommand();
        }

        public async Task OnRetryAsyncCommand()
        {
            if (!LauncherSettingsProvider.Instance.AllowSettings)
            {
                return;
            }

            if (LauncherSettingsProvider.Instance.Server == null)
            {
                connectInfo.InfoText = LocalizationProvider.Instance.no_servers_available;
                return;
            }

            LauncherSettingsProvider.Instance.AllowSettings = false;
            connectInfo.InfoText = $"{LocalizationProvider.Instance.server_connecting} ...";

            ServerSetting DefaultServer = LauncherSettingsProvider.Instance.Server;

            if (DefaultServer == null)
            {
                return;
            }

            await ServerManager.LoadDefaultServerAsync(DefaultServer.Url);

            //This should only be the server we are loading from default. So it should be safe if the count is equal 1.
            if (ServerManager.SelectedServer != null)
            {
                RequestHandler.ChangeBackendUrl(ServerManager.SelectedServer.backendUrl);

                ImageRequest.CacheBackgroundImage();

                if (navigationViewModel.BackgroundImage == null)
                {
                    navigationViewModel.BackgroundImage = Path.Combine(ImageRequest.ImageCacheFolder, "bg.png");
                }

                if (DefaultServer.AutoLoginCreds == null || DefaultServer.AutoLoginCreds.Username == "")
                {
                    LauncherSettingsProvider.Instance.AllowSettings = true;

                    navigationViewModel.SelectedViewModel = new RegisterViewModel(navigationViewModel);
                }
                else
                {
                    if (LauncherSettingsProvider.Instance.UseAutoLogin && DefaultServer.AutoLoginCreds != null && !UserLoggedOut)
                    {
                        int status = await AccountManager.LoginAsync(DefaultServer.AutoLoginCreds);

                        if (status == 1)
                        {
                            LauncherSettingsProvider.Instance.AllowSettings = true;

                            navigationViewModel.SelectedViewModel = new ProfileViewModel(navigationViewModel);
                        }
                        else
                        {
                            LauncherSettingsProvider.Instance.AllowSettings = true;

                            navigationViewModel.SelectedViewModel = new LoginViewModel(navigationViewModel);
                        }
                    }
                    else
                    {
                        LauncherSettingsProvider.Instance.AllowSettings = true;

                        navigationViewModel.SelectedViewModel = new LoginViewModel(navigationViewModel);
                    }
                }
            }
            else
            {
                connectInfo.InfoText = String.Format(LocalizationProvider.Instance.server_unavailable_format_1, DefaultServer.Name);
            }

            UserLoggedOut = false;
            LauncherSettingsProvider.Instance.AllowSettings = true;
        }
    }
}
