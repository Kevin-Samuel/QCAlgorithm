using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Securities;
using QuantConnect.Models;

namespace QuantConnect
{
    public class BasicTemplateAlgorithm : QCAlgorithm
    {
        public override void Initialize()
        {
            SetStartDate(2014, 5, 1);
            SetEndDate(2014, 5, 30);
            SetCash(100000);
            SetRunMode(RunMode.Series);

            //AddSecurity(SecurityType.Equity, "SPY", resolution: Resolution.Minute);
            AddData<Weather>("NYCTEMP");
        }
        public void OnData(Weather data)
        {
            //Test using weather as a tradable security:
            if (!Portfolio.Invested)
            {
                Order("NYCTEMP", (int)(Portfolio.Cash / data.MeanC));
                Console.WriteLine("Buying Weather: " + data.MeanC);
            }
        }
    }


    ///Our custom data/security class: Weather:
    public class Weather : BaseData
    {
        public decimal MaxC;
        public decimal MeanC;
        public decimal MinC;
        public string errString;
        private int lineCount = 0;

        public Weather()
        {
            this.MeanC = 0; this.MinC = 0; this.Value = 0;
            this.MaxC = 0; this.errString = "";
        }

        //Initialize from a string.
        public Weather(string csv)
        {
            try
            {
                string[] data = csv.Split(',');
                this.Time = DateTime.Parse(data[0]);
                this.MaxC = Convert.ToDecimal(data[1]);
                this.MeanC = Convert.ToDecimal(data[2]);
                this.MinC = Convert.ToDecimal(data[3]);
                this.Value = MeanC;
                this.Symbol = "NYCTEMP";
            }
            catch (Exception err)
            {
                //Error converting.
                errString = err.Message + "::> " + csv;
            }
        }

        //Clone a Weather Object from another Object.
        public Weather(Weather original)
        {
            MaxC = original.MaxC;
            MeanC = original.MeanC;
            MinC = original.MinC;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="line"></param>
        /// <param name="date"></param>
        /// <param name="datafeed"></param>
        /// <returns></returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            if (lineCount++ == 0)
            {
                return new Weather();
            }
            else
            {
                return new Weather(line);
            }
        }

        //Select source of your data: this can change by each day for large datasets.
        //dataFeed indicates the requester: e.g. backtester or live - meaning you can pull in a different live data stream for your algorithm.
        public override string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed)
        {
            return "https://www.dropbox.com/s/txgqzv2vp5lzpqc/10065.csv?dl=1";
        }

        /// Clone the weather object: only required for fillforward, not applicable here:
        public override BaseData Clone()
        {
            return new Weather(this);
        }
    }
}