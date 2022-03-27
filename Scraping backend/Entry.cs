using System;
using System.Collections.Generic;
using System.Text;

namespace Scraping_v1
{
    class Entry
    {
        private double kurs;
        private double volym;
        private string datum;
        private double indexOMX;

        public Entry(double kurs, double volym, string datum, double indexOMX){
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

        public string Datum
        {
            get { return datum; }
            set { this.volym = Volym; }
        }

        public double IndexOMX
        {
            get { return indexOMX; }
            set { this.indexOMX = IndexOMX; }
        }

        public override string ToString()
        {
            return "datum: " + datum + ", volym: " + volym + ", kurs: " + kurs + ", indexOMX: " + indexOMX;
        }
    }
}
