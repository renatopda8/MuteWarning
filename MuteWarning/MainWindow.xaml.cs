using OBSWebsocketDotNet;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MuteWarning
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected OBSWebsocket _obs;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            _obs = new OBSWebsocket();

            if (!_obs.IsConnected)
            {
                try
                {
                    _obs.Connect("ws://127.0.0.1:4444", "?;(H_Qfwe8dqaf2k");
                }
                catch (AuthFailureException)
                {
                    MessageBox.Show("Authentication failed.", "Error");
                    Application.Current.Shutdown();
                    return;
                }
                catch (ErrorResponseException ex)
                {
                    MessageBox.Show("Connect failed : " + ex.Message, "Error");
                    Application.Current.Shutdown();
                    return;
                }
            }

            bool isMuted = false;
            SetVisible(isMuted);

            _obs.SourceMuteStateChanged += SourceMuteStateChanged;

            MouseDown += Window_MouseDown;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void SourceMuteStateChanged(OBSWebsocket sender, string sourceName, bool muted)
        {
            Dispatcher.Invoke(() => SetVisible(muted));
        }

        private void SetVisible(bool isVisible)
        {
            this.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (_obs.IsConnected)
                {
                    _obs.Disconnect();
                }
            }
            finally
            {
                base.OnClosing(e);
            }
        }
    }
}