using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using WPRssReader.Helper;
using WPRssReader.Model;
using WPRssReader.Resources;
using MSPToolkit.Utilities;
using Microsoft.Phone.Data.Linq;

namespace WPRssReader
{
    public partial class App
    {
        // The static ViewModel, to be used across the application.
        private const string DbConnectionString = "Data Source=isostore:/RssDB.sdf";

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters
                Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }


            // Create the database if it does not exist.
            using (var db = new BaseDataContext(DbConnectionString))
            {
                if (db.DatabaseExists() == false)
                {
                    // Create the local database.
                    db.CreateDatabase();

                    // Save categories to the database.
                    db.SubmitChanges();

                    // create an instance of DatabaseSchemaUpdater
                    DatabaseSchemaUpdater schemaUpdater = db.CreateDatabaseSchemaUpdater();
                    
                    // assign database schema version before calling Execute
                    schemaUpdater.DatabaseSchemaVersion = 1;
                    // execute changes to database schema
                    schemaUpdater.Execute();
                }
                else
                {
                    // create an instance of DatabaseSchemaUpdater
                    DatabaseSchemaUpdater schemaUpdater = db.CreateDatabaseSchemaUpdater();
                    // get current database schema version
                    // if not changed the version is 0 by default
                    int version = schemaUpdater.DatabaseSchemaVersion;

                    // if current version of database schema is old
                    if (version == 0)
                    {
                        // add Address column to the table corresponding to the Person class
                        schemaUpdater.AddColumn<Channel>("Image");
                        schemaUpdater.AddColumn<Article>("AddDate");
                        // IMPORTANT: update database schema version before calling Execute
                        schemaUpdater.DatabaseSchemaVersion = 1;
                        // execute changes to database schema
                        schemaUpdater.Execute();
                    }                
                }
            }
        }

        public static RssViewModel ViewModel { get; private set; }
        public static SolidColorBrush WhiteColor = new SolidColorBrush(Colors.White);
        public static AppSettings Settings { get; private set; }
        // Specify the local database connection string.

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            SetupApp();
        }

        private static void SetupApp()
        {
            // Create the ViewModel object.
            ViewModel = new RssViewModel(DbConnectionString);
            // Query the local database and load observable collections.
            ViewModel.LoadCollectionsFromDatabase();
            ViewModel.Accent = ((Color)Current.Resources["PhoneAccentColor"]).ToHTML();
            ViewModel.Background = (Visibility)Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible
                                       ? Colors.Black.ToHTML()
                                       : Colors.White.ToHTML();
            ViewModel.Foreground = ViewModel.Background == Colors.Black.ToHTML()
                                       ? Colors.White.ToHTML()
                                       : Colors.Black.ToHTML();

            Settings = new AppSettings();
            RssViewModel.SelectCount = Settings.LoadItemsCountSetting;
            if (Settings.UpdateOnStartUpSetting)
            {
                ViewModel.DoAddNewArticles();
            }
        }

        public static void LeaveFeedback()
        {
            var marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            //SetupApp();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            SaveDataAndUpdateTile();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            SaveDataAndUpdateTile();
        }

        private static void SaveDataAndUpdateTile()
        {
            for (int i = 0; i < ViewModel.Channels.Count; i++)
            {
                ViewModel.Channels[i].Index = i;
            }

            // Save changes to the database.
            ViewModel.SaveChangesToDb();

            // Ensure that required application state is persisted here.
            ShellTile apptile = ShellTile.ActiveTiles.First();

            apptile.Update(AddTile(ViewModel.NewCount, "first"));

            foreach (Channel c in ViewModel.Channels)
            {
                string url = string.Format("/ChannelPage.xaml?ID={0}", c.ID);
                ShellTile tileToFind =
                    ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(url));
                // If the Tile was found, then update the background image.
                if (tileToFind != null)
                {
                    tileToFind.Update(AddTile(c.NewCount, c.ID.ToString(), c.Title));
                }
            }
        }

        public static StandardTileData AddTile(int count, string id, string title = null)
        {
            StandardTileData secTileData = count > 0
                                               ? new StandardTileData
                                               {
                                                   Title = title,
                                                   BackgroundImage = TileImageGenerator.GenerateTile(
                                                                           new Uri("Background.png", UriKind.Relative),
                                                                           (uint)count,
                                                                           String.Format("TileImage{0}", id))
                                               }

                                               : new StandardTileData
                                               {
                                                   Title = title,
                                                   BackgroundImage =
                                                       new Uri("Background.png",
                                                               UriKind.Relative)
                                               };
            return secTileData;
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}