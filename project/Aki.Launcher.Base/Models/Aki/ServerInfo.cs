/* ServerInfo.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * Merijn Hendriks
 */


using System.Collections.Generic;

namespace Aki.Launcher
{
    public class ServerInfo
    {
        public string backendUrl;
        public string name;
        public string[] editions;
        public Dictionary<string, string> profileDescriptions;

        public ServerInfo()
        {
            backendUrl = "http://lukehemmin.com:6969";
            name = "Local SPT-AKI Server";
            editions = new string[0];
            profileDescriptions = new Dictionary<string, string>();
        }
    }
}
