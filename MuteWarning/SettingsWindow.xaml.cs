﻿using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace MuteWarning
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private bool _startWithWindow;
        private const string _startupRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string _startupApprovedRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

        public bool IsIconImageBlackActive => Configuration.IconImageBlackPath.Equals(Configuration.Settings.IconImagePath);

        public bool IsIconImageWhiteActive => Configuration.IconImageWhitePath.Equals(Configuration.Settings.IconImagePath);

        public bool StartWithWindow
        {
            get { return _startWithWindow; }
            set
            {
                _startWithWindow = value;
                OnPropertyChanged();
            }
        }

        public SettingsWindow()
        {
            InitializeComponent();
            GetStartWithWindows();
        }

        private void GetStartWithWindows()
        {
            using var startupKey = Registry.CurrentUser.OpenSubKey(_startupRegistryKey, true);
            object value = startupKey.GetValue(nameof(MuteWarning));
            if (value == null)
            {
                return;
            }

            using var startupApprovedKey = Registry.CurrentUser.OpenSubKey(_startupApprovedRegistryKey, true);
            StartWithWindow = startupApprovedKey.GetValue(nameof(MuteWarning)) is not byte[] binaryValue || binaryValue[0] == 2;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Configuration.Settings.ObsSocketUrl = ObsSocketUrlTextBox.Text?.Trim();
            Configuration.Settings.ObsSocketPassword = ObsSocketPasswordPasswordBox.Password?.Trim();
            Configuration.Settings.IconImagePath = IconImageWhiteRadioButton?.IsChecked ?? false ? Configuration.IconImageWhitePath : Configuration.IconImageBlackPath;
            Configuration.Settings.IsAutoConnectActive = IsAutoConnectActiveCheckBox.IsChecked ?? false;
            Configuration.Settings.AutoConnectIntervalInMinutes = int.TryParse(AutoConnectIntervalInMinutesTextBox.Text, out int result) ? result : 0;

            CheckResetIconPositionChecked();
            CheckStartWithWindowsChanged();

            Hide();
        }

        private void CheckResetIconPositionChecked()
        {
            if (ResetIconPositionCheckBox.IsChecked != true)
            {
                return;
            }

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = Application.Current.MainWindow.Width;
            double windowHeight = Application.Current.MainWindow.Height;

            Point centerPoint = new((screenWidth / 2) - (windowWidth / 2), (screenHeight / 2) - (windowHeight / 2));
            Configuration.Settings.IconPosition = centerPoint;
            Application.Current.MainWindow.Left = centerPoint.X;
            Application.Current.MainWindow.Top = centerPoint.Y;

            ResetIconPositionCheckBox.IsChecked = false;
        }

        private void CheckStartWithWindowsChanged()
        {
            if (StartWithWindowsCheckBox.IsChecked == StartWithWindow)
            {
                return;
            }

            using var startupKey = Registry.CurrentUser.OpenSubKey(_startupRegistryKey, true);
            using var startupApprovedKey = Registry.CurrentUser.OpenSubKey(_startupApprovedRegistryKey, true);

            if (StartWithWindowsCheckBox.IsChecked != true)
            {
                startupKey.DeleteValue(nameof(MuteWarning));
                startupApprovedKey.DeleteValue(nameof(MuteWarning), false);
                StartWithWindow = false;

                return;
            }

            Assembly curAssembly = Assembly.GetExecutingAssembly();
            string location = Path.ChangeExtension(curAssembly.Location, "exe");
            startupKey.SetValue(nameof(MuteWarning), location);

            if (startupApprovedKey.GetValue(nameof(MuteWarning)) is byte[] binaryValue && binaryValue[0] != 2)
            {
                binaryValue[0] = 2;
                startupApprovedKey.SetValue(nameof(MuteWarning), binaryValue);
            }

            StartWithWindow = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ObsSocketUrlTextBox.Text = Configuration.Settings.ObsSocketUrl;
            ObsSocketPasswordPasswordBox.Password = Configuration.Settings.ObsSocketPassword;
            IconImageWhiteRadioButton.IsChecked = !(IconImageBlackRadioButton.IsChecked = Configuration.IconImageBlackPath.Equals(Configuration.Settings.IconImagePath));
            IsAutoConnectActiveCheckBox.IsChecked = Configuration.Settings.IsAutoConnectActive;
            AutoConnectIntervalInMinutesTextBox.Text = Configuration.Settings.AutoConnectIntervalInMinutes?.ToString();
            ResetIconPositionCheckBox.IsChecked = false;
            StartWithWindowsCheckBox.IsChecked = StartWithWindow;

            Hide();
        }

        private void AutoConnectIntervalInMinutesTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.All(char.IsDigit))
            {
                return;
            }

            e.Handled = true;
        }

        private void AutoConnectIntervalInMinutesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox) sender;
            if (tb.Text.All(char.IsDigit))
            {
                return;
            }

            tb.Text = string.Concat(tb.Text.Where(char.IsDigit));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ObsSocketPasswordPasswordBox.Password = Configuration.Settings.ObsSocketPassword;
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