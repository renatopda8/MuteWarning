using OBSWebsocketDotNet;
using System;
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

            Connect();
            SetVisible(false);

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
            this.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (IsConnected)
                {
                    Disconnect();
                }
            }
            finally
            {
                base.OnClosing(e);
            }
        }

        private bool IsConnected => _obs.IsConnected;

        private void CheckConnectionButtons()
        {
            ConnectMenuItem.IsEnabled = !(DisconnectMenuItem.IsEnabled = _obs.IsConnected);
        }

        private void Connect()
        {
            try
            {
                _obs.Connect("ws://127.0.0.1:4444", "?;(H_Qfwe8dqaf2k");

                if (!IsConnected)
                {
                    throw new Exception("Connection failed");
                }

                CheckConnectionButtons();
            }
            catch (AuthFailureException)
            {
                MessageBox.Show("Authentication failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connect failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Disconnect()
        {
            try
            {
                _obs.Disconnect();
                
                if (IsConnected)
                {
                    throw new Exception("Disconnection failed");
                }

                CheckConnectionButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Disconnect failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}