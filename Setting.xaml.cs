using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MongoDB.Bson;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FYPFinal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class setting : Page
    {
        protected string username;
        public setting()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var logincredentials = (LoginCredentials)e.Parameter;
            username = logincredentials.UserName;
            if (logincredentials.NotificationToggle == "True")
            {
                NotificationSwtich.IsOn = true;
            }
            else if (logincredentials.NotificationToggle == "False")
            {
                NotificationSwtich.IsOn = false;
            }
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            var logincredentials = new LoginCredentials();
            logincredentials.UserName = username;
            this.Frame.Navigate(typeof(UserDashboard),logincredentials);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            var logincredentials = new LoginCredentials();
            logincredentials.UserName = username;
            if (NotificationSwtich.IsOn)
            {
                logincredentials.NotificationToggle = "True";
                this.Frame.Navigate(typeof(UserDashboard), logincredentials);
            }
            else if (!NotificationSwtich.IsOn)
            {
                logincredentials.NotificationToggle = "False";
                this.Frame.Navigate(typeof(UserDashboard), logincredentials);
            }
        }
    }
}
