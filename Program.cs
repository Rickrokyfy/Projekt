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
        public class Database
        {
            //Fält och antalet av de vanligaste orden
            public List<string> common = new List<string>();
            //Fält med possitiva nyckelord
            public List<string> positiva = new List<string>();
            //Fält med negativa nyckelord
            public List<string> negativa = new List<string>();
            //Överlagrad konstruktor
            public Database(string comword_file, string pos_word_file, string neg_word_file) 
            {
                //Läs och lägg till common words tills den filen är slut
                StreamReader comstream = new StreamReader(comword_file);
                string temp;
                while((temp = comstream.ReadLine())!=null && temp!="")
                {
                    common.Add(temp);
                }
                //Läs och lägg till possitiva ord
                StreamReader posstream = new StreamReader(pos_word_file);
                while((temp = posstream.ReadLine())!=null && temp != "")
                {
                    positiva.Append(temp);
                }
                //Läs och lägg till negativa ord
                StreamReader negstream = new StreamReader(neg_word_file);
                while((temp= negstream.ReadLine())!=null && temp != "")
                {
                    negativa.Append(temp);
                }
            }
        }
        
        public static void textanalys(string texten, ref Database databas)
        {
            //Konvertera texten till lower case
            string lower_text = texten.ToLower();
            //Om texten inehåller ett nyckelord relaterat till bitcoin och inte uppger att den är en bot undersöks den
            if ((lower_text.Contains("btc") || lower_text.Contains("bitcoin") || lower_text.Contains("xbt"))&&!lower_text.Contains("i am a bot") &&!lower_text.Contains("i'm a bot"))
            {
                //Sortera bort de vanligaste orden i engelska
                for(int i=0; i<databas.common.Count(); i++)
                {
                    lower_text =lower_text.Replace(" "+databas.common[i]+" ", " ");
                    lower_text = lower_text.Replace(" " + databas.common[i] + ".", " ");
                    lower_text = lower_text.Replace("." + databas.common[i] + " ", " ");
                }
                //Skriv ut kommentarstexten
                Console.WriteLine(lower_text);
            }


        }
        public static void Hanteraunderkommentarer(JObject jObject, ref int antalkommentarer, ref Database databas)
        {
            //Hämta underkommentarsfält om möjligt
            if(jObject.GetValue("replies")!=null)
            {
                //Försök läsa replies 
                if (jObject.GetValue("replies").ToString() != "")
                {
                    JObject underkomfalt = JObject.Parse(jObject.GetValue("replies").ToString());
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
                                textanalys(commenttext, ref databas);
                                //Console.WriteLine(commenttext);
                                antalkommentarer++;
                            }
                            //Om den har underkommentarer hanteras dessa
                            if (JObject.Parse(tempob.GetValue("data").ToString()).GetValue("replies") != null)
                            {
                                if (JObject.Parse(tempob.GetValue("data").ToString()).GetValue("replies").ToString() != "")
                                {
                                    Hanteraunderkommentarer(JObject.Parse(tempob.GetValue("data").ToString()), ref antalkommentarer, ref databas);
                                }
                                
                                //Console.WriteLine(JObject.Parse(tempob.GetValue("data").ToString()).GetValue("score").ToString());
                            }

                            

                        }

                    }
                }
                
            }
        }

        public static void process_post(string texttoanalyse, ref int antalkommentarer, ref Database databas)
        {
            
            //Hämta ut objekten
            JArray asjson = JArray.Parse(texttoanalyse);
            //Hämta posttext

            //TODO HÄMTA UT OCH ANALYSERA POSTENS TEXT OM POSTEN HAR EN TEXT
            //POSTENS TEXT HITTAS GENOM: PLATS 1 I ARRAY, DATA, CHILDREN, DATA, SELFTEXT
            //Hämta info om huvudkommentar
            JObject postob = JObject.Parse(asjson[0].ToString());
            JObject post_dat_ob = JObject.Parse(postob.GetValue("data").ToString());
            JArray post_ar_ob = JArray.Parse(post_dat_ob.GetValue("children").ToString());
            //Från post ar ob hämtas element 1 och des parameter ob,
            //Om posten är mer än bara en länk
            if(post_ar_ob.ToString()!="[]")
            {
                JObject finalob = JObject.Parse(JObject.Parse(post_ar_ob[0].ToString()).GetValue("data").ToString());
                //Om detta objekt inehåller en selftext
                if (finalob.GetValue("selftext") != null)
                {
                    Console.WriteLine("POSTTEXT " + finalob.GetValue("selftext").ToString());
                    string postbody = finalob.GetValue("selftext").ToString();
                    textanalys(postbody, ref databas);
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
                    //Skriv score
                    //Console.WriteLine(dataob.GetValue("score").ToString());
                    //Hämta kommentarstexten om möjligt
                    if (dataob.ContainsKey("body"))
                    {

                        commenttext = dataob.GetValue("body").ToString();
                        //Console.WriteLine(dataob.GetValue("score").ToString());
                        textanalys(dataob.GetValue("body").ToString(), ref databas);
                        //Console.WriteLine(commenttext);
                        //Hämta underkommentarer
                     
                        Hanteraunderkommentarer(dataob, ref antalkommentarer, ref databas);
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

        public static void api_get_posts(string uri, string reddit,  string sluttid, ref int total_processed_texts, ref Database databas)
        {
            //Om dataobjektet inte är tomt
            if (JObject.Parse(uri).GetValue("data").ToString()!="[]")
            {
                //Hämta ut dataobjektet
                string mellanhand = JObject.Parse(uri).GetValue("data").ToString();
                JArray jArray = JArray.Parse(mellanhand);
                //Öka antal hanterade texter
                total_processed_texts += jArray.Count();
                //Gå igenom alla object i arrayen
                string urltemp;
                JObject tempob;
                //Gå igenom alla objekt i arrayen
                for (int i = 0; i < jArray.Count(); i++)
                {
                    //Hämta ut url koden
                    tempob = JObject.Parse(jArray[i].ToString());
                    urltemp = tempob.GetValue("full_link").ToString();
                    Console.WriteLine(urltemp);
                    //Undersök denna urls post
                    process_post(Get_uri(urltemp += "/.json"), ref total_processed_texts, ref databas);
                }
                //Hämta ny tid att undersöka, sista gammla tid +1
                int newtime = int.Parse(JObject.Parse(jArray[jArray.Count() - 1].ToString()).GetValue("created_utc").ToString())+1;
                string newuri = "https://api.pushshift.io/reddit/search/submission/?subreddit=" + reddit + "&sort=desc&sort_type=created_utc&after=" + newtime + "&before=" + sluttid + "&size=1000";
                //Gör sökning men med ny tid
                api_get_posts(uri, reddit,  sluttid, ref total_processed_texts, ref databas);
            }
            //annars
            else
            {
                //Skriv ut antal hanterade texter
                Console.WriteLine(total_processed_texts);
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
            string sokning = "https://api.pushshift.io/reddit/search/submission/?subreddit=" +reddit+"&sort=desc&sort_type=created_utc&after="+ starttid +"&before="+ sluttid+"&size=1000";
            //Hämta sidans text
            string output = Get_uri(sokning);
            //Skapa databasen
            Database databas = new Database(@"C:\Users\01Ahl\Desktop\C#_projekt_mapp\com.txt", @"C:\Users\01Ahl\Desktop\C#_projekt_mapp\neg.txt", @"C:\Users\01Ahl\Desktop\C#_projekt_mapp\pos.txt");
            //Gå igenom och hämta poster utifrån texten, dessa analyseras sedan mha databasen
            int hanteradetexter = 0;
            api_get_posts(output, reddit, sluttid, ref hanteradetexter, ref databas);

        }
    }

}
