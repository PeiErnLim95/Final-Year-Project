using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FYPFinal
{

    public sealed partial class useredit : Page
    {
        public useredit()
        {
            this.InitializeComponent();
        }

        public static bool ValidatePassword(string password)
        {
            try
            {
                const int minimum = 8;
                const int maximum = 15;

                bool length = password.Length >= minimum && password.Length <= maximum;
                bool upperCase = false;
                bool lowerCase = false;
                bool digit = false;
                bool symbol = false;
                bool punctuation = false;

                if (length)
                {
                    foreach (char character in password)
                    {
                        if (char.IsUpper(character)) upperCase = true;
                        else if (char.IsLower(character)) lowerCase = true;
                        else if (char.IsDigit(character)) digit = true;
                        else if (char.IsSymbol(character)) symbol = true;
                        else if (char.IsPunctuation(character)) punctuation = true;
                    }
                }

                bool valid = length && upperCase && lowerCase && digit && !symbol && !punctuation;
                return valid;
            }
            catch
            {
                return false;
            }
        }

        public static bool ValidateEmail(string email)
        {
            try
            {
                var addrress = new System.Net.Mail.MailAddress(email);
                return addrress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var logincredentials = (LoginCredentials)e.Parameter;
            SqlCommand command1;
            SqlCommand command2;
            string email = null;
            string passworder = null;
            try
            {
                SqlConnection connection = new SqlConnection(@"Data Source=DESKTOP-2UB7OKA;Initial Catalog=RegisterLogin;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True");
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
                    MessageDialog messagebox = new MessageDialog("Database Connection Error: " + ex);
                    messagebox.ShowAsync();
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                MessageDialog messagebox = new MessageDialog(ex.StackTrace);
                messagebox.ShowAsync();
            }
        }

        private async void Save(object sender, RoutedEventArgs e)
        {
            if (Email.Text=="" || Password.Password=="" || ConPassword.Password=="")
            {
                MessageDialog messagebox = new MessageDialog("Do not leave blank on any field.");
                await messagebox.ShowAsync();
            }
            else if (ValidatePassword(Password.Password) == false)
            {
                MessageDialog messagebox = new MessageDialog("The password have to be between 8-15 characters and a combination of UpperCase and LowerCase letter as well as digit number. Your current password is " + Password.Password);
                await messagebox.ShowAsync();
                Password.Password = "";
            }
            else if (ValidateEmail(Email.Text) == false)
            {
                MessageDialog messagebox = new MessageDialog("The email address is not in the correct format. Your current email address is " + Email.Text);
                await messagebox.ShowAsync();
                Email.Text = "";
            }
            else if (Password.Password != ConPassword.Password)
            {
                MessageDialog messagebox = new MessageDialog("The password is not a match. Your current password is " + Password.Password);
                await messagebox.ShowAsync();
                Password.Password = "";
            }
            else
            {
                string connetionString = null;
                SqlConnection connection;
                SqlCommand command;

                connetionString =
                    "Data Source=DESKTOP-2UB7OKA;Initial Catalog=RegisterLogin;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True";

                connection = new SqlConnection(connetionString);
                try
                {
                    connection.Open();
                    command = new SqlCommand(
                        "UPDATE Register SET EmailAddress=@NEmailAddress, Password=@NPassword WHERE (Username = '" +
                        Username.Text + "')", connection);
                    {
                        command.Parameters.AddWithValue("@NEmailAddress", Email.Text);
                        command.Parameters.AddWithValue("@NPassword", Password.Password);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                    MessageDialog messagebox = new MessageDialog("The profile has been updated successfully.");
                    await messagebox.ShowAsync();
                    var logincredentials = new LoginCredentials();
                    logincredentials.UserName = Username.Text;
                    this.Frame.Navigate(typeof(UserDashboard), logincredentials);
                }
                catch (Exception ex)
                {
                    MessageDialog messagebox = new MessageDialog("Database Connection Error: " + ex);
                    await messagebox.ShowAsync();
                }
            }
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            var logincredentials = new LoginCredentials();
            logincredentials.UserName = Username.Text;
            this.Frame.Navigate(typeof(userprofile),logincredentials);
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
