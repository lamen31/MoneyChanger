using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyChanger.Data
{
    public class Money
    {
        public string _moneytipe { get; set; }
        public string _moneyvalue { get; set; }


        public void setmoney(string strmoneytipe, string strmoneyvalue)
        {
            _moneytipe = strmoneytipe;
            _moneyvalue = strmoneyvalue;
        }
    }
}
