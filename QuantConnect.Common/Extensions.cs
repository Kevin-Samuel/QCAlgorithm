/*
* QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals, V0.1
* Extension helper methods on lists etc.
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

//QuantConnect Project Libraries:
using QuantConnect.Logging;

namespace QuantConnect {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Extensions Function Collections - Group all static extensions functions here.
    /// </summary>
    public static class Extensions {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/


        /// <summary>
        /// List Extension Method: Move one element from A to B.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List we're operating on.</param>
        /// <param name="oldIndex">Index of variable we want to move.</param>
        /// <param name="newIndex">New location for the variable</param>
        public static void Move<T>(this List<T> list, int oldIndex, int newIndex) {
            T oItem = list[oldIndex];
            list.RemoveAt(oldIndex);
            if (newIndex > oldIndex) newIndex--;
            list.Insert(newIndex, oItem);
        }


        /// <summary>
        /// Convert a string into a Byte Array
        /// </summary>
        /// <param name="str">String to Convert to Bytes.</param>
        /// <returns>Byte Array</returns>
        public static byte[] GetBytes(this string str) {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }


        /// <summary>
        /// Clear all items from a thread safe queue: has risk of forever clearing if another 
        /// thread adding to queue at same time.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        public static void Clear<T>(this ConcurrentQueue<T> queue) {
            T item;
            while (queue.TryDequeue(out item)) {
                // NOP
            }
        }

        /// <summary>
        /// Convert a byte array into a string.
        /// </summary>
        /// <param name="bytes">byte array to convert.</param>
        /// <returns>String from Bytes.</returns>
        public static string GetString(this byte[] bytes) {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }


        /// <summary>
        /// Convert a string to its MD5 equivalent:
        /// </summary>
        /// <param name="str">String we want to MD5 encode.</param>
        /// <returns>MD5 Hash of a string:</returns>
        public static string ToMD5(this string str) {
            StringBuilder builder = new StringBuilder();
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(str));
                for (int i = 0; i < data.Length; i++)
                    builder.Append(data[i].ToString("x2"));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Extension method to automatically set the update value to same as "add" value for TryAddUpdate
        /// </summary>
        public static void AddOrUpdate<K, V>(this ConcurrentDictionary<K, V> dictionary, K key, V value)
        {
            dictionary.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
        }


        /// <summary>
        /// Round a double to a x-significant figures:
        /// </summary>
        public static double RoundToSignificantDigits(this double d, int digits)
        {
            if (d == 0) return 0;
            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits);
        }


        /// <summary>
        /// FAST String to Decimal Conversion. Some assuptions:
        /// 1. Always position, no "signs" +,- etc.
        /// </summary>
        /// <param name="str">String to be converted</param>
        /// <returns>decimal value of the string</returns>
        public static decimal ToDecimal(this string str) {

            long value = 0;
            int exp = 0;
            int decimalPlaces = int.MinValue;
            long maxValueDivideTen = (long.MaxValue/10);


            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                if (ch >= '0' && ch <= '9') {
                    while (value >= maxValueDivideTen) 
                    {
                        value >>= 1;
                        exp++;
                    }
                    value = value * 10 + (ch - '0');
                    decimalPlaces++;
                } else if (ch == '.') {
                    decimalPlaces = 0;
                } else {
                    break;
                }
            }

            if (decimalPlaces > 0) {
                int divider = 10;
                for (int i = 1; i < decimalPlaces; i++) divider *= 10;

                return (decimal)value / divider;
            }

            return (decimal)value;
        }


        /// <summary>
        /// Get the extension of this string file
        /// </summary>
        /// <param name="str">String we're looking for the extension for.</param>
        /// <returns>Last 4 character string of string.</returns>
        public static string GetExtension(this string str) {
            return str.Substring(Math.Max(0, str.Length - 4));
        }


        /// <summary>
        /// Convert Strings to Stream
        /// </summary>
        /// <param name="str">String to convert to stream</param>
        /// <returns>StreamReader</returns>
        public static Stream ToStream(this string str) {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


        /// <summary>
        /// Round a DateTime to nearest Timespan Period.
        /// </summary>
        /// <param name="time">TimeSpan To Round</param>
        /// <param name="roundingInterval">Rounding Unit</param>
        /// <param name="roundingType">Rounding method</param>
        /// <returns>Rounded timespan</returns>
        public static TimeSpan Round(this TimeSpan time, TimeSpan roundingInterval, MidpointRounding roundingType) {
            return new TimeSpan(
                Convert.ToInt64(System.Math.Round(
                    time.Ticks / (decimal)roundingInterval.Ticks,
                    roundingType
                )) * roundingInterval.Ticks
            );
        }

        /// <summary>
        /// Default Timespan Rounding 
        /// </summary>
        public static TimeSpan Round(this TimeSpan time, TimeSpan roundingInterval) {
            return Round(time, roundingInterval, MidpointRounding.ToEven);
        }


        /// <summary>
        /// Round a datetime method down:
        /// </summary>
        public static DateTime RoundDown(this DateTime dateTime, TimeSpan interval)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % interval.Ticks));
        }


        /// <summary>
        /// Round a DateTime to the nearest unit.
        /// </summary>
        public static DateTime Round(this DateTime datetime, TimeSpan roundingInterval) {
            return new DateTime((datetime - DateTime.MinValue).Round(roundingInterval).Ticks);
        }


        /// <summary>
        /// Explicitly Round UP to the Nearest TimeSpan Unit.
        /// </summary>
        /// <param name="time">Base Time to Round UP</param>
        /// <param name="d">TimeSpan Unit</param>
        /// <returns>Rounded DateTime</returns>
        public static DateTime RoundUp(this DateTime time, TimeSpan d) {
            return new DateTime(((time.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
        }

    }
}
