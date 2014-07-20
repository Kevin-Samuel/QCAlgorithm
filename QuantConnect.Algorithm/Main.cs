using System;
using System.Collections;
using System.Collections.Generic;

namespace QuantConnect
{
    using QuantConnect.Securities;
    using QuantConnect.Models;


    public class Hawkes
    {
        double mu_ = 0, alpha_ = 0, beta_ = 0, bfactor_ = 0;
        DateTime prevTime;
        bool first = true;

        public Hawkes(double mu, double alpha, double beta)
        {
            mu_ = mu;
            alpha_ = alpha;
            beta_ = beta;
        }

        public double Process(double count, DateTime dt)
        {
            if (first)
            {
                first = false;
                prevTime = dt;
                return mu_;
            }

            double mseconds = (dt - prevTime).TotalSeconds;
            double exp = Math.Exp(-beta_ * mseconds);
            bfactor_ *= exp;
            bfactor_ += exp * count;
            prevTime = dt;
            return mu_ + alpha_ * bfactor_;
        }
    }


    public class SymbolData
    {
        private Hawkes i = new Hawkes(1, 1.2, 1.8);
        public double LastTradePrice { get; set; }
        public DateTime LastTradeTime { get; set; }
        public Hawkes Intensity { get { return i; } }
        public double TickSize { get; set; }
        public double PrevPrice { get; set; }
    }


    public partial class BasicTemplateAlgorithm : QCAlgorithm, IAlgorithm
    {
        string EURUSD = "EURUSD";
        string AUDUSD = "AUDUSD";
        string NZDUSD = "NZDUSD";
        string USDCAD = "USDCAD";
        string GBPUSD = "GBPUSD";

        Dictionary<string, SymbolData> symbols = new Dictionary<string, SymbolData>();

        //Initialize the data and resolution you require for your strategy:
        public override void Initialize()
        {
            //Initialize the start, end dates for simulation; cash and data required.
            SetStartDate(new DateTime(2014, 03, 01));
            SetEndDate(new DateTime(2014, 06, 30));
            SetCash(15000); //Starting Cash in USD.

            symbols.Add(EURUSD, new SymbolData());
            symbols.Add(AUDUSD, new SymbolData());
            symbols.Add(GBPUSD, new SymbolData());
            symbols.Add(NZDUSD, new SymbolData());
            symbols.Add(USDCAD, new SymbolData());

            symbols[EURUSD].TickSize = 0.0001;
            symbols[AUDUSD].TickSize = 0.0001;
            symbols[NZDUSD].TickSize = 0.0001;
            symbols[GBPUSD].TickSize = 0.0001;
            symbols[USDCAD].TickSize = 0.0001;

            AddSecurity(SecurityType.Forex, EURUSD, Resolution.Tick); //Minute, Second or Tick
            //AddSecurity(SecurityType.Forex, AUDUSD, Resolution.Tick);
            //AddSecurity(SecurityType.Forex, USDCAD, Resolution.Tick);
            AddSecurity(SecurityType.Forex, NZDUSD, Resolution.Tick);
            //AddSecurity(SecurityType.Forex, NZDUSD, Resolution.Tick);
            //AddSecurity(SecurityType.Forex, GBPUSD, Resolution.Tick);
            SetRunMode(RunMode.Series); //Series or Parallel for intraday strategies.
        }

        //Handle TradeBar Events: a TradeBar occurs on a time-interval (second or minute bars)
        public override void OnTradeBar(Dictionary<string, TradeBar> data)
        {
            foreach (var symbol in symbols.Keys)
            {
                if (data.ContainsKey(symbol))
                {
                    //Process(symbol, symbols[symbol], data[symbol]);
                }
            }
        }

        //Handle Tick Events - Only when you're requesting tick data
        public override void OnTick(Dictionary<string, List<Tick>> ticks)
        {
            foreach (var symbol in symbols.Keys)
            {
                if (ticks.ContainsKey(symbol) && ticks[symbol].Count > 0)
                {
                    Process(symbol, symbols[symbol], ticks[symbol][ticks[symbol].Count - 1]);
                }
            }
        }

        public void Process(string symbol, SymbolData d, Tick t)
        {
            double mid = (double)(t.BidPrice + t.AskPrice) / 2.0;

            if (d.PrevPrice == 0)
            {
                d.PrevPrice = mid;
                return;
            }

            double intensity = d.Intensity.Process(Math.Abs(d.PrevPrice - mid) / d.TickSize, t.Time);

            if (Portfolio[symbol].HoldStock)
            {
                if (Math.Abs(d.LastTradePrice - mid) > 20 * d.TickSize)
                    Liquidate(symbol);
            }

            if (intensity > 15.25)
            {
                if (!Portfolio[symbol].HoldStock)
                {
                    Debug(intensity.ToString());
                    if (mid > d.PrevPrice)
                    {
                        Order(symbol, 10000);
                        d.LastTradePrice = mid;
                    }

                    if (mid < d.PrevPrice)
                    {
                        Order(symbol, -10000);
                        d.LastTradePrice = mid;
                    }
                }
            }

            d.PrevPrice = mid;
        }
    }
}