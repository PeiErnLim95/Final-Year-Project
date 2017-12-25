using System;
using System.Data.SqlClient;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FYPFinal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class userprofile : Page
    {
        public userprofile()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var logincredentials = (LoginCredentials)e.Parameter;
            try
            {
                using (SqlConnection connection = new SqlConnection("Data Source=DESKTOP-2UB7OKA;Initial Catalog=RegisterLogin;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True"))
                {
                    SqlCommand command1;
                    SqlCommand command2;
                    string email = null;
                    string passworder = null;
                    connection.Open();
                    try
                    {
                        using (command1 = new SqlCommand("SELECT EmailAddress FROM Register WHERE Username = @Username", connection))
                        {
                            command1.Parameters.AddWithValue("@Username", logincredentials.UserName);
                            email = command1.ExecuteScalar().ToString();
                        }

                        using (command2 = new SqlCommand("SELECT Password FROM Register WHERE Username = @Username", connection))
                        {
                            command2.Parameters.AddWithValue("@Username", logincredentials.UserName);
                            passworder = command2.ExecuteScalar().ToString();
                        }

                        Email.Text = email;
                        Username.Text = logincredentials.UserName;
                        Password.Password = passworder;
                    }
                    catch (Exception ex)
                    {
                        MessageDialog messagebox = new MessageDialog("Database Connection Error: "+ex);
                        messagebox.ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageDialog messagebox = new MessageDialog("Database Connection Error: " + ex);
                messagebox.ShowAsync();
            }
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            var logincredentials = new LoginCredentials();
            logincredentials.UserName = Username.Text;
            this.Frame.Navigate(typeof(useredit),logincredentials);
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            var logincredentials = new LoginCredentials();
            logincredentials.UserName = Username.Text;
            this.Frame.Navigate(typeof(UserDashboard), logincredentials);
        }
    }
}
