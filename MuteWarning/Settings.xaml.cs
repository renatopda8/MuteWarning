using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MuteWarning
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public static Configuration Configuration => Configuration.Settings;

        public Settings()
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
            Configuration.ObsSocketUrl = ObsSocketUrlTextBox.Text;
            Configuration.ObsSocketPassword = ObsSocketPasswordTextBox.Text;

            Hide();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ObsSocketUrlTextBox.Text = Configuration.ObsSocketUrl;
            ObsSocketPasswordTextBox.Text = Configuration.ObsSocketPassword;

            Hide();
        }
    }
}