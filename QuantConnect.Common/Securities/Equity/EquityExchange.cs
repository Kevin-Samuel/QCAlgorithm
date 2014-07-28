/*
* QUANTCONNECT.COM: Equity Exchange.cs
* Equity Exchange Class - Helper routine to determine if this asset type is currently open and tradable.
*/

/**********************************************************
 * USING NAMESPACES
 **********************************************************/
using System;

//QuantConnect Libraries:
using QuantConnect;
using QuantConnect.Logging;
using QuantConnect.Models;

namespace QuantConnect.Securities {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/

    /// <summary>
    /// Exchange Class - Information and Helper Tools for Exchange Situation
    /// </summary>
    public class EquityExchange : SecurityExchange {

        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/


        /******************************************************** 
        * CLASS CONSTRUCTION
        *********************************************************/
        /// <summary>
        /// Initialise Equity Exchange Objects
        /// </summary>
        public EquityExchange() : 
            base() {
        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        /// <summary>
        /// US Equities Exchange Open Critieria
        /// </summary>
        public override bool ExchangeOpen {
            get
            {
                return DateTimeIsOpen(Time);
            }
        }

        /// <summary>
        /// Number of trading days per year for this security, used for performance statistics.
        /// </summary>
        public override int TradingDaysPerYear 
        {
            get 
            {
                return 252;
            }
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Once live and looping, check if this datetime is open, before updating the security.
        /// </summary>
        /// <param name="dateToCheck">Time to check</param>
        /// <returns>True if open</returns>
        public override bool DateTimeIsOpen(DateTime dateToCheck)
        {
            //Market not open yet:
            if (dateToCheck.TimeOfDay.TotalHours < 9.5 || dateToCheck.TimeOfDay.TotalHours >= 16 || dateToCheck.DayOfWeek == DayOfWeek.Saturday || dateToCheck.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Set the incoming datetime object date to the market open time:
        /// </summary>
        /// <param name="time">Date we want to set:</param>
        /// <returns></returns>
        public override DateTime TimeOfDayOpen(DateTime time)
        {
            //Set open time to 9:30am for US equities:
            return time.Date.AddHours(9.5);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public override DateTime TimeOfDayClosed(DateTime time)
        {
            //Set close time to 4pm for US equities:
            return time.Date.AddHours(16);
        }


        /// <summary>
        /// Conditions to check if the equity markets are open
        /// </summary>
        /// <param name="dateToCheck">datetime to check</param>
        /// <returns>true if open</returns>
        public override bool DateIsOpen(DateTime dateToCheck)
        {
            if (dateToCheck.DayOfWeek == DayOfWeek.Saturday || dateToCheck.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            //Check the date first.
            if (USHoliday.Dates.Contains(dateToCheck.Date)) {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Check if this datetime is open, including extended market hours:
        /// </summary>
        /// <param name="time">Time to check</param>
        /// <returns>Bool true if in normal+extended market hours.</returns>
        public override bool DateTimeIsExtendedOpen(DateTime time)
        {
            if (time.DayOfWeek == DayOfWeek.Saturday || time.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            if (time.TimeOfDay.TotalHours < 4 || time.TimeOfDay.TotalHours >= 20)
            {
                return false;
            }

            return true;
        }

    } //End of EquityExchange

} //End Namespace