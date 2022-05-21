using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MuteWarning
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public string IconImageBlackPath => "/Images/micMuted.png";
        public string IconImageWhitePath => "/Images/micMutedWhite.png";

        public bool IsIconImageBlackActive => IconImageBlackPath.Equals(Configuration.Settings.IconImagePath);
        public bool IsIconImageWhiteActive => IconImageWhitePath.Equals(Configuration.Settings.IconImagePath);

        public SettingsWindow()
        {
            InitializeComponent();
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
            Configuration.Settings.ObsSocketPassword = ObsSocketPasswordTextBox.Text?.Trim();
            Configuration.Settings.IconImagePath = IconImageWhiteRadioButton?.IsChecked ?? false ? IconImageWhitePath : IconImageBlackPath;
            Configuration.Settings.IsAutoConnectActive = IsAutoConnectActiveCheckBox.IsChecked ?? false;
            Configuration.Settings.AutoConnectIntervalInMinutes = int.TryParse(AutoConnectIntervalInMinutesTextBox.Text, out int result) ? result : 0;

            if (ResetIconPositionCheckBox.IsChecked == true)
            {
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

            Hide();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ObsSocketUrlTextBox.Text = Configuration.Settings.ObsSocketUrl;
            ObsSocketPasswordTextBox.Text = Configuration.Settings.ObsSocketPassword;
            IconImageWhiteRadioButton.IsChecked = !(IconImageBlackRadioButton.IsChecked = IconImageBlackPath.Equals(Configuration.Settings.IconImagePath));
            IsAutoConnectActiveCheckBox.IsChecked = Configuration.Settings.IsAutoConnectActive;
            AutoConnectIntervalInMinutesTextBox.Text = Configuration.Settings.AutoConnectIntervalInMinutes?.ToString();
            ResetIconPositionCheckBox.IsChecked = false;

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
    }
}