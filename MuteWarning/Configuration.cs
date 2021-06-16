using System.Configuration;

namespace MuteWarning
{
    public class Configuration
    {
        private static Configuration _settings;

        private Configuration()
        {
            ObsSocketUrl = ConfigurationManager.AppSettings[nameof(ObsSocketUrl)];
            ObsSocketPassword = ConfigurationManager.AppSettings[nameof(ObsSocketPassword)];
        }

        public static Configuration Settings => _settings ??= new Configuration();

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