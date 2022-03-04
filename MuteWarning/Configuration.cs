using Newtonsoft.Json;
using System;
using System.IO;

namespace MuteWarning
{
    public class Configuration
    {
        private static Configuration _settings;
        private static string _appDataPath;

        static Configuration()
        {
            string settingsJsonPath = Path.Combine(AppDataPath, $"{nameof(Settings)}.json");
            if (!Directory.Exists(AppDataPath))
            {
                throw new FileNotFoundException("The settings file could not be found.", settingsJsonPath);
            }
            
            string settingsJson = File.ReadAllText(settingsJsonPath);
            _settings = JsonConvert.DeserializeObject<Configuration>(settingsJson);
        }

        private Configuration()
        {
            //Nothing
        }

        private static string AppDataPath => _appDataPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(MuteWarning));

        public static Configuration Settings => _settings;

        /// <summary>
        /// URL de conexão do servidor do OBS WebSockets
        /// </summary>
        public string ObsSocketUrl { get; set; }

        /// <summary>
        /// Senha de conexão do servidor do OBS WebSockets
        /// </summary>
        public string ObsSocketPassword { get; set; }
    }
}