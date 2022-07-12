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
            return "datum: " + datum.ToShortDateString() + ", volym: " + String.Format("{0:# ###}", volym) + ", kurs: " + kurs + ", indexOMX: " + indexOMX;
        }
    }
}
