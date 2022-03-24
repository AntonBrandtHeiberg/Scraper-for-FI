using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InsiderScraping
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSök_Click(object sender, EventArgs e)
        {
            string företag = tbxFöretag.Text;
            string url = "https://marknadssok.fi.se/Publiceringsklient/sv-SE/Search/Search?SearchFunctionType=Insyn&Utgivare=" + företag + "&PersonILedandeSt%C3%A4llningNamn=&Transaktionsdatum.From=&Transaktionsdatum.To=&Publiceringsdatum.From=&Publiceringsdatum.To=&button=search&Page=1";
            
            //Time to surf the interwebs!!
            var web = new HtmlAgilityPack.HtmlWeb();
            var doc = web.Load(url);

            //Get to the good part hehe
            List<Persons> traders = new List<Persons>();
            var bitem = doc.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/div[1]/table[1]/tbody[1]/tr[1]");
            
            //Find every tr node from xPath
            foreach (var item in doc.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/div[1]/table[1]/tbody[1]/tr"))
            {
                //create an object with the scraped attributes as values
                Persons person1 = new Persons();
                person1.Name = item.SelectNodes("td[3]").ToString();
                person1.Volume = item.SelectNodes("td[11]").ToString();
                traders.Add(person1);
            }
        }
    }
}
