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

            SetVisible(false);

            _obs.SourceMuteStateChanged += SourceMuteStateChanged;

            MouseDown += Window_MouseDown;

            RunOnBackground(() =>
            {
                Connect(false);
            });
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
            SetVisible(muted);
        }

        private void SetVisible(bool isVisible)
        {
            Dispatcher.Invoke(() => this.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (IsConnected)
                {
                    Disconnect(false);
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
            Dispatcher.Invoke(() => ConnectMenuItem.IsEnabled = !(DisconnectMenuItem.IsEnabled = IsConnected));
        }

        private void Connect(bool showMessages = true)
        {
            try
            {
                _obs.Connect("ws://127.0.0.1:4444", "?;(H_Qfwe8dqaf2k");

                if (!IsConnected)
                {
                    throw new Exception("Connection failed");
                }
            }
            catch (AuthFailureException)
            {
                if (showMessages)
                {
                    MessageBox.Show("Authentication failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }                
            }
            catch (Exception ex)
            {
                if (showMessages)
                {
                    MessageBox.Show($"Connect failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                CheckConnectionButtons();
            }
        }

        private void Disconnect(bool showMessages = true)
        {
            try
            {
                _obs.Disconnect();

                if (IsConnected)
                {
                    throw new Exception("Disconnection failed");
                }
            }
            catch (Exception ex)
            {
                if (showMessages)
                {
                    MessageBox.Show($"Disconnect failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                CheckConnectionButtons();
            }
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        private void RunOnBackground(Action action)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (object sender, DoWorkEventArgs e) =>
            {
                action?.Invoke();
            };

            bw.RunWorkerAsync();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            RunOnBackground(() => Connect());
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            RunOnBackground(() => Disconnect());
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Exit();
        }
    }
}