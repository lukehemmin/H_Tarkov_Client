using Aki.Launcher;

namespace Aki.Launcher.Models.Aki
{
    public class AkiEdition
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public AkiEdition(string name) 
        {
            Name = name;
            ServerManager.SelectedServer.profileDescriptions.TryGetValue(name, out string desc);
            Description = desc;
        }
    }
}
