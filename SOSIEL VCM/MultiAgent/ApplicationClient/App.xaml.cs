using System.Windows;
using GalaSoft.MvvmLight.Threading;
using RDotNet;

namespace ApplicationClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
            REngine.SetEnvironmentVariables();
        }
    }
}
