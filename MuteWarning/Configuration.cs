using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;

namespace MuteWarning
{
    public class Configuration
    {
        private static readonly Configuration _settings;

        private static string _appDataPath;
        private static string _settingsFilePath;

        private static string AppDataPath => _appDataPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(MuteWarning));
        private static string SettingsFilePath => _settingsFilePath ??= Path.Combine(AppDataPath, $"{nameof(Settings)}.json");

        static Configuration()
        {
            if (!Directory.Exists(AppDataPath))
            {
                throw new FileNotFoundException("The settings file could not be found.", SettingsFilePath);
            }
            
            string settingsJson = File.ReadAllText(SettingsFilePath);
            _settings = JsonConvert.DeserializeObject<Configuration>(settingsJson);
        }

        public static Configuration Settings => _settings;

        private Configuration()
        {
            //Nothing
        }

        public void Save()
        {
            string settingsJson = JsonConvert.SerializeObject(this);
            File.WriteAllText(SettingsFilePath, settingsJson);
        }

        /// <summary>
        /// URL de conexão do servidor do OBS WebSockets
        /// </summary>
        public string ObsSocketUrl { get; set; }

        /// <summary>
        /// Senha de conexão do servidor do OBS WebSockets
        /// </summary>
        public string ObsSocketPassword { get; set; }

        /// <summary>
        /// Posição do ícone na tela durante a última execução
        /// </summary>
        public Point? IconPosition { get; set; }
    }
}