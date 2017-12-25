using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Web;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.Notifications;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Tweetinvi;
using Tweetinvi.Models;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FYPFinal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class UserDashboard : Page
    {
        protected static string notification;
        protected static bool cyberbully;
        private static ManualResetEvent mre = new ManualResetEvent(false);

        public UserDashboard()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var logincredentials = (LoginCredentials)e.Parameter;
            username.Text = logincredentials.UserName;
            notification = logincredentials.NotificationToggle;
            if (logincredentials.NotificationToggle == "True")
            {
                Notification.Text = "ON";
                Notification.Foreground = new SolidColorBrush(Color.FromArgb(120, 0, 255, 0));
                Notify(0, cyberbully, notification);
            }
            else if (logincredentials.NotificationToggle == "False")
            {
                Notification.Text = "OFF";
                Notification.Foreground = new SolidColorBrush(Color.FromArgb(120, 255, 0, 0));
                Notify(0, cyberbully, notification);
            }
        }

        private async void startstream(object sender, RoutedEventArgs e)
        {
            List<string> streamdata = new List<string>();
            List<string> keyList = new List<string>();
                try
                {
                    var task1 = Task.Run(() => GetKeyword(0))
                        .ContinueWith(prevTask => Connecting(1000, keyList))
                        .ContinueWith(prevTask => RetrieveData(1000))
                        .ContinueWith(prevTask => MakeRequest(1000, streamdata))
                        .ContinueWith(prevTask => Notify(1000, cyberbully, notification));
                    task1.Wait();

                }
                catch (Exception ex)
                {
                    MessageDialog messagebox = new MessageDialog("Task running error:" + ex);
                    await messagebox.ShowAsync();
                }
        }

        /*public async void Stopstream(int sleepTime)
        {
            try
            {
                this.IsHitTestVisible = false;
                this.Opacity = 0.7;

                var title = "Start Streaming...";
                var content = "This process will only stop on demand, please hit the stop button whenever you want to stop the process.";

                var stopCommand = new UICommand("STOP STREAM", cmd => {});
                var dialog = new MessageDialog(content, title);
                dialog.Options = MessageDialogOptions.None;
                dialog.Commands.Add(stopCommand);

                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 0;

                var command = await dialog.ShowAsync();

                if (command == stopCommand)
                {
                    stopstream = true;
                }
            }
            catch (Exception ex)
            {
                MessageDialog messagebox = new MessageDialog("Task running error:" + ex);
                messagebox.ShowAsync();
            }
        }*/

        public static async void Connecting(int sleepTime, List<string> keyList)
        {
            //Set the token that provided by Twitter to gain authorized access into Twitter database
            Auth.SetUserCredentials("YTNuoC9rrJs8g9kZ0hRweKrpp", "wXj6VSl68jeFStRWHDnhG19oP1WZGeBFMNgT3KCkI6MaX46SMT", "892680922322960384-8ka1NuhgiuxjSLUffQVdwmnOIbIduZa", "y92ycGrGCJS9vBJU79gq34rV6FCwNjBPFFOqhEHaTQe1l");

            //Create stream with filter stream type
            var stream = Stream.CreateFilteredStream();
            int numoftweet = 0;
            //Set language filter to English only
            stream.AddTweetLanguageFilter(LanguageFilter.English);
            //Connect to database that stored the keyword
            foreach (var key in keyList)
            {
                stream.AddTrack(key);
            }
            //Let the stream match with all the conditions stated above
            stream.MatchingTweetReceived += async (sender, argument) =>
            {
                //Connect to MongoDB server and database
                var tweet = argument.Tweet;
                try
                {
                    var client = new MongoClient();
                    var database = client.GetDatabase("StreamData");
                    var collection = database.GetCollection<BsonDocument>("StreamData");
                    //Exclude any Retweeted Tweets
                    if (tweet.IsRetweet) return;
                    //Store the data as a BsonDocument into MongoDB database
                    var tweetdata = new BsonDocument
                    {
                        //Store only the data that needed from a Tweet
                        {"Timestamp", tweet.TweetLocalCreationDate},
                        {"TweetID", tweet.IdStr},
                        {"TweetContent",tweet.Text},
                        {"DateCreated", tweet.CreatedBy.CreatedAt.Date},
                        {"UserID", tweet.CreatedBy.IdStr},
                        {"Username", tweet.CreatedBy.Name}
                    };
                    //Insert data into MongoDB database
                    await collection.InsertOneAsync(tweetdata);
                    //Every tweets streamed, add 1 into the variable
                    numoftweet += 1;
                    //If the number of tweets exceed 100, stopped the stream
                    if (numoftweet >= 100)
                    {
                        stream.StopStream();
                        RetrieveData(2000);
                    }
                }
                //Catch if any exception/errors occured
                catch (Exception ex)
                {
                    MessageDialog messagebox = new MessageDialog("MongoDB Connection Error:" + ex);
                    await messagebox.ShowAsync();
                }
            };
            //Start the stream
            stream.StartStreamMatchingAllConditions();
        }

        public static async void GetKeyword(int sleepTime)
        {
            try
            {
                //Connect to MongoDB using localhost (The MongoDB server is run locally, so are all the data stored)
                var client1 = new MongoClient("mongodb://localhost:27017");
                var database1 = client1.GetDatabase("KeywordList");
                var collection1 = database1.GetCollection<BsonDocument>("Keyword");

                //Get only the TweetContent data
                var data = collection1.Aggregate().Project<BsonDocument>(Builders<BsonDocument>.Projection
                    .Exclude(find => find["_id"]).Include(find => find["Keyword"])).ToList();

                //Create a generic list object
                List<string> keywordList = new List<string>();

                //Store each of the data inside the MongoDB Collection into the generic list
                foreach (var d in data)
                {
                    keywordList.Add(d["Keyword"].AsString);
                }

                //mre.WaitOne();
                Connecting(1000, keywordList);
            }
            catch (Exception ex)
            {
                MessageDialog messagebox = new MessageDialog("Retrieve Keyword Error: "+ex);
                await messagebox.ShowAsync();
            }
        }

        public static async void RetrieveData(int sleepTime)
        {
            try
            {
                //Connect to MongoDB using localhost (The MongoDB server is run locally, so are all the data stored)
                var client = new MongoClient("mongodb://localhost:27017");
                var database = client.GetDatabase("StreamData");
                var collection = database.GetCollection<BsonDocument>("StreamData");
                List<string> streamdata = new List<string>();

                //Get only the TweetContent data
                var data = collection.Aggregate().Project<BsonDocument>(Builders<BsonDocument>.Projection
                    .Exclude(find => find["_id"]).Include(find => find["TweetContent"])).ToList();

                //Store each of the data inside the MongoDB Collection into the generic list
                foreach (var d in data)
                {
                    streamdata.Add(d["TweetContent"].AsString);
                }

                //mre.WaitOne();
                MakeRequest(2000, streamdata);
            }
            catch (Exception ex)
            {
                MessageDialog messagebox = new MessageDialog("Retrieve Data Error: "+ex);
                await messagebox.ShowAsync();
            }
        }

        public static async void MakeRequest(int sleepTime, List<string> streamdata)
        {
            //Create object for HttpClient to connect to HTTP endpoint
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            //These AppID and subcription key is get from LUIS.AI and Azure portal
            var luisAppId = "aab50509-223f-444f-921b-d94be61b24ed";
            var subscriptionKey = "0cb92ecd4a194aa7ad5f6bb74ea79529";

            //The request header contains the subscription key from Azure portal
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            //This parameter contains the utterance to send to LUIS
            foreach (var data in streamdata)
            {
                queryString["q"] = data;

                //These optional request parameters are being set followed the LUIS published settings
                queryString["timezoneOffset"] = "0";
                queryString["verbose"] = "true";
                queryString["spellCheck"] = "false";
                queryString["staging"] = "true";

                try
                {
                    //Connect to the published URI of the data model set
                    var uri = "https://southeastasia.api.cognitive.microsoft.com/luis/v2.0/apps/" + luisAppId + "?" + queryString;

                    //Get the response from the LUIS model
                    var response = await client.GetAsync(uri);

                    //Read the LUIS response as string
                    var responseContent = await response.Content.ReadAsStringAsync();

                    //Parse the LUIS response from string to JSON format
                    var jsonresponse = JObject.Parse(responseContent);

                    //Assign only intent from the JSON into variable
                    var intentonly = jsonresponse.SelectToken("topScoringIntent.intent").ToString();

                    //Store into another database if the intent is cyberbully
                    if (intentonly == "Cyberbully")
                    {
                        try
                        {
                            MongoClient mclient = new MongoClient();
                            var sdatabase = mclient.GetDatabase("StreamData");
                            var scollection = sdatabase.GetCollection<BsonDocument>("StreamData");
                            var filter = Builders<BsonDocument>.Filter.Eq("TweetContent", data);
                            var findresult = await scollection.Find(filter).ToListAsync();

                            var arrayresult = new BsonDocument();
                            foreach (var item in findresult)
                            {
                                arrayresult.Add(item);
                            }
                            var bsonresult = arrayresult;

                            var cdatabase = mclient.GetDatabase("CyberbullyData");
                            var ccollection = cdatabase.GetCollection<BsonDocument>("Cyberbully");
                            await ccollection.InsertOneAsync(bsonresult);
                            var deleteresult = await scollection.DeleteManyAsync(bsonresult);
                            cyberbully = true;
                            Notify(2000, cyberbully, notification);
                        }
                        catch (Exception ex)
                        {
                            MessageDialog messagebox = new MessageDialog("MongoDB Connection Error: "+ex);
                            await messagebox.ShowAsync();
                        }

                    }
                    else
                    {
                        try
                        {
                            MongoClient mclient = new MongoClient();
                            var mdatabase = mclient.GetDatabase("StreamData");
                            var mcollection = mdatabase.GetCollection<BsonDocument>("StreamData");
                            var filter = Builders<BsonDocument>.Filter.Eq("TweetContent", data);
                            var result = await mcollection.DeleteManyAsync(filter);
                            cyberbully = false;
                            Notify(2000, cyberbully, notification);
                        }
                        catch (Exception ex)
                        {
                            MessageDialog messagebox = new MessageDialog("MongoDB Connection Error: " + ex);
                            await messagebox.ShowAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                         MessageDialog messagebox = new MessageDialog("LUIS Connection Error: "+ex);
                         messagebox.ShowAsync();
                }
            }
        }

        public static async void Notify(int sleepTime, bool cyberbully, string notification)
        {
            if (notification == "True")
            {
                if (cyberbully)
                {
                    try
                    {
                        // Generate the toast notification content
                        ToastContent content = GenerateToastContent();
                        //Pop the toast
                        ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(content.GetXml()));
                    }
                    catch (Exception ex)
                    {
                        MessageDialog messagebox = new MessageDialog("Toast Notification Error: " + ex);
                        await messagebox.ShowAsync();
                    }
                }
            }
        }

        public static ToastContent GenerateToastContent()
        {
            return new ToastContent()
            {
                Launch = "action=viewEvent&eventId=1983",
                Scenario = ToastScenario.Reminder,

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Cyberbully Content Detected"
                            },
                        }
                    }
                },

                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButtonDismiss()
                    }
                }
            };
        }

        public static void GetCData()
        {
            
        }

        private void Userprofile_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            profiletext.Visibility = Visibility.Visible;
        }

        private void Userprofile_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            profiletext.Visibility = Visibility.Collapsed;
        }

        private void Setting_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            settingtext.Visibility = Visibility.Visible;
        }

        private void Setting_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            settingtext.Visibility = Visibility.Collapsed;
        }

        private void Viewrecord_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            recordtext.Visibility = Visibility.Visible;
        }

        private void Viewrecord_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            recordtext.Visibility = Visibility.Collapsed;
        }

        private void Userprofile_OnClick(object sender, RoutedEventArgs e)
        {
            var logincredentials = new LoginCredentials();
            logincredentials.UserName = username.Text;            
            this.Frame.Navigate(typeof(userprofile),logincredentials);
        }

        private void Setting_OnClick(object sender, RoutedEventArgs e)
        {
            var logincredentials = new LoginCredentials();
            logincredentials.UserName = username.Text;
            logincredentials.NotificationToggle = notification;
            this.Frame.Navigate(typeof(setting), logincredentials);
        }

        private void Viewrecord_OnClick(object sender, RoutedEventArgs e)
        {
            var logincredentials = new LoginCredentials();
            logincredentials.UserName = username.Text;
            this.Frame.Navigate(typeof(viewrecord),logincredentials);
        }

        private void Generatereport_OnClick(object sender, RoutedEventArgs e)
        {
            var logincredentials = new LoginCredentials();
            logincredentials.UserName = username.Text;
            this.Frame.Navigate(typeof(generatereport), logincredentials);
        }

        private void Logout_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void Logout_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            logouttext.Visibility = Visibility.Visible;
        }

        private void Logout_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            logouttext.Visibility = Visibility.Collapsed;
        }
    }
}
