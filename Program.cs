using System;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace Csharp_base
{
    class Program
    {
        
        public static void Hanteraunderkommentarer(JObject jObject, ref int antalkommentarer)
        {
            //Console.WriteLine(jObject.ToString());
            //Hämta underkommentarsfält om möjligt
            if(jObject.GetValue("replies")!=null)
            {
                //Försök läsa replies TODO Kan vara uttdaterad och överflödig
                if (jObject.GetValue("replies").ToString() != "")
                {
                    JObject underkomfalt = JObject.Parse(jObject.GetValue("replies").ToString());
                    //Hämta datan om möjligt 
                    JObject commentdata = JObject.Parse(underkomfalt.GetValue("data").ToString());
                    //Hämta svaren
                    JArray replyfield = JArray.Parse(commentdata.GetValue("children").ToString());
                    //Gå igenom alla replies
                    JObject tempob;
                    for (int i = 0; i < replyfield.Count(); i++)
                    {
                        //Hämta elementet på platsen som ett objekt
                        tempob = JObject.Parse(replyfield[i].ToString());
                        //OM underkommentarers data och namn kan brytas ut hämtas dessa
                        if (tempob.GetValue("data").ToString() != "")
                        {
                            if (JObject.Parse(tempob.GetValue("data").ToString()).GetValue("body") != null)
                            {

                                Console.WriteLine(JObject.Parse(tempob.GetValue("data").ToString()).GetValue("body").ToString());
                                antalkommentarer++;
                            }
                            //Om den har underkommentarer hanteras dessa
                            if (JObject.Parse(tempob.GetValue("data").ToString()).GetValue("replies") != null)
                            {
                                if (JObject.Parse(tempob.GetValue("data").ToString()).GetValue("replies").ToString() != "")
                                {
                                    Hanteraunderkommentarer(JObject.Parse(tempob.GetValue("data").ToString()), ref antalkommentarer);
                                }
                            }

                        }

                    }
                }
                
            }
        }

        public static void process_post(string texttoanalyse, ref int antalkommentarer)
        {
            //Hämta ut objekten
            JArray asjson = JArray.Parse(texttoanalyse);
            //Objekt nr 2 är av intresse
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
            //Läs genom alla kommentarer i kommentarsfältet
            antalkommentarer += ar_ob.Count;
            for (int i = 0; i < ar_ob.Count; i++)
            {
                //Hämta det objekt som inehåller informationen om kommentaren
                temp = ar_ob[i];
                tempob = JObject.Parse(temp.ToString());
                //Hämta dataobjektet
                dataob = JObject.Parse(tempob.GetValue("data").ToString());

                if (dataob.ToString() != "")
                {
                    //Hämta kommentarstexten om möjligt
                    if (dataob.ContainsKey("body"))
                    {

                        commenttext = dataob.GetValue("body").ToString();
                        Console.WriteLine(commenttext);
                        //Hämta underkommentarer
                     
                        Hanteraunderkommentarer(dataob, ref antalkommentarer);
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
        static void Main(string[] args)
        {
            //TODO HÄMTA 
            //Hämta ngt från reddit genom att ange uri
            //TODO VISSA SAKER KANSKE KRÄVER API NYCKEL
            Console.WriteLine("Ange uri: ");
            string uri = Console.ReadLine();
            string result = Get_uri(uri);
            //TODO HÄMTA ADRESSER TILL ETT ANTAL POSTER INNOM ETT GIVET TIDSINTERVALL GIVET SUBREDDIT
            int antalkommentarer = 0;
            process_post(result, ref antalkommentarer);
            Console.WriteLine(antalkommentarer);

        }
    }

}
