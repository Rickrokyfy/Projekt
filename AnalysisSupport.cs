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
}
