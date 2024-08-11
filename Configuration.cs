using System.IO;
using Dalamud.Configuration;
using Newtonsoft.Json;

namespace WinTitle
{
    public class Configuration : IPluginConfiguration
    {
        public bool SetTitleToLoggedCharacter = false;
        public int Version { get; set; } = 1;

        public static Configuration Load()
        {
            return WinTitle.PluginInterface.ConfigFile.Exists
                ? JsonConvert.DeserializeObject<Configuration>(
                    File.ReadAllText(WinTitle.PluginInterface.ConfigFile.FullName)) ?? new Configuration()
                : new Configuration();
        }

        public void Save()
        {
            File.WriteAllText(WinTitle.PluginInterface.ConfigFile.FullName, JsonConvert.SerializeObject(this));
        }
    }
}