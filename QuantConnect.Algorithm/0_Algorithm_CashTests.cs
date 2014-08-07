using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Securities;
using QuantConnect.Models;
using System.Globalization;

namespace QuantConnect
{

    public class CashTestingStrategy : QCAlgorithm
    {
        public override void Initialize()
        {
            SetStartDate(2013, 1, 1);
            SetEndDate(2013, 12, 31);
            SetCash(100000);
            AddData<CashType>("CASH");
        }

        public void OnData(CashType data)
        {
            try
            {
                //TEST 1: BUY AND HOLD:
                //if (!Portfolio.Invested)
                //{
                //    int quantity = (int)(Portfolio.Cash / Math.Abs(data.Value));
                //    Order("CASH", quantity);
                //    Console.WriteLine("Buying Cash 'Shares': Cash: " + data.Value + " Q:" + quantity);
                //}

                //TEST2: BUY - FLAT - BUY each month
                //if (Time.Month % 2 != 0 && !Portfolio.Invested)        //Jan, Mar, May -- alternating months.
                //{
                //    Order("CASH", quantity);
                //} 
                //else if (Time.Month % 2 == 0 && Portfolio.Invested)
                //{   
                //   Order("CASH", -Portfolio["CASH"].Quantity);    
                //}

                //TEST3: LONG SHORT LONG - Each MOnth
                //if (Time.Month % 2 != 0 && (Portfolio["CASH"].IsShort || !Portfolio["CASH"].Invested))
                //{
                //    Order("CASH", 100 + Portfolio["CASH"].AbsoluteQuantity);
                //}
                //else if (Time.Month % 2 == 0 && Portfolio["CASH"].IsLong)
                //{
                //    Order("CASH", -2 * Portfolio["CASH"].Quantity);
                //}

                //TEST 4: FULL SWEEP TESTING:
                if (Time == new DateTime(2013, 1, 1))
                {
                    Order("CASH", 100); // +100 Holdings
                }
                else if (Time == new DateTime(2013, 2, 1))
                {
                    Order("CASH", -50); // +50 Holdings
                }
                else if (Time == new DateTime(2013, 3, 1))
                {
                    Order("CASH", -100); // -50 Holdings
                }
                else if (Time == new DateTime(2013, 4, 1))
                {
                    Order("CASH", -50); // -100 Holdings
                }
                else if (Time == new DateTime(2013, 5, 1))
                {
                    Order("CASH", 50); // -50 Holdings
                }
                else if (Time == new DateTime(2013, 6, 1))
                {
                    Order("CASH", 100);// +50 Holdings
                }
                else if (Time == new DateTime(2013, 7, 1))
                {
                    Order("CASH", 50); // +100 Holdings
                }
                else if (Time == new DateTime(2013, 8, 1))
                {
                    Order("CASH", -50); // +50 Holdings
                }
                else if (Time == new DateTime(2013, 9, 1))
                {
                    Order("CASH", -100); // -50 Holdings
                }
                else if (Time == new DateTime(2013, 10, 1))
                {
                    Order("CASH", -50); // -100 Holdings
                }
                else if (Time == new DateTime(2013, 11, 1))
                {
                    Order("CASH", +50); // -50 Holdings
                }
                else if (Time == new DateTime(2013, 12, 1))
                {
                    Order("CASH", +100); // +50 Holdings
                }
                else if (Time == new DateTime(2013, 12, 15))
                {
                    Order("CASH", -50); // +0 Holdings
                }

            }
            catch (Exception err)
            {
                Debug("Err: " + err.Message);
            }
        }

        // PLOT OUR CASH POSITION:
        public override void OnEndOfDay()
        {
            try
            {
                Plot("Cash", Portfolio.Cash);
                Plot("PortfolioValue", Portfolio.TotalPortfolioValue);
                Plot("HoldingValue", Portfolio["CASH"].HoldingsValue);
                Plot("HoldingQuantity", Portfolio["CASH"].Quantity);
            }
            catch (Exception err)
            {
                Debug("Err: " + err.Message);
            }
        }
    }


    public class CashType : BaseData
    {
        public CashType()
        {
            this.Symbol = "CASH";
        }

        public override string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed)
        {
            return "https://www.dropbox.com/s/oiliumoyqqj1ovl/2013-cash.csv?dl=1";
        }

        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            //New Bitcoin object
            CashType cash = new CashType();

            try
            {
                string[] data = line.Split(',');
                cash.Time = DateTime.ParseExact(data[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                cash.Value = Convert.ToDecimal(data[1]);
            }
            catch { /* Do nothing, skip first title row */ }

            return cash;
        }
    }

}