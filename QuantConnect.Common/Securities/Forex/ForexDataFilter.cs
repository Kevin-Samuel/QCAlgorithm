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
    public class ForexDataFilter : SecurityDataFilter
    {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/

        /******************************************************** 
        * CONSTRUCTOR/DELEGATE DEFINITIONS
        *********************************************************/
        /// <summary>
        /// Initialize Data Filter Class:
        /// </summary>
        public ForexDataFilter()
            : base() 
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
            //FX data is from FXCM and fairly clean already.
            return true;
        }

    } //End Filter

} //End Namespace