using System;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;


namespace Csharp_base
{
    class Program
    {
        
        public static void Hanteraunderkommentarer(JObject jObject)
        {
            //Console.WriteLine(jObject.ToString());
            //Hämta underkommentarsfält om möjligt
            if(jObject.GetValue("replies").ToString()!="")
            {
                try
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
                                Console.WriteLine(JObject.Parse(tempob.GetValue("data").ToString()).GetValue("body").ToString());
                        }

                    }
                }catch(Newtonsoft.Json.JsonReaderException)
                { Console.WriteLine(jObject.ToString());
                    System.Environment.Exit(1);
                }
            }
            


          





        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string text = System.IO.File.ReadAllText(@"C:\Users\01Ahl\source\repos\Csharp_base\input.txt");
            //Hämta ut objekten
            JArray asjson = JArray.Parse(text);
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
            JArray tempar;
            string commenttext;
            //Läs genom alla kommentarer i kommentarsfältet
            for (int i = 0; i < ar_ob.Count; i++)
            {
                //Hämta det objekt som inehåller informationen om kommentaren
                temp = ar_ob[i];
                tempob = JObject.Parse(temp.ToString());
                //Hämta dataobjektet
                dataob = JObject.Parse(tempob.GetValue("data").ToString());
                
                if(dataob.ToString()!="")
                {
                    //Hämta kommentarstexten om möjligt
                    if(dataob.ContainsKey("body"))
                    {

                        commenttext = dataob.GetValue("body").ToString();
                        Console.WriteLine(commenttext);
                        //Hämta kommentarsfältet
                        //tempar = JArray.Parse(dataob.GetValue("children").ToString());
                        //Hämta underkommentarer
                        Hanteraunderkommentarer(dataob);
                    }
                }
            }


        }
    }

}
