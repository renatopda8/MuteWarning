using OBSWebsocketDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MuteWarning
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, bool> _isSourceMuted;

        private OBSWebsocket OBS { get; }
        private Dictionary<string, bool> IsSourceMuted
        {
            get => _isSourceMuted;
            set
            {
                _isSourceMuted = value ?? new Dictionary<string, bool>();
                CheckVisibility();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            OBS = new OBSWebsocket();
            IsSourceMuted = new Dictionary<string, bool>();

            Initialize();
        }

        private void Initialize()
        {
            SetVisible(false);

            OBS.SourceMuteStateChanged += SourceMuteStateChanged;
            OBS.Disconnected += Disconnected;

            MouseDown += Window_MouseDown;

            RunOnBackground(() =>
            {
                Connect(false);
            });
        }

        private void Disconnected(object sender, EventArgs e)
        {
            Disconnect(false);
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
            if (!IsSourceMuted.ContainsKey(sourceName))
            {
                IsSourceMuted.Add(sourceName, muted);
            }

            IsSourceMuted[sourceName] = muted;
            CheckVisibility();
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

        private bool IsConnected => OBS.IsConnected;

        private void CheckConnectionButtons()
        {
            Dispatcher.Invoke(() => ConnectMenuItem.IsEnabled = !(DisconnectMenuItem.IsEnabled = IsConnected));
        }

        private void CheckVisibility()
        {
            SetVisible(IsSourceMuted.Any(s => s.Value));
        }

        private void Connect(bool showMessages = true)
        {
            try
            {
                OBS.Connect("ws://127.0.0.1:4444", "?;(H_Qfwe8dqaf2k");

                if (!IsConnected)
                {
                    throw new Exception("Connection failed");
                }

                IsSourceMuted = OBS.GetSourcesList()
                    .Where(si => "input".Equals(si.Type) && ("wasapi_output_capture".Equals(si.TypeID) || "wasapi_input_capture".Equals(si.TypeID)))
                    .ToDictionary(si => si.Name, si => OBS.GetMute(si.Name));
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
                OBS.Disconnect();

                if (IsConnected)
                {
                    throw new Exception("Disconnection failed");
                }

                IsSourceMuted = new Dictionary<string, bool>();
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