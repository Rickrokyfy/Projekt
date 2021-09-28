using System;
using System.Net;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using AnalysisSupport;

namespace Csharp_base
{

    class collector
    {
        
        public class formatandprint
        {
            public int time;
            public void print(string thetext)
            {
                if ((thetext.Contains("btc") || thetext.Contains("bitcoin") || thetext.Contains("xbt")) && !thetext.Contains("i am a bot") && !thetext.Contains("i'm a bot") && !thetext.Contains("**Bitcoin(BTC) Basic Info:**"))
                {
                    //Open for writing
                    StreamWriter outfile = File.AppendText(@"C:\Users\01Ahl\Desktop\C#_projekt_mapp\collection");
                    //Remove excessive chars
                    thetext = thetext.Replace(".", " ");
                    thetext = thetext.Replace(",", " ");
                    thetext = thetext.Replace("!", " ");
                    thetext = thetext.Replace("?", " ");
                    thetext = thetext.Replace('\n', ' ');
                    //Remove stopwords
                    //foreach (string stopword in stopwords){
                    //thetext.Replace(" " + stopword + " ", " ");
                    //}
                    //Write to file
                    outfile.WriteLine(time + ","+thetext);
                    //Close after writing 
                    outfile.Close();
                }
            }
            
        }


        public static void Hanteraunderkommentarer(JObject jObject,  ref formatandprint fmprint)
        {
            //Hämta underkommentarsfält om möjligt
            if (jObject.GetValue("replies") != null)
            {
                //Försök läsa replies 
                if (jObject.GetValue("replies").ToString() != "")
                {

                    JObject underkomfalt = JObject.Parse(jObject.GetValue("replies").ToString());
                    //Console.WriteLine("underkomfalt = " +underkomfalt);
                    //Hämta datan om möjligt 
                    JObject commentdata = JObject.Parse(underkomfalt.GetValue("data").ToString());
                    //Hämta svaren
                    JArray replyfield = JArray.Parse(commentdata.GetValue("children").ToString());
                    //Gå igenom alla replies
                    JObject tempob;
                    string commenttext;
                    for (int i = 0; i < replyfield.Count(); i++)
                    {
                        //Hämta elementet på platsen som ett objekt
                        tempob = JObject.Parse(replyfield[i].ToString());
                        //OM underkommentarers data och namn kan brytas ut hämtas dessa
                        if (tempob.GetValue("data").ToString() != "")
                        {
                            //Hämta kommentarstext
                            if (JObject.Parse(tempob.GetValue("data").ToString()).GetValue("body") != null)
                            {

                                commenttext = JObject.Parse(tempob.GetValue("data").ToString()).GetValue("body").ToString();
                                //Analysera kommentarstexten
                                fmprint.print(commenttext);
                                
                                
                            }
                            //Om den har underkommentarer hanteras dessa
                            if (JObject.Parse(tempob.GetValue("data").ToString()).GetValue("replies") != null)
                            {
                                if (JObject.Parse(tempob.GetValue("data").ToString()).GetValue("replies").ToString() != "")
                                {
                                    Hanteraunderkommentarer(JObject.Parse(tempob.GetValue("data").ToString()), ref fmprint);
                                }

                                //Console.WriteLine(JObject.Parse(tempob.GetValue("data").ToString()).GetValue("score").ToString());
                            }



                        }

                    }
                }

            }
        }

        public static void process_post(string texttoanalyse,  ref formatandprint fmprint)
        {

            //Hämta ut objekten
            JArray asjson = JArray.Parse(texttoanalyse);
            //Hämta posttext

            //TODO HÄMTA UT OCH ANALYSERA POSTENS TEXT OM POSTEN HAR EN TEXT
            //POSTENS TEXT HITTAS GENOM: PLATS 1 I ARRAY, DATA, CHILDREN, DATA, SELFTEXT
            //Hämta info om post, posten är child till ngn arbiträr datastruktur tror jag
            JObject postob = JObject.Parse(asjson[0].ToString());
            JObject post_dat_ob = JObject.Parse(postob.GetValue("data").ToString());
            JArray post_ar_ob = JArray.Parse(post_dat_ob.GetValue("children").ToString());
            //Om posten finns
            if (post_ar_ob.ToString() != "[]")
            {
                JObject finalob = JObject.Parse(JObject.Parse(post_ar_ob[0].ToString()).GetValue("data").ToString());
                //Om detta objekt inehåller en selftext
                if (finalob.GetValue("selftext") != null)
                {

                }
            }


            //OBS! DENNA KOD ÄR SKIT PGA HAR MED MASSA EXTRACHECKAR, ÄNDRA BARA STARTVÄRDE FÖR LOOPEN TILL 1 så löser sig nog mycket
            //Hämta kommentarer
            JToken comments = asjson.Last;
            //Konvertera till Jsonobject
            JObject com_ob = JObject.Parse(comments.ToString());
            //Hämta ut infon i "data"
            JObject dat_ob = JObject.Parse(com_ob.GetValue("data").ToString());
            //Hämta ut info i  "children" 
            JArray ar_ob = JArray.Parse(dat_ob.GetValue("children").ToString());

            JToken temp;
            JObject tempob, dataob;
            string commenttext;
            //Läs genom alla kommentarer, dvs de arobs som är över 0, i kommentarsfältet
            //TODO TA BORT EXTRACHECKAR, DE FANNS BARA FÖR ATT FÖRUT HANTERADES INTE POSTEN SEPARAT VILKET GJORDE ATT SAKER KUKA UR
            for (int i = 0; i < ar_ob.Count; i++)
            {
                //Hämta det objekt som inehåller informationen om kommentaren
                temp = ar_ob[i];
                tempob = JObject.Parse(temp.ToString());

                //Hämta dataobjektet
                dataob = JObject.Parse(tempob.GetValue("data").ToString());
                if (dataob.ToString() != "")
                {
                    //Skriv score
                    //Console.WriteLine(dataob.GetValue("score").ToString());
                    //Hämta kommentarstexten om möjligt
                    if (dataob.ContainsKey("body"))
                    {
                        //Get and analyse text
                        commenttext = dataob.GetValue("body").ToString();
                        fmprint.print(commenttext);

                        //Get subcomments
                        Hanteraunderkommentarer(dataob,  ref fmprint);
                    }
                }
            }


        }
        public static string Get_uri(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static void api_get_posts(string posts, string reddit, string sluttid, ref formatandprint formatandprinter)
        {

            if (JObject.Parse(posts).GetValue("data").ToString() != "[]")
            {
                //Hämta ut dataobjektet
                string mellanhand = JObject.Parse(posts).GetValue("data").ToString();
                JArray jArray = JArray.Parse(mellanhand);
                //Gå igenom alla object i arrayen
                string urltemp;
                JObject tempob;
                //Gå igenom alla objekt i arrayen
                for (int i = 0; i < jArray.Count(); i++)
                {
                    //Hämta ut url koden
                    tempob = JObject.Parse(jArray[i].ToString());
                    urltemp = tempob.GetValue("full_link").ToString();
                    //Undersök denna urls post
                    process_post(Get_uri(urltemp += "/.json"),  ref formatandprinter);
                }
                //Hämta ny tid att undersöka,
                int newtime = int.Parse(JObject.Parse(jArray[jArray.Count() - 1].ToString()).GetValue("created_utc").ToString()) + 1;
                formatandprinter.time = newtime;
                //Undersök sista gammla tid +1
                string newuri = "https://api.pushshift.io/reddit/search/submission/?subreddit=" + reddit + "&sort=asc&sort_type=created_utc&after=" + newtime + "&before=" + sluttid + "&size=500";
                string newposts = Get_uri(newuri);
                Console.WriteLine("Getting new posts");
                Console.WriteLine("Newtime = " + newtime);
                api_get_posts(newposts, reddit, sluttid,  ref formatandprinter);
            }
            

        }
        static void Main(string[] args)
        {
            //Hämta subreddit
            Console.WriteLine("Vilken subreddit ska undersökas?");
            string reddit = Console.ReadLine();
            Console.WriteLine("Ange starttid för undersökning i unix tid");
            string starttid = Console.ReadLine();
            Console.WriteLine("Ange sluttid för undersökning i unix tid");
            string sluttid = Console.ReadLine();
            string sokning = "https://api.pushshift.io/reddit/search/submission/?subreddit=" + reddit + "&sort=asc&sort_type=created_utc&after=" + starttid + "&before=" + sluttid + "&size=500";
            //Hämta sidans text
            string output = Get_uri(sokning);
            //Skapa databasen
            formatandprint formatandprinter = new formatandprint();
            formatandprinter.time = int.Parse(starttid);
            //Gå igenom och hämta poster utifrån texten, dessa analyseras sedan mha databasen
            api_get_posts(output, reddit,  sluttid,  ref formatandprinter);

        }
    }

}