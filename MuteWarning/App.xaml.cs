using System.Threading;
using System.Windows;

namespace MuteWarning
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex _muteWarningMutex;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _muteWarningMutex = new Mutex(true, nameof(MuteWarning), out bool isNewInstance);

            if (!isNewInstance)
            {
                Current.Shutdown();
            }
        }
    }
}