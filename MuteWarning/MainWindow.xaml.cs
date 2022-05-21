using FluentScheduler;
using OBSWebsocketDotNet;
using System;
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
        private readonly object _connectionLock = new();
        private SettingsWindow _settings;

        private OBSWebsocket OBS { get; }
        private AudioSourcesControl SourcesControl { get; }
        private SettingsWindow SettingsWindow => _settings ??= new SettingsWindow();

        public MainWindow()
        {
            InitializeComponent();

            OBS = new OBSWebsocket();
            SourcesControl = new AudioSourcesControl(OnSourceUpdated);

            Initialize();
        }

        private void Initialize()
        {
            SetVisible(false);

            OBS.SourceMuteStateChanged += SourceMuteStateChanged;
            OBS.Disconnected += Disconnected;

            if (Configuration.Settings.IconPosition.HasValue)
            {
                Left = Configuration.Settings.IconPosition.Value.X;
                Top = Configuration.Settings.IconPosition.Value.Y;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            
            StartAutoConnect(true);
        }

        private void StartAutoConnect(bool runNow = false)
        {
            if (!Configuration.Settings.IsAutoConnectActive || IsConnected)
            {
                return;
            }

            //Auto connect já agendado
            if (JobManager.AllSchedules.Any())
            {
                return;
            }

            var intervalInMinutes = Configuration.Settings.AutoConnectIntervalInMinutes;
            if (!intervalInMinutes.HasValue || intervalInMinutes.Value < 1)
            {
                return;
            }

            TimeUnit runFunction(Schedule s, int m) =>
                runNow ? s.ToRunNow().AndEvery(m) : s.ToRunEvery(m);

            JobManager.AddJob(
                () => Connect(false),
                s => runFunction(s.NonReentrant(), intervalInMinutes.Value).Seconds()
            );
        }

        private void StopAutoConnect()
        {
            if (!Configuration.Settings.IsAutoConnectActive)
            {
                return;
            }

            //Auto connect não agendado
            if (!JobManager.AllSchedules.Any())
            {
                return;
            }

            JobManager.Stop();
            JobManager.RemoveAllJobs();
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

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Configuration.Settings.IconPosition = new Point(Left, Top);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Configuration.Settings.IsIconLocked)
                {
                    WindowHelper.ChangeWindowStyles(this, WindowHelper.WindowStylesOperations.Set,
                        WindowHelper.ExtendedWindowStyles.WS_EX_TOOLWINDOW,
                        WindowHelper.ExtendedWindowStyles.WS_EX_TRANSPARENT);
                }
                else
                {
                    WindowHelper.ChangeWindowStyles(this, WindowHelper.WindowStylesOperations.Set,
                        WindowHelper.ExtendedWindowStyles.WS_EX_TOOLWINDOW);
                }
            }
            finally
            {
                CheckIconLockButtons();
            }
        }

        private void ChangeIconLockState()
        {
            try
            {
                var operation = Configuration.Settings.IsIconLocked ? WindowHelper.WindowStylesOperations.Remove : WindowHelper.WindowStylesOperations.Set;
                WindowHelper.ChangeWindowStyles(this, operation, WindowHelper.ExtendedWindowStyles.WS_EX_TRANSPARENT);
                Configuration.Settings.IsIconLocked = !Configuration.Settings.IsIconLocked;
            }
            finally
            {
                CheckIconLockButtons();
            }
        }

        private void SourceMuteStateChanged(OBSWebsocket sender, string sourceName, bool muted)
        {
            SourcesControl.UpdateSource(sourceName, muted);
        }

        private void SetVisible(bool isVisible)
        {
            Dispatcher.Invoke(() =>
            {
                Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
                CheckIconLockButtons();
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (IsConnected)
                {
                    Disconnect(false);
                }

                Configuration.Settings.Save();
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

        private void CheckIconLockButtons()
        {
            Dispatcher.Invoke(() =>
            {
                LockIconMenuItem.IsEnabled = IsVisible && !Configuration.Settings.IsIconLocked;
                UnlockIconMenuItem.IsEnabled = IsVisible && Configuration.Settings.IsIconLocked;
            });
        }

        private void OnSourceUpdated(bool isAnySourceMuted)
        {
            SetVisible(isAnySourceMuted);
        }

        private void Connect(bool showMessages = true)
        {
            lock (_connectionLock)
            {
                try
                {
                    if (IsConnected)
                    {
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(Configuration.Settings.ObsSocketUrl) || string.IsNullOrWhiteSpace(Configuration.Settings.ObsSocketPassword))
                    {
                        throw new Exception("Connection failed");
                    }

                    OBS.Connect(Configuration.Settings.ObsSocketUrl, Configuration.Settings.ObsSocketPassword);

                    if (!IsConnected)
                    {
                        throw new Exception("Connection failed");
                    }

                    StopAutoConnect();
                    SourcesControl.SetSources(
                        OBS.GetSourcesList()
                            .Where(si => "input".Equals(si.Type) && ("wasapi_output_capture".Equals(si.TypeID) || "wasapi_input_capture".Equals(si.TypeID)))
                            .Select(si => new AudioSource(si.Name, OBS.GetMute(si.Name)))
                            .ToArray()
                    );
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
        }

        private void Disconnect(bool showMessages = true)
        {
            lock (_connectionLock)
            {
                try
                {
                    if (!IsConnected)
                    {
                        SourcesControl.ClearSources();
                        return;
                    }

                    OBS.Disconnect();

                    if (IsConnected)
                    {
                        throw new Exception("Disconnection failed");
                    }

                    SourcesControl.ClearSources();
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
                    StartAutoConnect();
                }
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

        private void LockIcon_Click(object sender, RoutedEventArgs e)
        {
            ChangeIconLockState();
        }

        private void UnlockIcon_Click(object sender, RoutedEventArgs e)
        {
            ChangeIconLockState();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow.Show();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Exit();
        }
    }
}