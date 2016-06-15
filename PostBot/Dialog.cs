using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using System.Configuration;
using PostBot;
using Microsoft.Bot.Builder.Luis.Models;

namespace PostBot
{
    [Serializable]
    public class Dialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        const string luisEndpoint = "https://api.projectoxford.ai/luis/v1/application?id=a87e779d-cd85-4ba5-af63-b2dc4622ad3b&subscription-key=40a189ba4f8a4824b4e3371ca059a82b";

        const string entity_Recipient = "recipient";
        const string entity_Set = "builtin.datetime.set";
        const string entity_Date = "builtin.datetime.date";
        const string entity_Money = "builtin.money";

        const string entity_schedule = "builtin.datetime.set";

        const string intent_reschedule = "RescheduleDelivery";
        const string intent_findStation = "FindStation";
        const string intent_hi = "hi";
        const string intent_deliveryState = "DeliveryState";
        const string intent_changeDelivery = "ChangeDelivery";

        const string set_year = "XXXX";
        const string set_month = "XXXX-XX";
        const string set_day = "XXXX-XX-XX";
        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var message = await argument;

            string resp = "";
            LUISModel model = await GetEntityFromLUIS(message.Text);

            if (model.intents.Count() > 0)
            {
                switch (model.intents[0].intent)
                {
                    case intent_hi:


                        await context.PostAsync("Hi! Try \"when will my package arrive?\"\n\nor: \"send my delivery to my neigbhour instead\"");

                        context.Wait(MessageReceivedAsync);
                        break;
                    case intent_deliveryState:
                        var start = "Graz";
                        var dest = "Am Europlatz 3, Vienna";
                        Message msg = new Message();
                        string query = "http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/Routes?wp.0=" + start + ";64;1&wp.1=" + dest + ";66;2&key=AgArU18mPZIHjbt9F0l5_StVXlcXITxAbGRYl07EEUzOLiXIRYLBzWAiBTeTYNcQ";
                        List<Attachment> a = new List<Attachment>();
                        a.Add(new Attachment(contentUrl: query, contentType: "image/jpeg"));

                        msg.Attachments = a;

                        string metaquery = "http://dev.virtualearth.net/REST/V1/Routes/Driving?o=json&wp.0=" + start + "&wp.1=" + dest + "&key=AgArU18mPZIHjbt9F0l5_StVXlcXITxAbGRYl07EEUzOLiXIRYLBzWAiBTeTYNcQ";
                        string s = new WebClient().DownloadString(metaquery);
                        dynamic meta = JsonConvert.DeserializeObject(s);
                        string seconds = meta.resourceSets[0].resources[0].travelDuration;
                        TimeSpan time = TimeSpan.FromSeconds(Double.Parse(seconds));
                        var arrivalTime = DateTime.Now.Add(time);
                        msg.Text = "Your package is currently in " + start + " and will arrive at " + arrivalTime.ToString(@"HH\:mm");


                        PostAndWait(context, msg);
                        break;
                    case intent_changeDelivery:

                        var hasDate = model.entities.Where(e => e.type == "builtin.datetime.time").FirstOrDefault();
                        if(hasDate != null)
                        {

                           
                            var parser = new Chronic.Parser();

                            var span = parser.Parse(hasDate.entity);
                            if (span != null)
                            {
                                var when = span.Start ?? span.End;
                                
                                if (when.Value.Hour < 18)
                                    PostAndWait(context, "Alright, I'll notify the driver and reschedule your delivery to " + when.Value.ToString(@"HH\:mm"));
                                else
                                {                         
                                    PostAndWait(context, MakeMessage("http://dev.virtualearth.net/REST/V1/Imagery/Map/Road/Am%20Europlatz%20Vienna?mapLayer=TrafficFlow&key=AgArU18mPZIHjbt9F0l5_StVXlcXITxAbGRYl07EEUzOLiXIRYLBzWAiBTeTYNcQ",
                                        "Sorry, that's not possible. We'll deliver the package to your nearest pickup station at Am Europlatz 3, Vienna"));
                                }


                                break;
                            }
                       
                        }   
                        
                        PostAndWait(context, "Delivery will be sent to Am Europlatz 3 instead");

                        break;

                    case intent_findStation:
                        Message m = new Message();
                        string q = "http://dev.virtualearth.net/REST/V1/Imagery/Map/Road/Am%20Europlatz%20Vienna?mapLayer=TrafficFlow&key=AgArU18mPZIHjbt9F0l5_StVXlcXITxAbGRYl07EEUzOLiXIRYLBzWAiBTeTYNcQ";
                        List<Attachment> attachments = new List<Attachment>();
                        attachments.Add(new Attachment(contentUrl: q, contentType: "image/jpeg"));

                        m.Attachments = attachments;
                        m.Text = "The nearest pickup station is Am Europlatz 3, Vienna";

                        PostAndWait(context, m);
                        break;
                    default:
                        PostAndWait(context, "Didn't get that");
                        break;
                }
            }
            else
            {
                PostAndWait(context, "Didn't get that");
            }

        }

        private static Message MakeMessage(string query, string text)
        {
            Message m = new Message();
             List<Attachment> attachments = new List<Attachment>();
            attachments.Add(new Attachment(contentUrl: query, contentType: "image/jpeg"));

            m.Attachments = attachments;
            m.Text = text;
            return m;
        }



        private async void PostAndWait(IDialogContext context, string resp)
        {

            await context.PostAsync(resp);

            context.Wait(MessageReceivedAsync);
        }
        private async void PostAndWait(IDialogContext context, Message resp)
        {

            await context.PostAsync(resp);

            context.Wait(MessageReceivedAsync);
        }

     
        private static async Task<LUISModel> GetEntityFromLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            LUISModel Data = new LUISModel();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = luisEndpoint + "&q=" + Query;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Data = JsonConvert.DeserializeObject<LUISModel>(JsonDataResponse);
                }
            }
            return Data;
        }
    }
}