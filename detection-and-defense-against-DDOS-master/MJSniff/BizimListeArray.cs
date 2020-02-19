using System;
using System.Collections.Generic;
using System.Linq;

namespace MJsniffer
{
    public class BizimListeArray
    {
        public List<BizimListe> Liste;

        public BizimListeArray()
        {
            Liste = new List<BizimListe>();
        }
        public void Ekle(BizimListe eklenecek)
        {
            if (Liste.Count < 20)
            {
                Liste.Add(eklenecek);
                
            }
            else
            {
                Liste = Liste.GetRange(1, 19);
                Liste.Add(eklenecek);
                
            }
        }

        public Tuple<bool,string> Sorgula()
        {

            foreach (var eleman in Liste)
            {
                if (Liste.Where(a => a.DestinationIp == eleman.DestinationIp).ToList().Count >= 10)
                    return Tuple.Create(true, eleman.DestinationIp);
            }

            return Tuple.Create(false, string.Empty);
        }
        
        
    }
}