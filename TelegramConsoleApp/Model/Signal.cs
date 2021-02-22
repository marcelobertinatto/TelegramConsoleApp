using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramConsoleApp.Model
{
    public class Signal
    {
        public long MessageId { get; set; }
        public string Currency { get; set; }
        public string Time { get; set; }
        public string CurrencyTime { get; set; }
        public string CurrencySignal { get; set; }
        public string Date { get; set; }
        public string CurrencyAssertPercentage1 { get; set; }
        public string CurrencyAssertPercentage2 { get; set; }
        public string CurrencyAssertPercentage3 { get; set; }
        public Dictionary<DateTime,string> BackTest { get; set; }
       
    }
}
