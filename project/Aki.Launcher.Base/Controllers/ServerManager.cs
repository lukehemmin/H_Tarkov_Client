/* ServerManager.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * Merijn Hendriks
 */


using Aki.Launcher.MiniCommon;
using System.Threading.Tasks;

namespace Aki.Launcher
{
    public static class ServerManager
    {
        public static ServerInfo SelectedServer { get; private set; } = null;

        public static void LoadServer(string backendUrl)
        {
            string json = "";

            try
            {
                RequestHandler.ChangeBackendUrl(backendUrl);
                json = RequestHandler.RequestConnect();
            }
            catch
            {
                SelectedServer = null;
                return;
            }

            SelectedServer = Json.Deserialize<ServerInfo>(json);
        }

        public static async Task LoadDefaultServerAsync(string server)
        {
            await Task.Run(() =>
            {
                LoadServer(server);
            });
        }
    }
}
