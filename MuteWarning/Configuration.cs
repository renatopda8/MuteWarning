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
        private bool _isIconLocked;

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
            set
            {
                _iconPosition = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Define se o ícone está fixado na posição atual e permitindo cliques através da imagem
        /// </summary>
        public bool IsIconLocked
        {
            get { return _isIconLocked; }
            set
            {
                _isIconLocked = value;
                OnPropertyChanged();
            }
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