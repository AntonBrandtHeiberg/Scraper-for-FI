using System;
using System.Collections.Generic;
using System.Text;

namespace Scraping_HTMLAgilityPack
{
    class Entry
    {
        private double kurs;
        private double volym;
        private DateTime datum;
        private double indexOMX;

        public Entry(double kurs, double volym, DateTime datum, double indexOMX)
        {
            this.kurs = kurs;
            this.volym = volym;
            this.datum = datum;
            this.indexOMX = indexOMX;
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

        public double IndexOMX
        {
            get { return indexOMX; }
            set { this.indexOMX = IndexOMX; }
        }

        public override string ToString()
        {
            return String.Format("Datum: {0} | Värde: {1:# ###.##}kr | Kurs: {2:n}kr| Volym: {3:# ###} | OMX Index: {4:N}", Datum.ToShortDateString(), TotalVärde, Kurs, Volym, IndexOMX);
        }
    }
}
