using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace tapi
{

    public static class Extensions
    {
        public static string GetMessage(this Exception e) => e.GetBaseException().Message;

        /// <summary>
        /// Get a sequence between days
        /// </summary>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <returns>Sequence of whole days</returns>
        public static IEnumerable<DateTime> To(this DateTime start, DateTime end)
        {
            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                yield return date;
            }
        }

       
        /// <summary>
        /// Get the first date of a week from a tuple of year and week
        /// </summary>
        /// <param name="yearAndWeek"></param>
        /// <returns>The first date of a week</returns>
        public static DateTime FirstDateOfWeekIso8601(this Tuple<int, int> yearAndWeek)
        {
            if (yearAndWeek == null)
                throw new ArgumentException("year and week tuple can't be null");

            return FirstDateOfWeekIso8601(yearAndWeek.Item1, yearAndWeek.Item2);
        }

        private static DateTime FirstDateOfWeekIso8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            var t = 0;
            //if januari 1 is in the first week of the year we should add 7 * week - 1
            if (jan1.DayOfWeek == DayOfWeek.Monday ||
                jan1.DayOfWeek == DayOfWeek.Tuesday ||
                jan1.DayOfWeek == DayOfWeek.Wednesday ||
                jan1.DayOfWeek == DayOfWeek.Thursday)
            {
                t = -1;
            }
            return jan1.AddDays(7 * (weekOfYear + t)).GetFirstInWeek();
        }
        /// <summary>
        /// Get first date in week
        /// </summary>
        /// <param name="date">Date to get the first in week for</param>
        /// <returns>The first date in the week</returns>
        public static DateTime GetFirstInWeek(this DateTime date)
        {
            switch (date.DayOfWeek)
            {

                case DayOfWeek.Monday:
                    return date.AddDays(0);
                case DayOfWeek.Tuesday:
                    return date.AddDays(-1);
                case DayOfWeek.Wednesday:
                    return date.AddDays(-2);
                case DayOfWeek.Thursday:
                    return date.AddDays(-3);
                case DayOfWeek.Friday:
                    return date.AddDays(-4);
                case DayOfWeek.Saturday:
                    return date.AddDays(-5);
                case DayOfWeek.Sunday:
                default:
                    return date.AddDays(-6);
            }
        }
        /// <summary>
        /// Get the last date for the current week
        /// </summary>
        /// <param name="date">Date to get the last in week for</param>
        /// <returns>Last date of the week</returns>
        public static DateTime GetLastInWeek(this DateTime date)
        {
            switch (date.DayOfWeek)
            {

                case DayOfWeek.Monday:
                    return date.AddDays(6);
                case DayOfWeek.Tuesday:
                    return date.AddDays(5);
                case DayOfWeek.Wednesday:
                    return date.AddDays(4);
                case DayOfWeek.Thursday:
                    return date.AddDays(3);
                case DayOfWeek.Friday:
                    return date.AddDays(2);
                case DayOfWeek.Saturday:
                    return date.AddDays(1);
                case DayOfWeek.Sunday:
                default:
                    return date.AddDays(0);
            }
        }

        /// <summary>
        /// Creates a CSV file with the properties of the type as columns and the items in the collection as rows
        /// </summary>
        /// <typeparam name="T">The type of the elements in collection</typeparam>
        /// <param name="collection">Source collection</param>
        /// <param name="separator"></param>
        /// <param name="useQuotations"></param>
        /// <returns>A string with the CSV content.</returns>
        public static string ToCsv<T>(this IEnumerable<T> collection, string separator = ",", bool useQuotations = true)
        {

            var props = typeof(T).GetProperties();

            var sep = useQuotations ? string.Format("\"{0}\"", separator) : separator;
            var startStop = useQuotations ? "\"" : "";

            var header = startStop + string.Join(sep, props.Select(x => x.Name)) + startStop;
            var format = startStop + string.Join(sep, Enumerable.Range(0, props.Length).Select(x => "{" + x + "}")) + startStop;

            var data = from l in collection select string.Format(format, props.Select(x => x.GetValue(l, null)).ToArray());

            return header + "\r\n" + string.Join("\r\n", data);
        }
    }
}
