/*
* QUANTCONNECT.COM - 
* QC.OS -- Operating System Checks for Cross Platform C#
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;


namespace QuantConnect {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Limited Length Queue:
    /// http://stackoverflow.com/questions/1292/limit-size-of-queuet-in-net
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LimitedQueue<T> : Queue<T>
    {
        private int limit = -1;

        /// <summary>
        /// Max Length 
        /// </summary>
        public int Limit
        {
            get { return limit; }
            set { limit = value; }
        }

        /// <summary>
        /// Create a new fixed length queue:
        /// </summary>
        public LimitedQueue(int limit)
            : base(limit)
        {
            this.Limit = limit;
        }

        /// <summary>
        /// Enqueue a new item int the generic fixed length queue:
        /// </summary>
        public new void Enqueue(T item)
        {
            while (this.Count >= this.Limit)
            {
                this.Dequeue();
            }
            base.Enqueue(item);
        }
    }
} // End QC Namespace
