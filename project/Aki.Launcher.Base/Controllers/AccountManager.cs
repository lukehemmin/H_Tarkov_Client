/* AccountManager.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 * Merijn Hendriks
 */


using Aki.Launcher.Helpers;
using Aki.Launcher.MiniCommon;
using Aki.Launcher.Models.Aki;
using Aki.Launcher.Models.Launcher;
using System.Threading.Tasks;

namespace Aki.Launcher
{
    public static class AccountManager
    {
        private const string STATUS_FAILED = "FAILED";
        private const string STATUS_OK = "OK";
        public static AccountInfo SelectedAccount { get; private set; } = null;
        public static ProfileInfo SelectedProfileInfo { get; private set; } = null;

        public static void Logout() => SelectedAccount = null;

        public static async Task<int> LoginAsync(LoginModel Creds)
        {
            return await Task.Run(() =>
            {
                return Login(Creds.Username, Creds.Password);
            });
        }

        public static async Task<int> LoginAsync(string username, string password)
        {
            return await Task.Run(() =>
            {
                return Login(username, password);
            });
        }

        public static int Login(string username, string password)
        {
            LoginRequestData data = new LoginRequestData(username, password);
            string id = STATUS_FAILED;
            string json = "";

            try
            {
                id = RequestHandler.RequestLogin(data);

                if (id == STATUS_FAILED)
                {
                    return -1;
                }

                json = RequestHandler.RequestAccount(data);
            }
            catch
            {
                return -2;
            }

            SelectedAccount = Json.Deserialize<AccountInfo>(json);
            RequestHandler.ChangeSession(SelectedAccount.id);

            UpdateProfileInfo();

            return 1;
        }

        public static void UpdateProfileInfo()
        {
            LoginRequestData data = new LoginRequestData(SelectedAccount.username, SelectedAccount.password);
            string profileInfoJson = RequestHandler.RequestProfileInfo(data);

            if (profileInfoJson != null)
            {
                ServerProfileInfo serverProfileInfo = Json.Deserialize<ServerProfileInfo>(profileInfoJson);
                SelectedProfileInfo = new ProfileInfo(serverProfileInfo);
            }
        }

        public static async Task<int> RegisterAsync(string username, string password, string edition)
        {
            return await Task.Run(() =>
            {
                return Register(username, password, edition);
            });
        }

        public static int Register(string username, string password, string edition)
        {
            RegisterRequestData data = new RegisterRequestData(username, password, edition);
            string registerStatus = STATUS_FAILED;

            try
            {
                registerStatus = RequestHandler.RequestRegister(data);

                if (registerStatus != STATUS_OK)
                {
                    return -1;
                }
            }
            catch
            {
                return -2;
            }

            int loginStatus = Login(username, password);

            if (loginStatus != 1)
            {
                switch (loginStatus)
                {
                    case -1:
                        return -3;

                    case -2:
                        return -2;
                }
            }

            return 1;
        }

        //only added incase wanted for future use.
        public static async Task<int> RemoveAsync()
        {
            return await Task.Run(() =>
            {
                return Remove();
            });
        }

        public static int Remove()
        {
            LoginRequestData data = new LoginRequestData(SelectedAccount.username, SelectedAccount.password);
            string json = STATUS_FAILED;

            try
            {
                json = RequestHandler.RequestAccount(data);

                if (json != STATUS_OK)
                {
                    return -1;
                }
            }
            catch
            {
                return -1;
            }

            SelectedAccount = null;


            // Left in for future, incase needed for reference
            //launcherConfig.Username = "";
            //launcherConfig.Password = "";
            //JsonHandler.SaveLauncherConfig(launcherConfig);
            return 1;
        }

        public static async Task<int> ChangeUsernameAsync(string username)
        {
            return await Task.Run(() =>
            {
                return ChangeUsername(username);
            });
        }

        public static int ChangeUsername(string username)
        {
            ChangeRequestData data = new ChangeRequestData(SelectedAccount.username, SelectedAccount.password, username);
            string json = STATUS_FAILED;

            try
            {
                json = RequestHandler.RequestChangeUsername(data);

                if (json != STATUS_OK)
                {
                    return -1;
                }
            }
            catch
            {
                return -2;
            }

            ServerSetting DefaultServer = LauncherSettingsProvider.Instance.Server;

            if (DefaultServer.AutoLoginCreds != null)
            {
                DefaultServer.AutoLoginCreds.Username = username;
            }

            SelectedAccount.username = username;
            LauncherSettingsProvider.Instance.SaveSettings();

            return 1;
        }

        public static async Task<int> ChangePasswordAsync(string password)
        {
            return await Task.Run(() =>
            {
                return ChangePassword(password);
            });
        }
        public static int ChangePassword(string password)
        {
            ChangeRequestData data = new ChangeRequestData(SelectedAccount.username, SelectedAccount.password, password);
            string json = STATUS_FAILED;

            try
            {
                json = RequestHandler.RequestChangePassword(data);

                if (json != STATUS_OK)
                {
                    return -1;
                }
            }
            catch
            {
                return -2;
            }

            ServerSetting DefaultServer = LauncherSettingsProvider.Instance.Server;

            if (DefaultServer.AutoLoginCreds != null)
            {
                DefaultServer.AutoLoginCreds.Password = password;
            }

            SelectedAccount.password = password;
            LauncherSettingsProvider.Instance.SaveSettings();

            return 1;
        }

        public static async Task<int> WipeAsync(string edition)
        {
            return await Task.Run(() =>
            {
                return Wipe(edition);
            });
        }
        public static int Wipe(string edition)
        {
            RegisterRequestData data = new RegisterRequestData(SelectedAccount.username, SelectedAccount.password, edition);
            string json = STATUS_FAILED;

            try
            {
                json = RequestHandler.RequestWipe(data);

                if (json != STATUS_OK)
                {
                    return -1;
                }
            }
            catch
            {
                return -2;
            }

            SelectedAccount.edition = edition;
            return 1;
        }
    }
}
