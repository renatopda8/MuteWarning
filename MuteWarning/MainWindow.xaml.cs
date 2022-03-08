using OBSWebsocketDotNet;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace MuteWarning
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Settings _settings;

        private AudioSourcesControl SourcesControl { get; }
        private Settings SettingsWindow => _settings ??= new Settings();
        private OBSWebsocket OBS { get; }

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
                    ChangeWindowStyles(WindowStylesOperations.Set,
                        WindowHelper.ExtendedWindowStyles.WS_EX_TOOLWINDOW,
                        WindowHelper.ExtendedWindowStyles.WS_EX_TRANSPARENT);
                }
                else
                {
                    ChangeWindowStyles(WindowStylesOperations.Set,
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
                var operation = Configuration.Settings.IsIconLocked ? WindowStylesOperations.Remove : WindowStylesOperations.Set;
                ChangeWindowStyles(operation, WindowHelper.ExtendedWindowStyles.WS_EX_TRANSPARENT);
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
            Dispatcher.Invoke(() => LockIconMenuItem.IsEnabled = !(UnlockIconMenuItem.IsEnabled = Configuration.Settings.IsIconLocked));
        }

        private void OnSourceUpdated(bool isAnySourceMuted)
        {
            SetVisible(isAnySourceMuted);
        }

        private void Connect(bool showMessages = true)
        {
            try
            {
                if (IsConnected)
                {
                    return;
                }

                OBS.Connect(Configuration.Settings.ObsSocketUrl, Configuration.Settings.ObsSocketPassword);

                if (!IsConnected)
                {
                    throw new Exception("Connection failed");
                }

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

        private void Disconnect(bool showMessages = true)
        {
            try
            {
                if (!IsConnected)
                {
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

        #region Window styles

        public enum WindowStylesOperations
        {
            Set,
            Remove
        }

        private void ChangeWindowStyles(WindowStylesOperations operation, params WindowHelper.ExtendedWindowStyles[] styles)
        {
            if (styles?.Any() != true)
            {
                return;
            }

            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            int exStyle = (int) WindowHelper.GetWindowLong(wndHelper.Handle, (int) WindowHelper.GetWindowLongFields.GWL_EXSTYLE);

            foreach (var style in styles)
            {
                if (WindowStylesOperations.Set == operation)
                {
                    exStyle |= (int) style;
                }
                else
                {
                    exStyle &= ~(int) style;
                }
            }

            WindowHelper.SetWindowLong(wndHelper.Handle, (int) WindowHelper.GetWindowLongFields.GWL_EXSTYLE, (IntPtr) exStyle);
        }

        #endregion
    }
}