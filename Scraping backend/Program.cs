using System;
using System.Net.Http;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Scraping_HTMLAgilityPack
{
    class Program
    {
        //map with köptyper to make sure .Equals functions correctly
        public static Dictionary<string, string> köptyper = new Dictionary<string, string>
            {
                {"förvärv", "F&#246;rv&#228;rv" },
                {"avyttring", "Avyttring" },
                {"tilldelning", "Tilldelning" }
            };

        //all standard companies relevant to the study
        public static Dictionary<string, string> stocks = new Dictionary<string, string>
        {
            { "Pandox", "PNDX-B.ST" },
            { "Volvo", "VOLV-B.ST" },
            { "Electrolux", "ELUX-B.ST" },
            { "Ericsson", "ERIC-B.ST" },
            { "H&M", "HM-B.ST" },
            { "SEB", "SEB-A.ST" },
            { "Swedbank", "SWED-A.ST" },
            { "Tele2", "TEL2-B.ST" },
            { "SKF", "SKF-B.ST" },
            { "Sandvik", "SAND.ST" },
            { "Skanska", "SKA-B.ST" }
        };

        //map to help with traversing the tablerows
        //pos 0 är datum
        //pos 2 är namn
        //pos 5 är avryttning eller tilldelning eller förvärv
        //pos 10 är antal, ex: 7 000 har HTML 7&#160;000, dvs: "&#160;" representerar multiplikation med 1000 verkar det som
        //pos 12 är kurs
        //pos 13 är valuta
        public static Dictionary<string, int> tableRow = new Dictionary<string, int>
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

        //Program starts here
        public static void Main(string[] args)
        {
            //ska Entry och EntryOverTime ha företag, köptyp, valuta och status som attribut?
            Scrape_I_Fi("2021 07 24", "2022 07 24", new int[] { 0, 1, 0 }, "Pandox", "förvärv", "-", "-");
        }

        /// <summary>
        /// Prints results from scraping FI.
        /// <paramref name="startDate"/> and <paramref name="endDate"/> is expected to be in: "YYYY MM DD" and <paramref name="interval"/> in: {years, months, days}.
        /// The rest are "translated" using public dictionaries: <see langword="köptyper"/> and <see langword="NOTIMPLEMENTED"/>.
        /// <paramref name="köptyp"/>, <paramref name="valuta"/> and <paramref name="status"/> can all be replaced with "-" if all values are relevant
        /// </summary>
        /// <returns>
        /// 
        /// </returns>
        public static void Scrape_I_Fi(string stringStartDate, string stringEndDate, int[] interval, string företag, string köptyp, string valuta, string status)
        {
            //stopwatch for debugging purposes
            Stopwatch watch = Stopwatch.StartNew();

            DateTime startDate = StringToDate(stringStartDate);
            DateTime endDate = StringToDate(stringEndDate);

            //make sure startDate occurs before or on the same day as endDate
            if (startDate > endDate)
            {
                throw new ArgumentException("startDate is larger than endDate");
            }

            //the interval between the different EntryOverTime objects in years, months, days

            if (interval[0] == 0 && interval[1] == 0 && interval[2] == 0)
            {
                throw new ArgumentException("interval has to be atleast 1 day long {0, 0, 1}");
            }

            if (!köptyper.TryGetValue(köptyp, out string köptyperValue))
            {
                throw new ArgumentException(String.Format("köptyper does not contain a value for {0}", köptyp));
            }

            Console.WriteLine(string.Format("--- {0} ---", företag));

            //sidor is the maximum amount of pages for that particular company
            int sidor = int.Parse(new HtmlAgilityPack.HtmlWeb().Load("https://marknadssok.fi.se/Publiceringsklient/sv-SE/Search/Search?SearchFunctionType=Insyn&Utgivare="
                    + företag.ToLower() +
                    "&PersonILedandeSt%C3%A4llningNamn=&Transaktionsdatum.From=&Transaktionsdatum.To=&Publiceringsdatum.From=&Publiceringsdatum.To=&button=search&Page=1"
                    ).DocumentNode.SelectSingleNode("html/body/div[1]/div/div/div[2]/div/div/div[2]/span[3]").InnerText);

            //check the if statment with publiceringsdatum for more context
            bool passedEndDate = false;

            //list of entries that match a criteria
            List<Entry> entryList = new List<Entry>();

            //start going through the pages
            for (int i = 1; i <= sidor && !passedEndDate; i++)
            {
                Stopwatch entryWatch = Stopwatch.StartNew();

                //url updates with the side page
                string url = "https://marknadssok.fi.se/Publiceringsklient/sv-SE/Search/Search?SearchFunctionType=Insyn&Utgivare="
                    + företag.ToLower() +
                    "&PersonILedandeSt%C3%A4llningNamn=&Transaktionsdatum.From=&Transaktionsdatum.To=&Publiceringsdatum.From=&Publiceringsdatum.To=&button=search&Page=" +
                    i;

                //Time to surf the interwebs!!
                HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
                HtmlDocument doc = web.Load(url);
                entryWatch.Stop();
                Console.WriteLine(string.Format("\n->Entry Page {0}: {1}", i, (double) entryWatch.ElapsedMilliseconds / 1000));
                entryWatch.Restart();

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
                    //2 months is ridiculous, I know but it happens sometimes appearently! 1400000 is two months in hours
                    DateTime publiceringsDatum = StringToDate(table[tableRow["publiceringsdatum"]].Replace("-", " "));
                    if (publiceringsDatum < endDate && endDate.Subtract(publiceringsDatum) < new TimeSpan(1400000, 0, 0))
                    {
                        passedEndDate = true;
                    }

                    DateTime date = StringToDate(table[tableRow["datum"]].Replace("-", " "));

                    //check if the date is relevant at all...
                    if (startDate <= date && date <= endDate)
                    {
                        //following if statements make sure every requierment is met
                        bool pass = true;

                        //check köptyp
                        if (!table[tableRow["köptyp"]].Equals(köptyper[köptyp.ToLower()]) && !köptyp.Equals("-"))
                        {
                            pass = false;
                        }

                        //check valuta
                        if (!table[tableRow["valuta"]].Equals(valuta.ToUpper()) && !valuta.Equals("-"))
                        {
                            pass = false;
                        }

                        //check status
                        //this is necessary because status can't be: ""
                        //Conditional (ternary) expression: boolean ? true expression : false expression;
                        string entryStatus = table[tableRow["status"]].ToLower().Equals("") ? "-" : table[tableRow["status"]];

                        if (!entryStatus.Equals(status) && !entryStatus.Equals("-"))
                        {
                            pass = false;
                        }

                        if (pass)
                        {
                            //okay this looks strange but it's fine, trust me! It creates a new entry and adds it to the entrylist but how I get the attributes is a bit cheesy
                            //colList is the tablerow (tr) and it contains lots of rows of table data (td). Every tabledata is a string
                            //tableRow is basically a map that allows me to easily get everything I want from the tr. tableRow["name"] gives an int.
                            //&#160 needs to be removed because that represents a space (" ")
                            double kurs = Convert.ToDouble(table[tableRow["kurs"]].Replace("&#160;", String.Empty));
                            double volym = Convert.ToDouble(table[tableRow["volym"]].Replace("&#160;", String.Empty));

                            Entry entry = new Entry(kurs, volym, date, 1.4); //1.4 is a placeholder
                            entryList.Add(entry);
                        }
                    }
                }
            }

            //entrylist needs to be sorted before EntryMerger uses it!
            List<Entry> sortedEntryList = entryList.OrderBy(o => o.Datum).ToList();

            //print sortedEntryList
            Console.WriteLine("Entries: ");
            foreach (Entry entry in sortedEntryList)
            {
                Console.WriteLine(entry);
            }

            //loop through entrylist and mash entries together according to dateInterval
            List<EntryOverTime> entryOverTimeList = EntryMerger(sortedEntryList, startDate, interval);

            //print EntryOverTimeList
            Console.WriteLine("\nEntryOverTime Objects: ");
            foreach (EntryOverTime entry in entryOverTimeList)
            {
                Console.WriteLine(entry);
            }

            watch.Stop();
            Console.WriteLine(String.Format("\n-> Scraping Entries: {0} seconds", (double) watch.ElapsedMilliseconds / 1000));
            watch.Restart();

            //print result from python script
            Console.WriteLine("\n" + PythonScript(företag, startDate));

            watch.Stop();
            Console.WriteLine(String.Format("-> Call Python: {0} seconds", (double) watch.ElapsedMilliseconds / 1000));
        }

        /// <summary>
        /// Calls a python script using <paramref name="startDate"/>, <paramref name="endDate"/> and <paramref name="stockName"/>
        /// </summary>
        /// <returns>
        /// Lowest and highest index compared to OMX and the respective <see langword="Date"/> during given timeframe for the <paramref name="stockName"/>
        /// </returns>
        public static string PythonScript(string företag, DateTime startDate)//DateTime startDate, DateTime endDate, string stockName)
        {
            //make sure to have enviornmental variables PYTHONPATH and PATH set
            string pythonScriptPath = @"""C:\Users\Anton Brandt Heiberg\Documents\Python\yFinance.py""";

            //run python with Windows Command Prompt
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = "/c " + pythonScriptPath + " " + stocks[företag] + " " + startDate.ToString("yyyy-MM-dd")
                //Arguments = företag; //Set the argument(s) to the script; If more are required then add here
            };
            
            //variables for debugging
            string errors;
            string result;

            using (Process process = Process.Start(psi))
            {
                errors = process.StandardError.ReadToEnd();
                result = process.StandardOutput.ReadToEnd();
            }
            
            return result;
        }

        /// <summary>
        /// Merges objects from <paramref name="entryList"/> to a new list of <see langword="EntryOverTime"/> instances
        /// </summary>
        /// <returns>
        /// A new <see langword="List"/> of <see langword="EntryOverTime"/> instances
        /// </returns>
        public static List<EntryOverTime> EntryMerger(List<Entry> entryList, DateTime startDate, int[] interval)
        {
            List<EntryOverTime> entryOverTimeList = new List<EntryOverTime>();

            DateTime startPlusInterval = AddInterval(startDate, interval);

            //validInterval is the timeInterval object that validates whether an entry should be bunched into the EntryOverTime Object
            //it is another format for interval basically
            TimeSpan vaildInterval = startPlusInterval.Subtract(startDate) - new TimeSpan();

            //referenceDate is the start of the time frame for every merged entry
            DateTime referenceDate = startDate;

            //while loop to keep track of what interval is currently in use
            for (int i = 0; i < entryList.Count; i++) //i is the current item in the list
            {
                //check if the next item is within the right timeframe
                //(referenceDate <= entryList[i].Datum && entryList[i].Datum <= AddInterval(referenceDate, interval)
                while (entryList[i].Datum > referenceDate || entryList[i].Datum > AddInterval(referenceDate, interval))
                {
                    referenceDate = AddInterval(referenceDate, interval);
                }

                double totalVärde = entryList[i].TotalVärde;
                double totalVolym = entryList[i].Volym;
                double totalKurs = entryList[i].Kurs;

                int nItems = 1;

                //while loop that checks if there are more entries
                //checks that there are more entries and that 
                //also check if the next entry's date is such that: entrydate - startDate = interval <= validInterval
                while (i + 1 < entryList.Count && entryList[i + 1].Datum.Subtract(referenceDate) < vaildInterval)
                {
                    totalVärde += entryList[i + 1].TotalVärde;
                    totalVolym += entryList[i + 1].Volym;
                    totalKurs += entryList[i + 1].Kurs;
                    i++;
                    nItems++;
                }

                //make the entry a entryOverTime instance and add it to the list
                entryOverTimeList.Add(new EntryOverTime(totalVärde, referenceDate, interval, (totalKurs / nItems), totalVolym, 1.4));
                referenceDate = AddInterval(referenceDate, interval);
            }
            
            return entryOverTimeList;
        }

        /// <summary>
        /// Adds the <paramref name="interval"/> values <see langword="years"></see>, <see langword="months"></see> and <see langword="days"></see> to the <paramref name="date"/> an <paramref name="n"/> amount of times.
        /// </summary>
        /// <returns>
        /// A new <see langword="DateTime"></see> object
        /// </returns>
        public static DateTime AddInterval(DateTime date, int[] interval, int n = 1)
        {
            //adding 0 via AddYears, AddMonths or AddDays will throw error so these statements are there for that reason.
            if(interval[0] != 0)
            {
                date = date.AddYears(interval[0] * n);
            }
            if(interval[1] != 0)
            {
                date = date.AddMonths(interval[1] * n);
            }
            if(interval[2] != 0)
            {
                date = date.AddDays(interval[2] * n);
            }

            return date;
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
