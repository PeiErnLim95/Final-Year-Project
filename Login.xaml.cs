using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FYPFinal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public class LoginCredentials
    {
        public string UserName { get; set; }
        public string NotificationToggle { get; set; }
    }

    public sealed partial class Login : Page
    {
        public Login()
        {
            this.InitializeComponent();
        }

        private async void Signin(object sender, RoutedEventArgs e)
        {
            if (username.Text=="" || password.Password =="")
            {
                MessageDialog messagebox = new MessageDialog("Do not leave blank on any field.");
                await messagebox.ShowAsync();
            }
            else
            {
                var logincredentials = new LoginCredentials();
                try
                {
                    SqlConnection connection = new SqlConnection(@"Data Source=DESKTOP-2UB7OKA;Initial Catalog=RegisterLogin;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True");
                    SqlDataAdapter dataadapter = new SqlDataAdapter("SELECT COUNT(*) FROM Register WHERE Username='" + username.Text + "' AND Password='" + password.Password + "'", connection);
                    DataTable datatable = new DataTable(); 
                    dataadapter.Fill(datatable);
                    if (datatable.Rows[0][0].ToString() == "1")
                    {
                        logincredentials.UserName = username.Text;
                        logincredentials.NotificationToggle = "True";
                        MessageDialog messagebox = new MessageDialog("Login successfully.");
                        await messagebox.ShowAsync();
                        connection.Close();
                        this.Frame.Navigate(typeof(UserDashboard), logincredentials);
                    }
                    else
                    {
                        MessageDialog messagebox = new MessageDialog("Invalid User! Please check your username and password.");
                        await messagebox.ShowAsync();
                        this.Frame.Navigate(typeof(Login));
                    }
                }
                catch (Exception ex)
                {
                    MessageDialog messagebox = new MessageDialog("Database Connection Error." + ex);
                    await messagebox.ShowAsync();
                    this.Frame.Navigate(typeof(Login));
                }
            }
        }

        private async void forgotpassword(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            if (username.Text=="")
            {
                MessageDialog messagebox = new MessageDialog("Please enter your username to request for password. If you have forgot your username as well, please contact customer service via cs@cyberxox.com.");
                await messagebox.ShowAsync();
            }
            else
            {
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
                            using (command1 = new SqlCommand("SELECT EmailAddress FROM Register WHERE Username = @Username",connection))
                            {
                                command1.Parameters.AddWithValue("@Username", username.Text);
                                email = command1.ExecuteScalar().ToString();
                            }

                            using (command2 = new SqlCommand("SELECT Password FROM Register WHERE Username = @Username", connection))
                            {
                                command2.Parameters.AddWithValue("@Username", username.Text);
                                passworder = command2.ExecuteScalar().ToString();
                            }

                            try
                            {
                                var fromAddress = new MailAddress("peiernlim@gmail.com");
                                var fromPassword = "jthbqieduhvjypdz";
                                var toAddress = new MailAddress(email);

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
                                mail.Subject = "CyberXoX - Password Reminder";
                                mail.Body = "Dear " + username.Text + ", \n\nBelow is your requested login credentials, please contact to customer service if you do not perform this action: \nUsername: " + username.Text + "\nPassword: " + passworder + "\n\nCustomer Service: cs@cyberxox.com";
                                smtp.Send(mail);
                                MessageDialog messagebox = new MessageDialog("The login credentials has been sent to the registered email. Please check your email and try to login again.");
                                await messagebox.ShowAsync();
                                this.Frame.Navigate(typeof(Login));
                            }
                            catch (Exception ex)
                            {
                                MessageDialog messagebox = new MessageDialog("Confirmation Email Error: " + ex);
                                await messagebox.ShowAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageDialog messagebox = new MessageDialog("The username is not existed in our database, please register your account with us.");
                            await messagebox.ShowAsync();
                        }
                    }
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
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
