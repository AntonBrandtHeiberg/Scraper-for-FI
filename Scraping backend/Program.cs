using System;
using System.Net.Http;
using HtmlAgilityPack;
using System.Collections.Generic;


namespace Scraping_v1
{
    class Program
    {
        enum köptyp
        {
            Förvärv, Tilldelning, Avyttring
        }

        public static string compareDate = "2021 02 20";

        static void Main(string[] args)
        {
            //map med köptyper:
            Dictionary<string, int> köptyper = new Dictionary<string, int>();
            //köptyper.Add(förvärv, true)

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

            //all standard company's relevant to the study
            String[] företaglista = { "volvo", "electrolux", "ericsson", "H&M", "seb", "swedbank", "tele2", "skf", "skanska", "sandvik" };

            string standardFöretag = "saab";
            string företag = standardFöretag;
            int sidor = 5;
            string url;
            //for every url to sidor do this!
            for(int i = 1; i <= sidor; i++)
            {
                url = "https://marknadssok.fi.se/Publiceringsklient/sv-SE/Search/Search?SearchFunctionType=Insyn&Utgivare=" 
                    + företag + 
                    "&PersonILedandeSt%C3%A4llningNamn=&Transaktionsdatum.From=&Transaktionsdatum.To=&Publiceringsdatum.From=&Publiceringsdatum.To=&button=search&Page=" +
                    i;
                //Time to surf the interwebs!!
                var web = new HtmlAgilityPack.HtmlWeb();
                var doc = web.Load(url);

                //Get to the good part hehe
                List<Person> traders = new List<Person>();
                //create a docuemntnode for every person
                var nitem = doc.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/div[1]/table[1]/tbody[1]/tr[1]");

                Console.WriteLine(CompareDates("2022 02 03", "2022 02 04"));

                List<Entry> entryList = new List<Entry>();
                //Find every tr node from xPath
                foreach(var item in doc.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/div[1]/table[1]/tbody[1]/tr"))
                {
                    //every item is a tablerow object from the table: class="table table-bordered table-hover table-striped zero-margin-top"

                    List<String> colList = new List<String>();
                    //loop through every 
                    foreach(HtmlNode col in item.SelectNodes("td"))
                    {
                        //add innertext of every td item
                        colList.Add(col.InnerText);
                    }

                    //as soon as we go back further than expected, break the entire process.
                    //2021 11 18 kolla först om året är mindre, sedan månad, sedan dag
                    string datum = colList[colPairs["datum"]].Replace("-", " ");
                    //Console.WriteLine("Datum: " + colList[colPairs["datum"]].Replace("-", " "));

                    if (CompareDates(compareDate, datum))
                    {
                        Console.WriteLine("it returned true!");
                        break;
                    }
                    //check if it's what were looking for, we don't want: tilldelning or avyttring!
                    //Console.WriteLine(colList[colPairs["köptyp"]]);
                    if(colList[colPairs["köptyp"]].Equals("F&#246;rv&#228;rv")){
                        //okay this one looks weird but it's fine, trust me! It creates a new entry and adds it to the entrylist but how I get the attributes is a bit cheesy
                        //colList contains strings of every td and colPairs is basically a map to make sure I get the right stringindex for colList, makes sense?

                        double kurs = Convert.ToDouble(colList[colPairs["kurs"]].Replace(",", "."));
                        //Console.WriteLine("Kurs: " + Convert.ToDouble(colList[colPairs["kurs"]].Replace(",", ".")));
                        double volym = Convert.ToDouble(colList[colPairs["volym"]].Replace("&#160;", "").Replace(",", "."));
                        //Console.WriteLine("Volym: " + Convert.ToDouble(colList[colPairs["volym"]].Replace("&#160;", "")));
                        
                        entryList.Add(new Entry(kurs, volym, datum, 1.4));
                    }
                }
                //print them
                foreach(Entry entry in entryList)
                {
                    Console.WriteLine(entry);
                }
            }
        }
        /*returns true if the argument is lesser than compareDate*/
        public static bool CompareDates(string date, string compareDate)
        {
            //2022 02 03 compare 2022 02 04 (Date is lesser than compareDate so we want it to return true)
            return StringToDate(date) < StringToDate(compareDate);
        }

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
