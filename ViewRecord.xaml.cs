using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using FYPFinal.Model;
using MongoDB.Bson;
using MongoDB.Driver;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FYPFinal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
   
    public sealed partial class viewrecord : Page
    {
        public viewrecord()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var logincredentials = (LoginCredentials)e.Parameter;
            username.Text = logincredentials.UserName;
            Display();
        }

        public async void Display()
        {
            try
            {
                MongoClient client = new MongoClient("mongodb://localhost");

                var database = client.GetDatabase("CyberbullyData");
                var collection = database.GetCollection<BsonDocument>("Cyberbully");
                var filter = new BsonDocument();
                IEnumerable<BsonDocument> document;
                var cursor = await collection.FindAsync(filter);
                ObservableCollection<Data> dataList = new ObservableCollection<Data>();
                while (await cursor.MoveNextAsync())
                {
                    document = cursor.Current;
                    foreach (var item in document)
                    {
                        Data data = new Data(item["Timestamp"].ToString(), item["TweetID"].ToString(),
                            item["TweetContent"].ToString(), item["DateCreated"].ToString(), item["UserID"].ToString(),
                            item["Username"].ToString());
                        dataList.Add(data);
                    }
                }
                DataSource.Source = dataList;
            }
            catch (Exception ex)
            {
                MessageDialog messagebox = new MessageDialog("Display Data Error: " + ex);
                await messagebox.ShowAsync();
            }
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            var logincredentials = new LoginCredentials();
            logincredentials.UserName = username.Text;
            this.Frame.Navigate(typeof(UserDashboard), logincredentials);
        }
    }
}
