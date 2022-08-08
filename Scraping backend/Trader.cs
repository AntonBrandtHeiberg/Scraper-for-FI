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
        private double averageOMX = 0;
        private int averageDays = 0;
        private double averageProfit = 0;

        public Trader(StockForecast sf)
        {
            this.sf = sf;
        }

        public StockForecast Sf
        {
            get { return sf; }
        }

        public void Simulation(List<EntryOverTime> entryList)
        {
            //get the omx value for the current Date for the start
            List<double> result = Program.PythonCaller(sf.EntryList[0].Företag, StockForecast.MakeDateString(entryList), 
                "EntryAnalyzer.py", 
                sf.MeanDevelopment.Item1.ToString() + " " + sf.MeanDevelopment.Item2
            );

            //foreach(double d in result)
            //{
            //    Console.WriteLine(d);
            //}

            GenerateAverage(result);
        }

        private void GenerateAverage(List<double> list)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if(i % 3 == 0)
                {
                    //OMX Growth
                    averageOMX += list[i];
                }
                else if(i % 3 == 1)
                {
                    //Average Days
                    averageDays += (int) list[i];
                }
                else
                {
                    //Stock Growth
                    averageProfit += list[i];
                }
            }

            averageOMX /= (list.Count / 3);
            averageDays /= (list.Count / 3);
            averageProfit /= (list.Count / 3);
        }

        public override string ToString()
        {
            return String.Format("Average OMX Growth: \n{0} | Average Stock Growth {1} | Average days until selling: {2}\n", averageOMX, averageProfit, averageDays);
        }
    }
}
