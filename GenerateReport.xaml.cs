using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FYPFinal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class generatereport : Page
    {
        public generatereport()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var logincredentials = (LoginCredentials)e.Parameter;
            username.Text = logincredentials.UserName;
        }

        private void DownloadReport(object sender, RoutedEventArgs e)
        {
            
        }

        private void ViewInApp(object sender, RoutedEventArgs e)
        {
            
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            var logincredentials = new LoginCredentials();
            logincredentials.UserName = username.Text;
            this.Frame.Navigate(typeof(UserDashboard), logincredentials);
        }
    }
}
