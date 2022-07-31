using System;
using System.Collections.Generic;
using System.Text;

namespace InsiderScraper
{
    class EntryOverTime
    {
        private DateTime startDate;
        private int[] dateInterval;

        private double kurs;
        private double volym;
        private string företag;
        private double totalVärde;

        public EntryOverTime(double totalVärde, DateTime startDate, int[] dateInterval, double kurs, double volym, string företag)
        {
            this.totalVärde = totalVärde;
            this.startDate = startDate;
            this.dateInterval = dateInterval; //do I really need dateInterval...?
            this.kurs = kurs;
            this.volym = volym;
            this.företag = företag;
        }
        public double TotalVärde
        {
            get { return totalVärde; }
            set { this.totalVärde = TotalVärde; }
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

        public string Företag
        {
            get { return företag; }
        }

        public DateTime StartDate
        {
            get { return startDate; }
            set { this.startDate = StartDate; }
        }

        public int[] DateInterval
        {
            get { return dateInterval; }
            set { this.dateInterval = DateInterval; }
        }

        public DateTime EndDate
        {
            get { return Program.AddInterval(startDate, dateInterval); }
        }
        public override string ToString()
        {
            return String.Format("Datum: {0} | Totalt Värde: {1:### ### ###.##}kr | Genomsnittskurs: {2:n}kr | Totalvolym: {3:# ###} | Company Name: {4:n}", StartDate.ToShortDateString(), TotalVärde, Kurs, Volym, Företag);
        }
    }
}
