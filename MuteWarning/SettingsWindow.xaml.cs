using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MuteWarning
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public static Configuration Settings => Configuration.Settings;

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
            Settings.ObsSocketUrl = ObsSocketUrlTextBox.Text;
            Settings.ObsSocketPassword = ObsSocketPasswordTextBox.Text;

            Hide();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ObsSocketUrlTextBox.Text = Settings.ObsSocketUrl;
            ObsSocketPasswordTextBox.Text = Settings.ObsSocketPassword;

            Hide();
        }
    }
}