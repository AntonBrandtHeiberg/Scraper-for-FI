using System;
using System.Net.Http;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace Scraping_HTMLAgilityPack
{
    class Program
    {
        static void Main(string[] args)
        {
            //here are all the important variables that dictate the rest of the code
            //all standard companies relevant to the study
            String[] företaglista = { "volvo", "electrolux", "ericsson", "H&M", "seb", "swedbank", "tele2", "skf", "skanska", "sandvik" };

            string företag = "nordea";
            DateTime startDate = StringToDate("2021 09 08");
            DateTime endDate = StringToDate("2021 09 08");
            string interval = "0 0 1";

            //map with köptyper to make sure the equals function functions correctly
            Dictionary<string, string> köptyp = new Dictionary<string, string>
            {
                {"förvärv", "F&#246;rv&#228;rv" },
                {"avyttring", "Avyttring" },
                {"tilldelning", "Tilldelning" }
            };

            //map to help with traversing the tablerows
            //pos 0 är datum
            //pos 2 är namn
            //pos 5 är avryttning eller tilldelning eller förvärv
            //pos 10 är antal, ex: 7 000 har Html 7&#160;000, dvs: "&#160;" representerar multiplikation med 1000 verkar det som
            //pos 12 är kurs
            //pos 13 är valuta
            Dictionary<string, int> colPairs = new Dictionary<string, int>
            {
                { "publiceringsdatum", 0 },
                { "namn", 2 },
                { "köptyp", 5 },
                { "datum", 9 },
                { "volym", 10 },
                { "kurs", 12 },
                { "valuta", 13 }
            };

            //sidor is the maximum amount of pages for that particular company and I get that int by scraping for the last available page
            int sidor = int.Parse(new HtmlAgilityPack.HtmlWeb().Load("https://marknadssok.fi.se/Publiceringsklient/sv-SE/Search/Search?SearchFunctionType=Insyn&Utgivare="
                    + företag +
                    "&PersonILedandeSt%C3%A4llningNamn=&Transaktionsdatum.From=&Transaktionsdatum.To=&Publiceringsdatum.From=&Publiceringsdatum.To=&button=search&Page=1"
                    ).DocumentNode.SelectSingleNode("/html/body/div[1]/div/div/div[2]/div/div/div[1]/ul/li[14]/a").InnerText.Substring(1));

            //when I've passed the desired end date it'll finish reading all the entries from the current page and then after that stop

            //start going through the pages
            for (int i = 1; i <= sidor;  i++)
            {
                //url updates with the side page
                var url = "https://marknadssok.fi.se/Publiceringsklient/sv-SE/Search/Search?SearchFunctionType=Insyn&Utgivare="
                    + företag +
                    "&PersonILedandeSt%C3%A4llningNamn=&Transaktionsdatum.From=&Transaktionsdatum.To=&Publiceringsdatum.From=&Publiceringsdatum.To=&button=search&Page=" +
                    i;

                //Time to surf the interwebs!!
                var web = new HtmlAgilityPack.HtmlWeb();
                var doc = web.Load(url);

                List<Entry> entryList = new List<Entry>();
                
                //Find every tablerow (tr) node via this xPath expression
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
                    DateTime date = StringToDate(colList[colPairs["datum"]].Replace("-", " "));
                    
                    //check if the date is relevant at all...
                    if(startDate <= date && date <= endDate)
                    {
                        //check if it's what were looking for, in this case: förvärv
                        if (colList[colPairs["köptyp"]].Equals(köptyp["förvärv"])) // förvärv can be changed to a string that decides what köptyp we're looking for
                        {
                            //okay this looks strange but it's fine, trust me! It creates a new entry and adds it to the entrylist but how I get the attributes is a bit cheesy
                            //colList is the tablerow (tr) and it contains lots of rows of table data (td). Every tabledata is a string
                            //colPairs is basically a map that allows me to easily get everything I want from the tr. colPairs["name"] gives an int.
                            //&#160 needs to be removed because that represents a space
                            double kurs = Convert.ToDouble(colList[colPairs["kurs"]].Replace("&#160;", String.Empty));
                            double volym = Convert.ToDouble(colList[colPairs["volym"]].Replace("&#160;", String.Empty));

                            Entry entry = new Entry(kurs, volym, date, 1.4); //1.4 will in the future be the result from the python script
                            entryList.Add(entry);
                        }
                    }
                }

                //loop through entrylist and mash entries together according to dateInterval
                //List<EntryOverTime> entryOverTimeList = EntryMerger(entryList, interval);

                //print them
                foreach (Entry entry in entryList)
                {
                    Console.WriteLine(entry);

                    //I need a method that prints only the relevant ones

                }
            }
        }

        /// <summary>
        /// Merges objects from <paramref name="entryList"/> to a new list of <see langword="EntryOverTime"/> instances
        /// </summary>
        /// <returns>
        /// a list of <see langword="EntryOverTime"/> instances
        /// </returns>
        public static List<EntryOverTime> EntryMerger(List<Entry> entryList, string interval)
        {
            List<EntryOverTime> entryOverTimeList = new List<EntryOverTime>();

            //look trough entries
            for(int i = 0; i < entryList.Count; i++)
            {
                //see if a next item exists
                if(i+1 < entryList.Count)
                {
                    double totalVolym = entryList[i].Volym;
                    double totalKurs = entryList[i].Kurs;

                    //check if the next item has the same date
                    if (entryList[i].Datum.Equals(entryList[i+1].Datum))
                    {
                        //they belong in the same entry but we gotta check if the next one ALSO matches
                        totalVolym = entryList[i].Volym + entryList[i+1].Volym;
                        totalKurs = (entryList[i].Kurs + entryList[i+1].Kurs) / 2;

                        //while loop that checks if there are more entries and if the next entry's date matches the previous
                        while(i+1 < entryList.Count && entryList[i+1].Datum.Equals(entryList[i+2].Datum))
                        {
                            i++;
                            totalVolym += entryList[i+1].Volym;
                            totalKurs = (totalKurs + entryList[i+1].Kurs) / 2;
                        }
                    }
                    //make the entry a entryOverTime instance and add it to the list
                    entryOverTimeList.Add(new EntryOverTime(entryList[i].Datum, interval, totalKurs, totalVolym));
                }
            }
            return entryOverTimeList;
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
