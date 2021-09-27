using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace AnalysisSupport
{
    public class wordapperances
    {
        public string theword;
        public int apperances;
        public wordapperances(string ord)
        {
            apperances = 0;
            theword = ord;

        }
    }
    public class Database
    {
        //Fält och antalet av de vanligaste orden
        public List<string> common = new List<string>();
        //Fält med possitiva nyckelord
        public List<string> positiva = new List<string>();
        //Fält med negativa nyckelord
        public List<string> negativa = new List<string>();
        public List<wordapperances> all_words = new List<wordapperances>();
        //Överlagrad konstruktor
        public Database(string comword_file, string pos_word_file, string neg_word_file)
        {
            //Läs och lägg till common words tills den filen är slut
            StreamReader comstream = new StreamReader(comword_file);
            string temp;
            while ((temp = comstream.ReadLine()) != null && temp != "")
            {
                common.Add(temp);
            }
            //Läs och lägg till possitiva ord
            StreamReader posstream = new StreamReader(pos_word_file);
            while ((temp = posstream.ReadLine()) != null && temp != "")
            {
                positiva.Append(temp);
            }
            //Läs och lägg till negativa ord
            StreamReader negstream = new StreamReader(neg_word_file);
            while ((temp = negstream.ReadLine()) != null && temp != "")
            {
                negativa.Append(temp);
            }
        }
        public void addword(string newword)
        {
            //Gå igenom alla orden
            bool wordpresent = false;
            for (int i = 0; i < all_words.Count(); i++)
            {
                //Om ordet redan observerats
                if (newword == all_words[i].theword)
                {
                    wordpresent = true;
                    all_words[i].apperances++;
                }
            }
            //Om ordet inte fanns
            if (!wordpresent)
            {
                //Lägg till ordet
                all_words.Add(new wordapperances(newword));
                all_words[all_words.Count() - 1].apperances = 1;
            }
        }
    }

    //A class for storing and working with the vader database in relation to texts
    public class Vaderbase
    {
        public SortedList<string, double> Vaderwords = new SortedList<string, double>();
        public List<KeyValuePair<string, double>> measured_strings = new List<KeyValuePair<string, double>>();
        public List<string> stopwords = new List<string>();
        //Creates a vaderbase object from a filedirectory
        public Vaderbase(string vaderfiledirectory, string stopwords_dir)
        {

            StreamReader vaderfile = new StreamReader(vaderfiledirectory);
            //Read from vaderfile until eof
            string temp, word;
            double value;
            List<KeyValuePair<string, double>> temp_kvp = new List<KeyValuePair<string, double>>();
            while ((temp = vaderfile.ReadLine()) != null)
            {
                //First element is the word,
                word = temp.Substring(0, temp.IndexOf("\t"));
                //Second element is the value
                temp = temp.Substring(temp.IndexOf("\t") + 1);
                value = double.Parse(temp.Substring(0, temp.IndexOf("\t")).Replace(".", ","));
                //Add the value
                temp_kvp.Add(new KeyValuePair<string, double>(word, value));
            }
            foreach (KeyValuePair<string, double> kvp in temp_kvp)
            {
                Vaderwords.Add(kvp.Key, kvp.Value);
            }
            //Read stopwords to eof
            StreamReader stopwread = new StreamReader(stopwords_dir);
            while ((temp = stopwread.ReadLine()) != null)
            {
                stopwords.Add(temp);
            }
            //Sort the stopwords
            stopwords.Sort();
        }

        //If the word is a vaderword, return that words sentiment value, else return 0
        public double wordvalue(string word)
        {
            //Try to find the word using binary search, 
            int index = Vaderwords.IndexOfKey(word);
            if (index > -1)
            {
                return (Vaderwords.Values[index]);
            }
            else
                return 0;
        }

        public void analyse_text(string texten)
        {
            //Konvertera texten till lower case
            string lower_text = texten.ToLower();
            //Om texten inehåller ett nyckelord relaterat till bitcoin och inte uppger att den är en bot undersöks den
            if ((lower_text.Contains("btc") || lower_text.Contains("bitcoin") || lower_text.Contains("xbt")) && !lower_text.Contains("i am a bot") && !lower_text.Contains("i'm a bot"))
            {
                string basetext = lower_text;
                //Ta bort enterslag
                lower_text = lower_text.Replace("\n", " ");
                string temp;

                //Ersätt punkt, frågetecken, utropstecken och komma med " " för att göra parsing lättare
                lower_text = lower_text.Replace(".", " ");
                lower_text = lower_text.Replace(",", " ");
                lower_text = lower_text.Replace("!", " ");
                lower_text = lower_text.Replace("?", " ");
                //Ta bort stopwords
                for (int i = 0; i < stopwords.Count(); i++)
                {
                    lower_text = lower_text.Replace(" " + stopwords[i] + " ", " ");
                }
                double sentimentvalue = 0;
                //Parsa till ord tills texten är slut
                while (lower_text != "")
                {
                    //Om det finns mer än ett ord kvar
                    if (lower_text.Substring(0, lower_text.Length).Contains(" "))
                    {
                        //Hämta ut ord
                        string word = lower_text.Substring(0, lower_text.IndexOf(" "));
                        sentimentvalue += wordvalue(word);
                        //Ersätt lower_text med en ny text där ordet är exkluderat
                        lower_text = lower_text.Substring(lower_text.IndexOf(" ") + 1, lower_text.Length - lower_text.IndexOf(" ") - 1);

                    }
                    //Annars hantera bara sista ordet
                    else
                    {
                        sentimentvalue += wordvalue(lower_text);
                        lower_text = "";
                    }
                }
                //Lägg till ett nytt namn-värde för den mätta strängen och dess sentimentvärde till de mätta strängarna
                measured_strings.Add(new KeyValuePair<string, double>(basetext, sentimentvalue));
            }
        }

    }
}
