using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using ClipboardZanager.ComponentModel.Services;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.IO;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Core.Desktop.Services;
using ClipboardZanager.Properties;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Shared.Services;
using ClipboardZanager.Strings;
using Newtonsoft.Json;

namespace ClipboardZanager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Handled Methods

        /// <summary>
        /// Occurs when the Run method of the Application object is called.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (Environment.OSVersion.Version.Major < 10)
            {
                MessageBox.Show("This application is only compatible with Windows 10.", "ClipboardZanager", MessageBoxButton.OK, MessageBoxImage.Stop);
                Current.Shutdown(Consts.SingleInstanceProcessExitCode);
                return;
            }

            Logger.InstanceLogSession = new LogSession();

            if (SingleInstance(e))
            {
                return;
            }

            ServiceLocator.SettingProvider = new ServiceSettingProvider();
            LanguageManager.GetInstance().SetCurrentCulture(new CultureInfo(Settings.Default.Language));

            SwitchColorTheme(SystemParameters.HighContrast);
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;

            if (Settings.Default.FirstStart)
            {
                var operatingSystemCulture = LanguageManager.GetInstance().GetAvailableCultures().FirstOrDefault(culture => culture.Name == CultureInfo.CurrentUICulture.Name);
                if (operatingSystemCulture != null)
                {
                    LanguageManager.GetInstance().SetCurrentCulture(operatingSystemCulture);
                    Settings.Default.Language = operatingSystemCulture.Name;
                }

                CoreHelper.UpdateStartWithWindowsShortcut(true);
                Settings.Default.KeyboardShortcut = Consts.DEFAULT_KeyboardHotKeys;
                Settings.Default.KeepDataTypes = Consts.DEFAULT_DataTypesToKeep;
                Settings.Default.IgnoredApplications = JsonConvert.SerializeObject(new ObservableCollection<IgnoredApplication>());
                Settings.Default.Save();
                Settings.Default.Reload();
            }

            Logger.Instance.Information($"Application v.{CoreHelper.GetApplicationVersion()} started.");
        }

        /// <summary>
        /// Occurs when the user ends the Windows session by logging off or shutting down the operating system.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            Logger.Instance.Information($"The user user ends the Windows session by logging off or shutting down the operating system. The application will stop.");
        }

        /// <summary>
        /// Occurs just before an application shuts down, and cannot be canceled.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (e.ApplicationExitCode == Consts.SingleInstanceProcessExitCode)
            {
                return;
            }

            ServiceLocator.GetService<ClipboardService>().Pause();
            ServiceLocator.GetService<MouseAndKeyboardHookService>().Pause();

            SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;

            Settings.Default.Save();
            Logger.Instance.Information($"Application shutdown width the exit code : {e.ApplicationExitCode}");
            Logger.Instance.Flush();
            Logger.Instance.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var st = new StackTrace(e.Exception, true);
            var frame = st.GetFrame(0);
            Logger.Instance.Fatal($"A unhandled exception occured : {e.Exception.Message}\nStack trace :\n{GetStackTrace(st)}\n", null, frame.GetMethod().Name, frame.GetFileName(), frame.GetFileLineNumber());
            Logger.Instance.Dispose();

            if (!Debugger.IsAttached)
            {
                Process.Start(Assembly.GetEntryAssembly().Location, string.Join(" ", Environment.GetCommandLineArgs(), "-skipsingleinstance"));
                Current.Shutdown(0);
            }
        }

        /// <summary>
        /// Occurs when the connection to internet changes.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private async void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            var cloudStorageService = ServiceLocator.GetService<CloudStorageService>();
            await cloudStorageService.TryAuthenticateAsync();
        }

        /// <summary>
        /// Occurs when a system parameter changes.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HighContrast")
            {
                SwitchColorTheme(SystemParameters.HighContrast);
            }
        }

        #endregion

        #region Methods 

        /// <summary>
        /// Check is there is more than one instance of the application, and if yes, it's killing it.
        /// </summary>
        /// <returns>Returns True if another instance has been founded and that the current process should be killed.</returns>
        /// <param name="e">The arguments of the <see cref="Application.Startup"/> application event.</param>
        private bool SingleInstance(StartupEventArgs startupInfo)
        {
            if (Debugger.IsAttached)
            {
                return false;
            }

            if (startupInfo.Args.Contains("-skipsingleinstance"))
            {
                Logger.Instance.Information($"Application started with -skipsingleinstance flag. Probably because of a previous crash on the previous instance.");
                return false;
            }

            var currentProcess = Process.GetCurrentProcess();
            if (Process.GetProcessesByName(currentProcess.ProcessName).Count() > 1)
            {
                Logger.Instance.Information($"Another instance of the software has been detected. The current process will stop.");
                Current.Shutdown(Consts.SingleInstanceProcessExitCode);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Switch the color theme from normal color schema to high contrast and vice versa.
        /// </summary>
        /// <param name="isHighContrast">Defines wether the high contrast of Windows is enabled.</param>
        private void SwitchColorTheme(bool isHighContrast)
        {
            if (Current == null)
            {
                return;
            }

            string colorSchema;

            if (isHighContrast)
            {
                colorSchema = $"{Assembly.GetExecutingAssembly().GetName().Name};component\\Themes/HighContrastColorSchema.xaml";
            }
            else
            {
                colorSchema = $"{Assembly.GetExecutingAssembly().GetName().Name};component\\Themes/DefaultColorSchema.xaml";
            }

            var resourceDictionary = (ResourceDictionary)LoadComponent(new Uri(colorSchema, UriKind.Relative));

            Current.Resources.MergedDictionaries.RemoveAt(0);
            Current.Resources.MergedDictionaries.Insert(0, resourceDictionary);

            for (var i = 0; i < Current.Windows.Count; i++)
            {
                Current.Windows[i]?.UpdateDefaultStyle();
            }
        }

        /// <summary>
        /// Returns the call stack.
        /// </summary>
        /// <param name="stackTrace">The stack trace.</param>
        /// <returns>A string with one call per line.</returns>
        private string GetStackTrace(StackTrace stackTrace)
        {
            if (stackTrace == null)
            {
                return string.Empty;
            }

            var result = new StringBuilder();
            foreach (var stackFrame in stackTrace.GetFrames())
            {
                result.AppendLine($"{stackFrame.GetMethod().Name} in {Path.GetFileName(stackFrame.GetFileName())}, line {stackFrame.GetFileLineNumber()}");
            }

            return result.ToString();
        }

        #endregion
    }
}
