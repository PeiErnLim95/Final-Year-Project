using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace FYPFinal
{
    class Analysis
    {
        public static void analysis(string[] args)
        {
            List<string> streamdata = new List<string>();
            var retrievetask = Task.Factory.StartNew(() => RetrieveData(0))
                .ContinueWith(prevTask => MakeRequest(2000, streamdata));
            retrievetask.Wait();
            Console.ReadLine();
        }

        public static void RetrieveData(int sleepTime)
        {
            //Connect to MongoDB using localhost (The MongoDB server is run locally, so are all the data stored)
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("TrainData");
            var collection = database.GetCollection<BsonDocument>("Sample1");
            List<string> streamdata = new List<string>();

            //Get only the TweetContent data
            var data = collection.Aggregate().Project<BsonDocument>(Builders<BsonDocument>.Projection.Exclude(find => find["_id"]).Include(find => find["TweetContent"])).ToList();

            //Store each of the data inside the MongoDB Collection into the generic list
            foreach (var d in data)
            {
                streamdata.Add(d["TweetContent"].AsString);
            }

            MakeRequest(2000, streamdata);
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
                            var sdatabase = mclient.GetDatabase("TrainData");
                            var scollection = sdatabase.GetCollection<BsonDocument>("Sample1");
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
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("MongoDB CRUD Process Error: " + ex);
                            Console.ReadLine();
                        }

                    }
                    else
                    {
                        MongoClient mclient = new MongoClient();
                        var mdatabase = mclient.GetDatabase("TrainData");
                        var mcollection = mdatabase.GetCollection<BsonDocument>("Sample1");
                        var filter = Builders<BsonDocument>.Filter.Eq("TweetContent", data);
                        var result = await mcollection.DeleteManyAsync(filter);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("LUIS Error: " + ex);
                    Console.ReadLine();
                }
            }
        }
    }
}
