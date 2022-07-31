using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsiderScraper
{
    class Trader
    {
        private StockForecast sf;
        //percentage yield compared to omx
        private double totalPercentage;

        private double profitPercentage;

        public Trader(StockForecast sf)
        {
            this.sf = sf;
        }

        public StockForecast Sf
        {
            get { return sf; }
        }

        public void Simulation(EntryOverTime entry)
        {
            Console.WriteLine(entry.StartDate.AddDays(sf.MeanDevelopment.Item1).ToString("yyyy-MM-dd"));
            totalPercentage = 0;

            //get the omx value for the current Date for the start
            List<double> stockNOMX = Program.PythonCaller(entry.Företag, entry.StartDate.ToString("yyyy-MM-dd"), "GetDataForSingleDay.py");

            double startOmxPrice = stockNOMX[1];

            //get company share price for the start
            double startStockPrice = stockNOMX[0];

            for (DateTime i = entry.StartDate.AddDays(1); i <= entry.StartDate.AddDays(sf.MeanDevelopment.Item1) && totalPercentage < sf.MeanDevelopment.Item2; i = i.AddDays(1))
            {
                List<double> list = Program.PythonCaller(entry.Företag, i.ToString("yyyy-MM-dd"), "GetDataForSingleDay.py");

                //this is to check if the stockmarket is open
                if (list.Count > 0)
                {
                    //get the omx value for the current Date
                    double omxPrice = list[1];

                    //get company share price
                    double stockPrice = list[0];

                    //course development
                    double stockPercentage = (stockPrice / startStockPrice - 1) * 100;
                    double omxPercentage = (omxPrice / startOmxPrice - 1) * 100;

                    totalPercentage = stockPercentage - omxPercentage;
                    Console.WriteLine("kommer aldrig hända");
                }

                Console.WriteLine(i.ToString("yyyy-MM-dd"));
            }
            //sell
            profitPercentage += totalPercentage;
        }

        public override string ToString()
        {
            return String.Format("profitPercentage: {0}", profitPercentage);
        }
    }
}