using OBSWebsocketDotNet;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
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
        private AudioSourcesControl SourcesControl { get; }

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int) GetWindowLong(wndHelper.Handle, (int) GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int) ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int) GetWindowLongFields.GWL_EXSTYLE, (IntPtr) exStyle);
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

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Exit();
        }

        #region Window styles

        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int) intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);

        #endregion
    }
}