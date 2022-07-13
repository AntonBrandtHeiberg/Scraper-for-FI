using System;
using System.Collections.Generic;
using System.Text;

namespace Scraping_HTMLAgilityPack
{
    class EntryOverTime
    {
        private DateTime startDate;
        private string dateInterval;
        private DateTime endDate;

        private double kurs;
        private double volym;

        public EntryOverTime(DateTime startDate, string dateInterval, double kurs, double volym)
        {
            this.startDate = startDate;
            this.dateInterval = dateInterval;
            this.kurs = kurs;
            this.volym = volym;
        }

        public double Kurs
        {
            get { return kurs; }
            set { this.kurs = Kurs; }
        }

        public double Volym
        {
            get { return volym; }
            set { this.volym = Volym; }
        }

        public DateTime StartDate
        {
            get { return startDate; }
            set { this.startDate = StartDate; }
        }

        public string DateInterval
        {
            get { return dateInterval; }
            set { this.dateInterval = DateInterval; }
        }

        public DateTime EndDate
        {
            get { return startDate.AddYears(int.Parse(dateInterval.Split(' ')[0])).AddMonths(int.Parse(dateInterval.Split(' ')[1])).AddDays(int.Parse(dateInterval.Split(' ')[2])); }
            set { this.endDate = EndDate; }
        }
        public override string ToString()
        {
            string[] interval = dateInterval.Split(' ');
            return "datum: " + StartDate.ToShortDateString() + ", interval: " + interval[0] + "-" + interval[1] + "-" + interval[2] + ", end date: " + EndDate.ToShortDateString() + ", genomsnittskurs: " + kurs + ", totalvolym: " + String.Format("{0:# ###}", volym);
        }
    }
}