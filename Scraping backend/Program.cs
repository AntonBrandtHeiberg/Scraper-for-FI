using System;
using System.Net.Http;
using HtmlAgilityPack;
using System.Collections.Generic;


namespace Scraping_v1
{
    class Program
    {
        //these enumerations could be useful if we want to study Tilldelning and Avyttring too!
        enum köptyp
        {
            Förvärv, Tilldelning, Avyttring
        }

        static void Main(string[] args)
        {
            //here are all the important variables that dictate the rest of the code
            //all standard companies relevant to the study
            String[] företaglista = { "volvo", "electrolux", "ericsson", "H&M", "seb", "swedbank", "tele2", "skf", "skanska", "sandvik" };

            string företag = "nordea";
            string referenceDate = "2021 09 13";

            //map med köptyper:
            //pos 0 är datum
            //pos 2 är namn
            //pos 5 är avryttning eller tilldelning eller förvärv
            //pos 10 är antal, ex: 7 000 har Html 7&#160;000, dvs: "&#160;" representerar multiplikation med 1000 verkar det som
            //pos 12 är kurs
            //pos 13 är valuta
            Dictionary<string, int> colPairs = new Dictionary<string, int>();
            colPairs.Add("datum", 0);
            colPairs.Add("namn", 2);
            colPairs.Add("köptyp", 5);
            colPairs.Add("volym", 10);
            colPairs.Add("kurs", 12);
            colPairs.Add("valuta", 13);

            //sidor is the maximum amount of pages for that particular company
            int sidor = int.Parse(new HtmlAgilityPack.HtmlWeb().Load("https://marknadssok.fi.se/Publiceringsklient/sv-SE/Search/Search?SearchFunctionType=Insyn&Utgivare="
                    + företag +
                    "&PersonILedandeSt%C3%A4llningNamn=&Transaktionsdatum.From=&Transaktionsdatum.To=&Publiceringsdatum.From=&Publiceringsdatum.To=&button=search&Page=1"
                    ).DocumentNode.SelectSingleNode("/html/body/div[1]/div/div/div[2]/div/div/div[1]/ul/li[14]/a").InnerText.Substring(1));
            for (int i = 1; i <= sidor; i++)
            {
                var url = "https://marknadssok.fi.se/Publiceringsklient/sv-SE/Search/Search?SearchFunctionType=Insyn&Utgivare="
                    + företag +
                    "&PersonILedandeSt%C3%A4llningNamn=&Transaktionsdatum.From=&Transaktionsdatum.To=&Publiceringsdatum.From=&Publiceringsdatum.To=&button=search&Page=" +
                    i;
                //Time to surf the interwebs!!
                var web = new HtmlAgilityPack.HtmlWeb();
                var doc = web.Load(url);

                List<Entry> entryList = new List<Entry>();
                //Find every tablerow (tr) node via xPath
                foreach (var item in doc.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/div[1]/table[1]/tbody[1]/tr"))
                {
                    //every item is now a tablerow object from the table: class="table table-bordered table-hover table-striped zero-margin-top"
                    
                    //add every table data (td) to colList. Essentially, colList is a replica of a tablerow from the website
                    List<String> colList = new List<String>();
                    foreach (HtmlNode col in item.SelectNodes("td"))
                    {
                        //the innertext is the string from each box (td)
                        colList.Add(col.InnerText);
                    }

                    //as soon as we go back further than expected, break the process and go to the foreach down below
                    //I want the if-statement to be true ONLY if we've gone back too much!
                    string date = colList[colPairs["datum"]].Replace("-", " ");
                    if (CompareDates(date, referenceDate))
                    {
                        Console.WriteLine("break!!1!");
                        break;
                    }

                    //check if it's what were looking for, we don't want: tilldelning or avyttring!
                    if (colList[colPairs["köptyp"]].Equals("F&#246;rv&#228;rv"))
                    {
                        //okay this looks strange but it's fine, trust me! It creates a new entry and adds it to the entrylist but how I get the attributes is a bit cheesy
                        //colList is the tablerow (tr) and it contains lots of rows of table data (td). Every tabledata is a string
                        //colPairs is basically a map that allows me to easily get everything I want from the tr.
                        double kurs = Convert.ToDouble(colList[colPairs["kurs"]].Replace("&#160;", String.Empty));
                        double volym = Convert.ToDouble(colList[colPairs["volym"]].Replace("&#160;", String.Empty));
                        
                        Entry entry = new Entry(kurs, volym, date, 1.4);
                        entryList.Add(entry);
                    }
                }
                //print them
                foreach (Entry entry in entryList)
                {
                    Console.WriteLine(entry);
                }
            }
        }

        /// <summary>
        /// Compares <paramref name="date"/> to <paramref name="compareDate"/>
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if date occurs before compareDate
        /// </returns>
        public static bool CompareDates(string date, string compareDate)
        {
            //2022 02 03 compare 2022 02 04 (Date is lesser than compareDate so we want it to return true)
            return StringToDate(date) < StringToDate(compareDate);
        }

        /// <summary>
        /// Converts the <paramref name="date"/> type from <see langword="string"></see> to <see langword="DateTime"></see>
        /// </summary>
        /// <returns>
        /// The <see langword="DateTime"></see> equivalent of <paramref name="date"/>
        /// </returns>
        public static DateTime StringToDate(string date)
        {
            String[] stringArray = date.Split(" ");
            int year = int.Parse(stringArray[0]);
            int month = int.Parse(stringArray[1]);
            int day = int.Parse(stringArray[2]);

            return new DateTime(year, month, day);
        }
    }
}
