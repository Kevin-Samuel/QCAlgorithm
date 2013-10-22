/*
* QUANTCONNECT.COM: Equity Cache.cs
* Equity Caching Class - Store online method calculations here, historical data points etc.
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

//QuantConnect Libraries:
using QuantConnect;
using QuantConnect.Logging;
using QuantConnect.Models;

namespace QuantConnect.Securities {

    /******************************************************** 
    * CLASS DEFINITIONS
    ********************************************************/

    /// <summary>
    /// Common Caching Spot For Market Data and Averaging. 
    /// </summary>
    public class EquityCache : SecurityCache {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/

        /******************************************************** 
        * CONSTRUCTOR/DELEGATE DEFINITIONS
        *********************************************************/
        /// <summary>
        /// Start a new Cache for the set Index Code
        /// </summary>
        public EquityCache() :
            base() {
            //Nothing to do:
        }


        /******************************************************** 
        * CLASS METHODS
        *********************************************************/



    } //End EquityCache Class
} //End Namespace