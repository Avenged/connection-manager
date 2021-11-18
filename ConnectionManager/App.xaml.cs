using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ConnectionManager.Common;
using ConnectionManager.Common.Services;
using ConnectionManager.Helpers;
using ConnectionManager.Services;
using ConnectionManager.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Foundation.Metadata;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.Storage;
using Windows.UI.Xaml.Media.Animation;
using ConnectionManager.Views;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ConnectionManager.Filesystem;
using Windows.Foundation.Collections;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ConnectionManager.Common.Models;
using AutoMapper;

namespace ConnectionManager
{
    public sealed partial class App : Application
    {
        private static bool ShowErrorNotification = false;
        private Lazy<ActivationService> _activationService;

        public static MainViewModel MainViewModel { get; private set; }

        public static Logger Logger { get; private set; }
        private static readonly UniversalLogWriter logWriter = new UniversalLogWriter();

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        public IServiceProvider Services { get; private set; }

        public App()
        {
            // Initialize logger
            Logger = new Logger(logWriter);
          
            UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedException;

            InitializeComponent();

            AppServiceConnectionHelper.Register();

            Services = ConfigureServices();
            Ioc.Default.ConfigureServices(Services);

            // Deferred execution until used. Check https://docs.microsoft.com/dotnet/api/system.lazy-1 for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        private IServiceProvider ConfigureServices()
        {
            ServiceCollection services = new();

            services.AddSingleton(new NavigationService());
            services.AddSingleton(new ProjectService());
            services.AddSingleton(new EnvironmentService());
            services.AddSingleton(new ActivedEnvironmentService());
            services.AddSingleton(new MapperConfiguration((configuration) =>
            {
                configuration.CreateMap<EnvironmentDO, Common.Models.Environment>();
                configuration.CreateMap<Common.Models.Environment, EnvironmentDO>();
                configuration.CreateMap<ProjectDO, Project>();
                configuration.CreateMap<Project, ProjectDO>();
            }));

            return services.BuildServiceProvider();
        }

        private static void EnsureSettingsAndConfigurationAreBootstrapped()
        {
            MainViewModel ??= new MainViewModel();
        }

        private void TryEnablePrelaunch()
        {
            CoreApplication.EnablePrelaunch(true);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            var path = ApplicationData.Current.LocalFolder;

            await logWriter.InitializeAsync("debug.log");
            Logger.Info($"App launched. Prelaunch: {args.PrelaunchActivated}");

            //start tracking app usage
            SystemInformation.Instance.TrackAppUse(args);

            bool canEnablePrelaunch = ApiInformation.IsMethodPresent("Windows.ApplicationModel.Core.CoreApplication", "EnablePrelaunch");

            EnsureSettingsAndConfigurationAreBootstrapped();

            //var rootFrame = EnsureWindowIsInitialized();

            if (args.PrelaunchActivated == false)
            {
                if (canEnablePrelaunch)
                {
                    TryEnablePrelaunch();
                }

                await ActivationService.ActivateAsync(args);

                //if (rootFrame.Content == null)
                //{
                //    // When the navigation stack isn't restored navigate to the first page,
                //    // configuring the new page by passing required information as a navigation
                //    // parameter

                //    //rootFrame.Navigate(typeof(ShellPage), args.Arguments, new SuppressNavigationTransitionInfo());
                //}
                //else
                //{
                //    //if (!(string.IsNullOrEmpty(e.Arguments) && MainPageViewModel.AppInstances.Count > 0))
                //    //{
                //    //    await MainPageViewModel.AddNewTabByPathAsync(typeof(PaneHolderPage), e.Arguments);
                //    //}
                //}

                // Ensure the current window is active
                Window.Current.Activate();
                Window.Current.CoreWindow.Activated += CoreWindow_Activated;
            }
            else
            {
                //if (rootFrame.Content == null)
                //{
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                //    rootFrame.Navigate(typeof(ShellPage), args.Arguments, new SuppressNavigationTransitionInfo());
                // }
                //else
                //{
                //if (!(string.IsNullOrEmpty(e.Arguments) && MainPageViewModel.AppInstances.Count > 0))
                //{
                //    await MainPageViewModel.AddNewTabByPathAsync(typeof(PaneHolderPage), e.Arguments);
                //}
                //}
            }
        }

        private Frame EnsureWindowIsInitialized()
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (Window.Current.Content is not Frame rootFrame)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame
                {
                    CacheSize = 1
                };

                rootFrame.NavigationFailed += OnNavigationFailed;

                //if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                //{
                //    //TODO: Load state from previously suspended application
                //}

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            return rootFrame;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == CoreWindowActivationState.CodeActivated ||
                args.WindowActivationState == CoreWindowActivationState.PointerActivated)
            {
                ShowErrorNotification = true;
                ApplicationData.Current.LocalSettings.Values["INSTANCE_ACTIVE"] = Process.GetCurrentProcess().Id;
                if (MainViewModel != null)
                {
                    //MainViewModel.Clipboard_ContentChanged(null, null);
                }
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            EnsureSettingsAndConfigurationAreBootstrapped();
            await ActivationService.ActivateAsync(args);
        }

        // Occurs when an exception is not handled on the UI thread.
        private static void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e) => AppUnhandledException(e.Exception);

        // Occurs when an exception is not handled on a background thread.
        // ie. A task is fired and forgotten Task.Run(() => {...})
        private static void OnUnobservedException(object sender, UnobservedTaskExceptionEventArgs e) => AppUnhandledException(e.Exception);

        private static void AppUnhandledException(Exception ex)
        {
            var type = ex.GetType().FullName;

            if (ex is TaskCanceledException || ex is AggregateException)
            {
                return;
            }

            string formattedException = string.Empty;

            formattedException += "--------- UNHANDLED EXCEPTION ---------";
            if (ex != null)
            {
                formattedException += $"\n>>>> HRESULT: {ex.HResult}\n";
                if (ex.Message != null)
                {
                    formattedException += "\n--- MESSAGE ---";
                    formattedException += ex.Message;
                }
                if (ex.StackTrace != null)
                {
                    formattedException += "\n--- STACKTRACE ---";
                    formattedException += ex.StackTrace;
                }
                if (ex.Source != null)
                {
                    formattedException += "\n--- SOURCE ---";
                    formattedException += ex.Source;
                }
                if (ex.InnerException != null)
                {
                    formattedException += "\n--- INNER ---";
                    formattedException += ex.InnerException;
                }
            }
            else
            {
                formattedException += "\nException is null!\n";
            }

            formattedException += "---------------------------------------";

            Debug.WriteLine(formattedException);

            Debugger.Break(); // Please check "Output Window" for exception details (View -> Output Window) (CTRL + ALT + O)

            Logger.UnhandledError(ex, ex.Message);
            if (ShowErrorNotification)
            {
                var toastContent = new ToastContent()
                {
                    Visual = new ToastVisual()
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = "ExceptionNotificationHeader".GetLocalized()
                                },
                                new AdaptiveText()
                                {
                                    Text = "ExceptionNotificationBody".GetLocalized()
                                }
                            },
                            AppLogoOverride = new ToastGenericAppLogo()
                            {
                                Source = "ms-appx:///Assets/error.png"
                            }
                        }
                    },
                    Actions = new ToastActionsCustom()
                    {
                        Buttons =
                        {
                            new ToastButton("ExceptionNotificationReportButton".GetLocalized(), "report")
                            {
                                ActivationType = ToastActivationType.Foreground
                            }
                        }
                    }
                };

                // Create the toast notification
                var toastNotif = new ToastNotification(toastContent.GetXml());

                // And send the notification
                ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
            }
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(ProjectsPage), new Lazy<UIElement>(CreateShell));
        }

        private UIElement CreateShell()
        {
            return new ShellPage();
        }
    }
}
