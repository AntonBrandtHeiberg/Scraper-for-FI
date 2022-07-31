using System;
using System.Collections.Generic;
using System.Text;

namespace InsiderScraper
{
    class Entry
    {
        private double kurs;
        private double volym;
        private DateTime datum;
        private readonly string företag;

        public Entry(double kurs, double volym, DateTime datum, string företag)
        {
            this.kurs = kurs;
            this.volym = volym;
            this.datum = datum;
            this.företag = företag;
        }

        public double TotalVärde
        {
            get { return (double) Kurs*Volym; }
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

        public DateTime Datum
        {
            get { return datum; }
            set { this.datum = Datum; }
        }

        public string Företag
        {
            get { return företag; }
        }

        public override string ToString()
        {
            return String.Format("Datum: {0} | Värde: {1:### ### ###.##}kr | Kurs: {2:n}kr| Volym: {3:# ###} | Company Name: {4:N}", Datum.ToShortDateString(), TotalVärde, Kurs, Volym, företag);
        }
    }
}
