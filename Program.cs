using System;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using AnalysisSupport;

namespace Csharp_base
{
    
    class Program
    {
        //Sort a list of wordapperances
        public static void Sortdata(ref List<wordapperances> invest_list)
        {
            //Om det finns element att sortera
            if(invest_list.Count>1)
            {
                List<wordapperances> lagre = new List<wordapperances>();
                List<wordapperances> hog_eller_lik = new List<wordapperances>();
                //Gå igenom alla element från elementet efter index till antalelement
                for (int i = 1; i < invest_list.Count(); i++)
                {
                    //om de e färre förekomster än pivotet läggs det till i ena listan
                    if (invest_list[0].apperances > invest_list[i].apperances)
                    {
                        lagre.Add(invest_list[i]);
                    }
                    //Annars läggs det till i andra listan
                    else
                    {
                        hog_eller_lik.Add(invest_list[i]);
                    }
                }
                //För båda listorna, om de har mer än 1 element körs rekursiv analys på dem
                if (hog_eller_lik.Count() > 1)
                {
                    Sortdata(ref hog_eller_lik);
                }
                if (lagre.Count() > 1)
                {
                    Sortdata(ref lagre);
                }
                //Sätt listan som ska undersökas till att vara en kombination av de två skapade listorna och pivotet
                lagre.Add(invest_list[0]);
                lagre.AddRange(hog_eller_lik);
                invest_list = lagre;
            }
            

        }

        
        public static void textanalys(string texten, ref Database databas)
        {

            //Konvertera texten till lower case
            string lower_text = texten.ToLower();
            //Om texten inehåller ett nyckelord relaterat till bitcoin och inte uppger att den är en bot undersöks den
            if ((lower_text.Contains("btc") || lower_text.Contains("bitcoin") || lower_text.Contains("xbt"))&&!lower_text.Contains("i am a bot") &&!lower_text.Contains("i'm a bot"))
            {

                //Ta bort enterslag
                lower_text = lower_text.Replace("\n", " ");
                StreamReader inlas = File.OpenText(@"C:\Users\01Ahl\Desktop\C#_projekt_mapp\pos.txt");
                string temp;
                while((temp = inlas.ReadLine())!=null)
                {
                    if(lower_text.Contains(temp))
                    {
                        //Skriv texten som en rad i ett dokument
                        StreamWriter utfil = File.AppendText(@"C:\Users\01Ahl\Desktop\C#_projekt_mapp\pos_texts.txt");
                        utfil.WriteLine(lower_text);
                        utfil.Close();
                        break;
                    }
                }

                

                //Sortera bort de vanligaste orden i engelska 
                //Ersätt punkt, frågetecken, utropstecken och komma med " " för att göra parsing lättare
                lower_text = lower_text.Replace(".", " ");
                lower_text = lower_text.Replace(",", " ");
                lower_text = lower_text.Replace("!", " ");
                lower_text = lower_text.Replace("?", " ");
                for(int i=0; i<databas.common.Count(); i++)
                {
                    lower_text =lower_text.Replace(" "+databas.common[i]+" ", " ");
                    //lower_text = lower_text.Replace(" " + databas.common[i] + ".", " ");
                    //lower_text = lower_text.Replace("." + databas.common[i] + " ", " ");
                }



                List<string> storedwords = new List<string>();
                //Parsa till ordpar tills texten är slut
                while (lower_text!="")
                {
                    //Om det finns mer än ett ord kvar
                    if(lower_text.Substring(0, lower_text.Length).Contains(" "))
                    {
                        //Lägg till första ord
                        string word = lower_text.Substring(0, lower_text.IndexOf(" "));
                        //Ersätt lower_text med en ny text där ordet är exkluderat
                        lower_text = lower_text.Substring(lower_text.IndexOf(" ") + 1, lower_text.Length - lower_text.IndexOf(" ") - 1);
                        if (!storedwords.Contains(word))
                            storedwords.Add(word);
                    }
                    //Annars hantera bara sista ordet om det finns ord kvar
                    else
                    {
                        //TODO KOLLA HUR LENGTH FUNGERAR
                        if (!storedwords.Contains(lower_text))
                            storedwords.Add(lower_text);
                        lower_text = "";
                    }
                }
                //För varje ord i lagrade ord
                foreach(string word in storedwords)
                {
                    //Lägg till ordet i databasen
                    databas.addword(word);
                }

            }


        }
        public static void Hanteraunderkommentarer(JObject jObject, ref int antalkommentarer, ref Vaderbase vaderbas)
        {
            //Hämta underkommentarsfält om möjligt
            if(jObject.GetValue("replies")!=null)
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
                                vaderbas.analyse_text(commenttext);
                                //Console.WriteLine(commenttext);
                                antalkommentarer++;
                            }
                            //Om den har underkommentarer hanteras dessa
                            if (JObject.Parse(tempob.GetValue("data").ToString()).GetValue("replies") != null)
                            {
                                if (JObject.Parse(tempob.GetValue("data").ToString()).GetValue("replies").ToString() != "")
                                {
                                    Hanteraunderkommentarer(JObject.Parse(tempob.GetValue("data").ToString()), ref antalkommentarer, ref vaderbas);
                                }
                                
                                //Console.WriteLine(JObject.Parse(tempob.GetValue("data").ToString()).GetValue("score").ToString());
                            }

                            

                        }

                    }
                }
                
            }
        }

        public static void process_post(string texttoanalyse, ref int antalkommentarer, ref Vaderbase vaderbas)
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
            if(post_ar_ob.ToString()!="[]")
            {
                JObject finalob = JObject.Parse(JObject.Parse(post_ar_ob[0].ToString()).GetValue("data").ToString());
                //Om detta objekt inehåller en selftext
                if (finalob.GetValue("selftext") != null)
                {
                    //Gå igenom texten och analysera den
                    vaderbas.analyse_text(finalob.GetValue("selftext").ToString());
                    //Räkna upp antal kommentarer
                    antalkommentarer++;
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
                        //Get and analyse text
                        commenttext = dataob.GetValue("body").ToString();
                        vaderbas.analyse_text(commenttext);
                        
                        //Get subcomments
                        Hanteraunderkommentarer(dataob, ref antalkommentarer, ref vaderbas);
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

        public static void api_get_posts(string posts, string reddit, int behandladetexter,  string sluttid, ref int total_processed_texts, ref Vaderbase vaderbasen)
        {
            //Om dataobjektet inte är tomt
            //TODO FIXA HÄR

            if (JObject.Parse(posts).GetValue("data").ToString() !="[]")
            {
                //Hämta ut dataobjektet
                string mellanhand = JObject.Parse(posts).GetValue("data").ToString();
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
                    //Undersök denna urls post
                    process_post(Get_uri(urltemp += "/.json"), ref total_processed_texts, ref vaderbasen);
                }
                //Hämta ny tid att undersöka,
                int newtime = int.Parse(JObject.Parse(jArray[jArray.Count() - 1].ToString()).GetValue("created_utc").ToString()) + 1;
                //TODO, skriv ut första tid och summan av de såhär långt hanterade texternas tid
                double sum = 0;
                for (int i = behandladetexter; i < vaderbasen.measured_strings.Count(); i++)
                    sum += vaderbasen.measured_strings[i].Value;
                StreamWriter csv = File.AppendText(@"C:\Users\01Ahl\Desktop\C#_projekt_mapp\csv.txt");
                csv.WriteLine((int.Parse(sluttid)-newtime) +","+ sum);
                csv.Close();

                //Undersök sista gammla tid +1
                string newuri = "https://api.pushshift.io/reddit/search/submission/?subreddit=" + reddit + "&sort=asc&sort_type=created_utc&after=" + newtime + "&before=" + sluttid + "&size=500";
                string newposts = Get_uri(newuri);


                Console.WriteLine("Getting new posts");
                Console.WriteLine("Newtime = " + newtime);
                api_get_posts(newposts, reddit, vaderbasen.measured_strings.Count(), sluttid, ref total_processed_texts, ref vaderbasen);
                
            }
            //annars
            else
            {
                //Skriv ut antal hanterade texter
                Console.WriteLine("Antal hanterade texter = "+total_processed_texts);
                for (int i = 0; i < vaderbasen.measured_strings.Count(); i++)
                    Console.WriteLine(vaderbasen.measured_strings[i].ToString());
                //Sortera efter sentiment och skriv ut

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
            string sokning = "https://api.pushshift.io/reddit/search/submission/?subreddit=" +reddit+"&sort=asc&sort_type=created_utc&after="+ starttid +"&before="+ sluttid+"&size=500";
            //Hämta sidans text
            string output = Get_uri(sokning);
            //Skapa databasen
            Database databas = new Database(@"C:\Users\01Ahl\Desktop\C#_projekt_mapp\com.txt", @"C:\Users\01Ahl\Desktop\C#_projekt_mapp\neg.txt", @"C:\Users\01Ahl\Desktop\C#_projekt_mapp\pos.txt");
            Vaderbase vaderbasen = new Vaderbase(@"C:\Users\01Ahl\Desktop\C#_projekt_mapp\vader_sentiment.txt", @"C:\Users\01Ahl\Desktop\C#_projekt_mapp\com.txt");
            //Gå igenom och hämta poster utifrån texten, dessa analyseras sedan mha databasen
            int hanteradetexter = 0;
            api_get_posts(output, reddit, 0, sluttid, ref hanteradetexter, ref vaderbasen);

        }
    }

}
