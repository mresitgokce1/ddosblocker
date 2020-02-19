using System;

namespace MJsniffer
{
    public class BizimListe
    {
        public string SourceIP { get; set; }

        public string DestinationIp { get; set; }

        public DateTime ZamanDateTime { get; set; }


        public BizimListe(string sourceIp, string destinationIp, DateTime zamanDateTime)
        {
            SourceIP = sourceIp;
            DestinationIp = destinationIp;
            ZamanDateTime = zamanDateTime;
        }
    }
}