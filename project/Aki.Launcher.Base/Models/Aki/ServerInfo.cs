/* ServerInfo.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * Merijn Hendriks
 */


namespace Aki.Launcher
{
    public class ServerInfo
    {
        public string backendUrl;
        public string name;
        public string[] editions;

        public ServerInfo()
        {
            backendUrl = "http://tarkov.lukehemmin.systems:6969";
            name = "Local H-Tarkov Server";
            editions = new string[0];
        }
    }
}
