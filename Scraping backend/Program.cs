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
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //here are all the important variables that dictate the rest of the code
            //all standard companies relevant to the study
            String[] företaglista = { "volvo", "electrolux", "ericsson", "H&M", "seb", "swedbank", "tele2", "skf", "skanska", "sandvik" };

            string företag = "nordea";
            DateTime startDate = StringToDate("2022 03 11");
            DateTime endDate = StringToDate("2022 03 11");

            //make sure startdate occurs before or on the same day as endDate
            if(startDate > endDate)
            {
                throw new Exception("startDate is larger than endDate");
            }

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
            Dictionary<string, int> tableRow = new Dictionary<string, int>
            {
                { "publiceringsdatum", 0 },
                { "namn", 2 },
                { "köptyp", 5 },
                { "datum", 9 },
                { "volym", 10 },
                { "kurs", 12 },
                { "valuta", 13 },
                { "status", 14 }
            };

            //sidor is the maximum amount of pages for that particular company and I get that int by scraping for the last available page
            int sidor = int.Parse(new HtmlAgilityPack.HtmlWeb().Load("https://marknadssok.fi.se/Publiceringsklient/sv-SE/Search/Search?SearchFunctionType=Insyn&Utgivare="
                    + företag +
                    "&PersonILedandeSt%C3%A4llningNamn=&Transaktionsdatum.From=&Transaktionsdatum.To=&Publiceringsdatum.From=&Publiceringsdatum.To=&button=search&Page=1"
                    ).DocumentNode.SelectSingleNode("/html/body/div[1]/div/div/div[2]/div/div/div[1]/ul/li[14]/a").InnerText.Substring(1));

            //check the if statment with publiceringsdatum for more context
            bool passedEndDate = false;

            //start going through the pages
            for (int i = 1; i <= sidor && !passedEndDate;  i++)
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
                    
                    //Essentially, table is a replica of the tablerow from the current webpage
                    List<String> table = new List<String>();
                    foreach (HtmlNode col in item.SelectNodes("td"))
                    {
                        //the innertext is the string from each box (td)
                        table.Add(col.InnerText);
                    }

                    //check if we need to go a page further, we don't need to go further if publiceringsdatum is 2 months after what we were looking for!
                    //2 months is ridiculous, I know but it happens sometimes appearently!
                    DateTime publiceringsDatum = StringToDate(table[tableRow["publiceringsdatum"]].Replace("-", " "));
                    if (publiceringsDatum < endDate && endDate.Subtract(publiceringsDatum) < new TimeSpan(1400000, 0, 0))
                    {
                        passedEndDate = true;
                    }

                    DateTime date = StringToDate(table[tableRow["datum"]].Replace("-", " "));
                    
                    //check if the date is relevant at all...
                    if(startDate <= date && date <= endDate)
                    {
                        //These are the requirments for what is relevant
                        if (table[tableRow["köptyp"]].Equals(köptyp["avyttring"]) && table[tableRow["valuta"]].Equals("SEK"))
                        {
                            //okay this looks strange but it's fine, trust me! It creates a new entry and adds it to the entrylist but how I get the attributes is a bit cheesy
                            //colList is the tablerow (tr) and it contains lots of rows of table data (td). Every tabledata is a string
                            //tableRow is basically a map that allows me to easily get everything I want from the tr. tableRow["name"] gives an int.
                            //&#160 needs to be removed because that represents a space
                            double kurs = Convert.ToDouble(table[tableRow["kurs"]].Replace("&#160;", String.Empty));
                            double volym = Convert.ToDouble(table[tableRow["volym"]].Replace("&#160;", String.Empty));

                            Entry entry = new Entry(kurs, volym, date, 1.4); //1.4 will in the future be the result from the python script
                            entryList.Add(entry);
                        }
                    }
                }

                //loop through entrylist and mash entries together according to dateInterval
                List<EntryOverTime> entryOverTimeList = EntryMerger(entryList, interval);

                //print them
                foreach (Entry entry in entryList)
                {
                    Console.WriteLine(entry);
                }
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
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
                        while(i+2 < entryList.Count && entryList[i+1].Datum.Equals(entryList[i+2].Datum))
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
