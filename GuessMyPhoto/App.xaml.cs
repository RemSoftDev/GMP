using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Networking.Connectivity;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using GuessMyPhoto.Views;

namespace GuessMyPhoto
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static bool CanGoBack = true;
        public static bool IgnorGoBackRequest = false;
        public static bool IsWindowsMobile;
        public static string ApiUrl = "http://www.guessmyphoto.net/api3/";
        public static string FbClientId = "405668006193095";

        private bool registeredNetworkStatusNotif;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                IsWindowsMobile = true;
            this.Suspending += OnSuspending;
        }

        private void BaseFramePage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (IgnorGoBackRequest)
            {
                e.Handled = true;
                return;
            }
            Frame rootFrame = Window.Current.Content as Frame;

            if (!e.Handled && rootFrame != null && rootFrame.CanGoBack && rootFrame.CurrentSourcePageType != typeof(StartPage) && CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }
        }

        private async void ConfigureStatusBar()
        {
            //if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            //{
            //    var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            //    if (titleBar != null)
            //    {
            //        titleBar.BackgroundColor = Colors.Orange;
            //        titleBar.ForegroundColor = Colors.White;
            //    }
            //}

            //Mobile customization
            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }
        }
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = false;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(StartPage), e.Arguments);
                }
                ConfigureStatusBar();
                if (IsWindowsMobile)
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
                SystemNavigationManager.GetForCurrentView().BackRequested += BaseFramePage_BackRequested;
                //Ensure the current window is active
                Window.Current.Activate();
                //var networkStatusCallback = new NetworkStatusChangedEventHandler(OnNetworkStatusChange);
                //if (!registeredNetworkStatusNotif)
                //{
                //    NetworkInformation.NetworkStatusChanged += networkStatusCallback;
                //    registeredNetworkStatusNotif = true;
                //}
            }
        }
        //async void OnNetworkStatusChange(object sender)
        //{
        //    // get the ConnectionProfile that is currently used to connect to the Internet                
        //    ConnectionProfile internetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
        //    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        //    if (internetConnectionProfile == null)
        //    {
        //        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //        {
        //            var dialog = new MessageDialog("Not connected to Internet\n");
        //        });
        //    }
        //    else
        //    {
        //        connectionProfileInfo = GetConnectionProfile(internetConnectionProfile);
        //        await _cd.RunAsync(CoreDispatcherPriority.Normal, () =>
        //        {
        //            rootPage.NotifyUser(connectionProfileInfo, NotifyType.StatusMessage);
        //        });
        //    }
        //    internetProfileInfo = "";
        //}

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
