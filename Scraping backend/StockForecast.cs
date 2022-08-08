using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsiderScraper
{
    class StockForecast
    {
        private List<EntryOverTime> entryList;
        private (int, double) meanDevelopment;

        public StockForecast(List<EntryOverTime> entryList, string företag)
        {
            this.entryList = entryList;

            meanDevelopment = StockMeanValues();
        }

        public (int, double) MeanDevelopment
        {
            get { return meanDevelopment; }
        }

        public List<EntryOverTime> EntryList
        {
            get { return entryList; }
            set { entryList = value; }
        }

        public (int, double) StockMeanValues()
        {
            List<Double> result = Program.PythonCaller(entryList[0].Företag, MakeDateString(EntryList), "StockGather.py");

            return ((int) result[1], result[0]);
        }

        public static string MakeDateString(List<EntryOverTime> entryList)
        {
            string allDates = String.Empty;
            foreach (EntryOverTime entry in entryList)
            {
                allDates += entry.StartDate.ToString("yyyy-MM-dd") + "Q";
            }
            allDates = allDates.Replace(" ", string.Empty);
            allDates = allDates.Remove(allDates.Length - 1);

            return allDates;
        }

        public override string ToString()
        {
            return String.Format("{0:0.##}% development after {1} days", meanDevelopment.Item2, meanDevelopment.Item1);
        }
    }
}
