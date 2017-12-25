using System;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Popups;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FYPFinal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Register : Page
    {
        public Register()
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

        public static bool ValidateUsername(string username)
        {
            try
            {
                bool symbol = false;
                bool punctuation = false;

                foreach (char character in username)
                {
                    if (char.IsSymbol(character)) symbol = true;
                    else if (char.IsPunctuation(character)) punctuation = true;
                }

                bool valid = !symbol && !punctuation;
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

        public async Task ConfirmationEmail(Windows.ApplicationModel.Contacts.Contact recipient, string subject, string message)
        {
            var EmailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            EmailMessage.Body = message;

            var email = recipient.Emails.FirstOrDefault<Windows.ApplicationModel.Contacts.ContactEmail>();
            if (email != null)
            {
                var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient(email.Address);
                EmailMessage.To.Add(emailRecipient);
                EmailMessage.Subject = subject;
            }

            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(EmailMessage);
        }

        private async void Signup(object sender, RoutedEventArgs e)
        {
            if (Agreement.IsChecked == false)
            {
                MessageDialog messagebox = new MessageDialog("You must agree on the Terms & Conditions.");
                await messagebox.ShowAsync();
            }
            else if (EmailAddress.Text=="" && Username.Text=="" && Password.Password=="" && ConfirmPassword.Password=="")
            {
                MessageDialog messagebox = new MessageDialog("Do not leave blank on any field.");
                await messagebox.ShowAsync();
            }
            else if (EmailAddress.Text == "" || Username.Text == "" || Password.Password == "" || ConfirmPassword.Password == "")
            {
                MessageDialog messagebox = new MessageDialog("Do not leave blank on any field.");
                await messagebox.ShowAsync();
            }
            else if (Password.Password != ConfirmPassword.Password)
            {
                MessageDialog messagebox = new MessageDialog("The password is not a match. Your current password is "+Password.Password.ToString());
                await messagebox.ShowAsync();
                ConfirmPassword.Password = "";
            }
            else if (ValidatePassword(Password.Password)==false)
            {
                MessageDialog messagebox = new MessageDialog("The password have to be between 8-15 characters and a combination of UpperCase and LowerCase letter as well as digit number. Your current password is " + Password.Password);
                await messagebox.ShowAsync();
                Password.Password = "";
                ConfirmPassword.Password = "";
            }
            else if (ValidateUsername(Username.Text) == false)
            {
                MessageDialog messagebox = new MessageDialog("The username shall not contain any symbols or punctuations. Your current username is " + Username.Text);
                await messagebox.ShowAsync();
                Username.Text = "";
            }
            else if (ValidateEmail(EmailAddress.Text) == false)
            {
                MessageDialog messagebox = new MessageDialog("The email address is not in the correct format. Your current email address is " + EmailAddress.Text);
                await messagebox.ShowAsync();
                EmailAddress.Text = "";
            }
            else
            {
                string connetionString = null;
                SqlConnection connection;
                SqlCommand command;
                bool nameexistence;
                bool emailexistence;
                string subject = "Welcome To CyberXoX";
                string message = "Congratulations on successfully registered at CyberXoX! Please login using your credentials in the application!"+"\n\nBelow is your registration details: "+"\nUsername: "+Username.Text+"\nPassword: "+Password.Password;

                connetionString = "Data Source=DESKTOP-2UB7OKA;Initial Catalog=RegisterLogin;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True";

                connection = new SqlConnection(connetionString);
                connection.Open();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Register WHERE Username = @Username",
                        connection))
                    {
                        cmd.Parameters.AddWithValue("UserName", Username.Text);
                        nameexistence = (int) cmd.ExecuteScalar() > 0;
                    }

                    using (SqlCommand cmd =
                        new SqlCommand("SELECT COUNT(*) FROM Register WHERE EmailAddress = @EmailAddress", connection))
                    {
                        cmd.Parameters.AddWithValue("EmailAddress", EmailAddress.Text);
                        emailexistence = (int) cmd.ExecuteScalar() > 0;
                    }

                    if (nameexistence)
                    {
                        MessageDialog messagebox = new MessageDialog("This username has already been registered.");
                        await messagebox.ShowAsync();
                        Username.Text = "";
                    }
                    else if (emailexistence)
                    {
                        MessageDialog messagebox = new MessageDialog("This email has already been registered.");
                        await messagebox.ShowAsync();
                        EmailAddress.Text = "";
                    }
                    else
                    {
                        String query = "Insert into Register(EmailAddress,Username,Password,ConPassword) values ('" +
                                       EmailAddress.Text + "','" + Username.Text + "','" + Password.Password + "','" +
                                       ConfirmPassword.Password + "')";
                        command = new SqlCommand();
                        command.CommandText = query;
                        command.Connection = connection;
                        command.ExecuteNonQuery();
                        try
                        {
                            var fromAddress = new MailAddress("peiernlim@gmail.com");
                            var fromPassword = "jthbqieduhvjypdz";
                            var toAddress = new MailAddress(EmailAddress.Text);

                            SmtpClient smtp = new SmtpClient
                            {
                                Host = "smtp.gmail.com",
                                Port = 587,
                                EnableSsl = true,
                                DeliveryMethod = SmtpDeliveryMethod.Network,
                                UseDefaultCredentials = false,
                                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)

                            };

                            MailMessage mail = new MailMessage(fromAddress, toAddress);
                            mail.Subject = subject;
                            mail.Body = message;
                            smtp.Send(mail);
                        }
                        catch (Exception ex)
                        {
                            MessageDialog messagebox1 = new MessageDialog("Confirmation Email Sending Error: "+ex);
                            await messagebox1.ShowAsync();
                        }
                        MessageDialog messagebox2 = new MessageDialog("Registered Successfully. Please check your email inbox for confirmation email.");
                        await messagebox2.ShowAsync();
                        EmailAddress.Text = "";
                        Username.Text = "";
                        Password.Password = "";
                        ConfirmPassword.Password = "";
                        connection.Close();
                        this.Frame.Navigate(typeof(MainPage));
                    }
                }
                catch (Exception ex)
                {
                    MessageDialog messagebox = new MessageDialog("Database connection error: " + ex);
                    await messagebox.ShowAsync();
                    this.Frame.Navigate(typeof(MainPage));
                }
            }
        }

        private void termscondition(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
