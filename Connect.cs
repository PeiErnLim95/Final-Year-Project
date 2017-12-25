using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Tweetinvi;
using Tweetinvi.Models;

namespace FYPFinal
{
    class Connect
    {
        public static void ConnectSchedule()
        {
            List<string> keyList = new List<string>();
            var connecttask = Task.Factory.StartNew(() => GetKeyword(1000)).ContinueWith(prevTask => Connecting(2000, keyList));
            connecttask.Wait();
        }

        public static void Connecting(int sleepTime, List<string> keyList)
        {
            //Set the token that provided by Twitter to gain authorized access into Twitter database
            Auth.SetUserCredentials("YTNuoC9rrJs8g9kZ0hRweKrpp", "wXj6VSl68jeFStRWHDnhG19oP1WZGeBFMNgT3KCkI6MaX46SMT", "892680922322960384-8ka1NuhgiuxjSLUffQVdwmnOIbIduZa", "y92ycGrGCJS9vBJU79gq34rV6FCwNjBPFFOqhEHaTQe1l");

            //Create stream with filter stream type
            var stream = Stream.CreateFilteredStream();
            //Set language filter to English only
            stream.AddTweetLanguageFilter(LanguageFilter.English);
            //Connect to database that stored the keyword
            foreach (var key in keyList)
            {
                stream.AddTrack(key);
            }
            //Let the stream match with all the conditions stated above
            stream.MatchingTweetReceived += (sender, argument) =>
            {
                //Connect to MongoDB server and database
                var tweet = argument.Tweet;
                try
                {
                    var client = new MongoClient();
                    var database = client.GetDatabase("StreamData");
                    var collection = database.GetCollection<BsonDocument>("StreamData");
                    //Exclude any Retweeted Tweets
                    if (tweet.IsRetweet != false) return;
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
                    collection.InsertOneAsync(tweetdata);
                }
                //Catch if any exception/errors occured
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.ReadLine();
                }
            };
            //Start the stream
            stream.StartStreamMatchingAllConditions();
        }

        public static void GetKeyword(int sleepTime)
        {
            //Connect to MongoDB using localhost (The MongoDB server is run locally, so are all the data stored)
            var client1 = new MongoClient("mongodb://localhost:27017");
            var database1 = client1.GetDatabase("KeywordList");
            var collection1 = database1.GetCollection<BsonDocument>("Keyword");

            //Get only the TweetContent data
            var data = collection1.Aggregate().Project<BsonDocument>(Builders<BsonDocument>.Projection.Exclude(find => find["_id"]).Include(find => find["Keyword"])).ToList();

            //Create a generic list object
            List<string> keywordList = new List<string>();

            //Store each of the data inside the MongoDB Collection into the generic list
            foreach (var d in data)
            {
                keywordList.Add(d["Keyword"].AsString);
            }

            Connecting(0, keywordList);
        }
    }
}
