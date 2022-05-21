using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MuteWarning
{
    public class Configuration : INotifyPropertyChanged
    {
        private static readonly Configuration _settings;

        private string _obsSocketUrl;
        private string _obsSocketPassword;
        private Point? _iconPosition;
        private string _iconImagePath;
        private bool _isIconLocked;
        private bool _isAutoConnectActive;
        private int? _autoConnectIntervalInMinutes;

        private static string _appDataPath;
        private static string _settingsFilePath;

        private static string AppDataPath => _appDataPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(MuteWarning));
        private static string SettingsFilePath => _settingsFilePath ??= Path.Combine(AppDataPath, $"{nameof(Settings)}.json");

        public static string IconImageBlackPath => "/Images/micMuted.png";
        public static string IconImageWhitePath => "/Images/micMutedWhite.png";

        static Configuration()
        {
            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }

            if (!File.Exists(SettingsFilePath))
            {
                _settings = new Configuration
                {
                    IconImagePath = IconImageBlackPath,
                    IsAutoConnectActive = true,
                    AutoConnectIntervalInMinutes = 5
                };
                _settings.Save();

                return;
            }

            string settingsJson = File.ReadAllText(SettingsFilePath);
            _settings = JsonConvert.DeserializeObject<Configuration>(settingsJson);

            if (string.IsNullOrWhiteSpace(_settings.IconImagePath))
            {
                _settings.IconImagePath = IconImageBlackPath;
            }
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
        public string ObsSocketUrl
        {
            get { return _obsSocketUrl; }
            set
            {
                _obsSocketUrl = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Senha de conexão do servidor do OBS WebSockets
        /// </summary>
        public string ObsSocketPassword
        {
            get { return _obsSocketPassword; }
            set
            {
                _obsSocketPassword = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Posição do ícone na tela durante a última execução
        /// </summary>
        public Point? IconPosition
        {
            get { return _iconPosition; }
            set { _iconPosition = value; }
        }

        /// <summary>
        /// Caminho do arquivo da imagem de aviso
        /// </summary>
        public string IconImagePath
        {
            get { return _iconImagePath; }
            set
            {
                _iconImagePath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Define se o ícone está fixado na posição atual e permitindo cliques através da imagem
        /// </summary>
        public bool IsIconLocked
        {
            get { return _isIconLocked; }
            set { _isIconLocked = value; }
        }

        /// <summary>
        /// Define se a função de conexão automática está ativa
        /// </summary>
        public bool IsAutoConnectActive
        {
            get { return _isAutoConnectActive; }
            set { _isAutoConnectActive = value; }
        }

        /// <summary>
        /// Define o intervalo em minutos para execução da tentativa de conexão automática com o OBS
        /// </summary>
        public int? AutoConnectIntervalInMinutes
        {
            get { return _autoConnectIntervalInMinutes; }
            set { _autoConnectIntervalInMinutes = value; }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        #endregion
    }
}