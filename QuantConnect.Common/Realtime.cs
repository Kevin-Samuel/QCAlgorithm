/*
* QUANTCONNECT.COM - 
* QC.RealTime - Custom real time event generator for C#
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

//QuantConnect Project Libraries:
using QuantConnect.Logging;
using QuantConnect.Securities;

namespace QuantConnect {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Real time event handler in C#
    /// </summary>
    public class RealTimeSynchronizedTimer {

        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        private TimeSpan _period;
        private Action _callback = null;
        private Stopwatch _timer = new Stopwatch();
        private Thread _thread;
        private bool _stopped = false;
        private DateTime _triggerTime = new DateTime();
        private bool _paused = false;

        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Constructor for Real Time Event Driver:
        /// </summary>
        public RealTimeSynchronizedTimer()
        {
            this._period = TimeSpan.FromSeconds(0);
            this._thread = new Thread(new ThreadStart(Scanner));
        }

        /// <summary>
        /// Trigger an event callback after precisely milliseconds-lapsed. 
        /// This is expensive, it creates a new thread and closely monitors the loop.
        /// </summary>
        /// <param name="period">delay period between event callbacks</param>
        /// <param name="callback">Callback event</param>
        public RealTimeSynchronizedTimer(TimeSpan period, Action callback)
        {
            this._period = period;
            this._callback = callback;
            this._timer = new Stopwatch();
            this._thread = new Thread(new ThreadStart(Scanner));
            this._stopped = false;
            this._triggerTime = DateTime.Now.RoundUp(period);
        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Start the Synchronized Real Time Timer - fire events at start of each second or minute 
        /// </summary>
        public void Start()
        { 
            this._timer.Start();
            this._thread.Start();
            _triggerTime = DateTime.Now.RoundDown(_period).Add(_period);
        }
        
        /// <summary>
        /// Scan the stopwatch for the desired millisecond delay:
        /// </summary>
        public void Scanner()
        {
            while (!_stopped)
            {
                if (_callback != null && DateTime.Now >= _triggerTime)
                {
                    _timer.Restart();
                    _triggerTime = DateTime.Now.RoundDown(_period).Add(_period);
                    _callback();
                }

                while (_paused && !_stopped) Thread.Sleep(10);
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Hang the real time event:
        /// </summary>
        public void Pause()
        {
            _paused = true;
        }

        /// <summary>
        /// AntiPause
        /// </summary>
        public void Resume()
        {
            _paused = false;
        }

        /// <summary>
        /// Stop the real time timer:
        /// </summary>
        public void Stop()
        {
            _stopped = true;
        }

    } // End Time Class

} // End QC Namespace
