/*
* QUANTCONNECT.COM: Secuity Cache
* Common caching class for storing historical ticks etc.
*/

/**********************************************************
 * USING NAMESPACES
 **********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using MathNet.Numerics;
using MathNet.Filtering;

//QuantConnect Libraries:
using QuantConnect;
using QuantConnect.Logging;
using QuantConnect.Models;

namespace QuantConnect.Securities {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Packet by Packet Data Filtering Mechanism for Dynamically Detecting Bad Ticks.
    /// </summary>
    public class EquityDataFilter : SecurityDataFilter
    {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        /// <summary>
        /// Range of acceptable standard deviations from norm for last XXX datapoints:
        /// </summary>
        private const double _acceptableDeviations = 4;

        /// <summary>
        /// History we're looking at to determine the standard deviation.
        /// </summary>
        private const int _dataPointHistory = 300;  //Five Minutes (Seconds for tradebars).

        //Working Variables:
        private double _mean = 0;
        private double _meanSum = 0;
        private double _m2;
        private Queue<double> _queue = new Queue<double>();

        /******************************************************** 
        * CONSTRUCTOR/DELEGATE DEFINITIONS
        *********************************************************/
        /// <summary>
        /// Initialize Data Filter Class:
        /// </summary>
        public EquityDataFilter() : base()
        {

        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Equity filter the data: true - accept, false - fail.
        /// </summary>
        /// <param name="data">Data class</param>
        /// <param name="vehicle">Security asset</param>
        public override bool Filter(Security vehicle, BaseData data)
        {

            // Filter disabled. Suggested filter below:
            return true;

            //Assuming this isnt the first packet:
            if (data == null) return true;

            //Use running/online techniques to calculate the standard deviation:
            double stddev = OnlineStandardDeviation(Convert.ToDouble(data.Value));

            if (_queue.Count < 2)
            {
                return true;
            }

            double deltaFromMean = (Convert.ToDouble(data.Value) - _mean);
            double stddevFromMean = deltaFromMean / stddev;

            //How many standard deviations from normal are we? less than 3.5 is OK. > 3.5 is probably an error.
            if (Math.Abs(stddevFromMean) > _acceptableDeviations)
            {
                var queueCopy = _queue.ToList();
                var testDev = MathNet.Numerics.Statistics.StreamingStatistics.StandardDeviation(queueCopy);
                Console.WriteLine("STANDARD DEVIATION: Online: " + stddev + "  Reference: " + testDev);

                //FAIL PACKET:
                return false;
            }
            else 
            {
                //ACCEPT PACKET:
                return true;
            }
        }


        /// <summary>
        /// Update the online standard deviation formula
        /// </summary>
        /// <param name="value">new data.</param>
        /// <returns>decimal standard deviation</returns>
        private double OnlineStandardDeviation(double value)
        {
            double stddev = 0;
            try
            {
                double variance = OnlineVariance(value);
                stddev = Math.Sqrt(variance);
            }
            catch (Exception err)
            {
                Log.Error("EquityDataFilter.OnlineStandardDeviation(): " + err.Message);
            }
            return stddev;
        }


        /// <summary>
        /// Online variance 
        ///  http://en.wikipedia.org/wiki/Algorithms_for_calculating_variance
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private double OnlineVariance(double value)
        {
            double variance = 0;
            double removed = 0;
            double delta = 0;
            double queueLength = (double)_queue.Count;

            try
            {
                //Update the circular buffer
                if (queueLength > _dataPointHistory)
                {
                    removed = _queue.Dequeue();
                }

                //Add the new value:
                _queue.Enqueue(value);
                queueLength = (double)_queue.Count;

                //Find delta from mean:
                delta = value - _mean;

                //Update mean sum: removed 0 by default:
                _meanSum += value - removed;
                _mean = _meanSum / _queue.Count;

                //"M2" - 
                _m2 = _m2 + delta * (value - _mean);

                //Make sure we have enough samples:
                if (queueLength < 2)
                {
                    return 0;
                } 

                //Variance:
                variance = _m2 / (queueLength - 1);
            }
            catch (Exception err)
            {
                Log.Error("EquityDataFilter.OnlineVariance(): " + err.Message);
            }
            return variance;
        }

    } //End Filter

} //End Namespace