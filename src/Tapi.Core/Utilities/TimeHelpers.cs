﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tapi
{
    public static class TimeHelpers
    {
        public static bool IsOverlapping(this (TimeSpan start, TimeSpan end) left, (TimeSpan start, TimeSpan end) right)
        {
            return left.start <= right.end && left.end >= right.start; 
        }

        public static (TimeSpan start, TimeSpan end) GetOverlapping(this (TimeSpan start, TimeSpan end) left, (TimeSpan start, TimeSpan end) right)
        {
            var isStartIn = left.start <= right.start && right.start <= left.end;
            var isEndIn = left.start <= right.end && right.end <= left.end;
            var isIn = right.start <= left.start && left.end <= right.end;
            if (isStartIn && isEndIn)
            {
                return ( right.start, right.end);
            }
            else if (isStartIn && !isEndIn)
            {
                return (right.start, left.end);
            }
            else if (!isStartIn && isEndIn)
            {
                return ( left.start, right.end);
            }
            else if (isIn)
            {
                return (left.start, left.end);
            }

            throw new ArgumentException("Intervals do not overlapp");
        }

        public static TimeSpan GetDuration(this (TimeSpan start, TimeSpan end) interval)
        {
            return interval.end - interval.start;
        }

        public static (string tag, (TimeSpan start, TimeSpan end) interval) GetPeriodInfo(DateTime date, TimeSpan time, PeriodType type)
        {
            var tag = "";
            switch (type)
            {
                case PeriodType.Minute:
                    tag = ((int)time.TotalHours).ToString("00") + ":" + time.Minutes.ToString("00");
                    break;
                case PeriodType.Hour:
                    tag = ((int)time.TotalHours).ToString("00");
                    break;
                case PeriodType.DayPart:
                    var partsLength = 3;
                    tag = (((int)time.TotalHours / partsLength) * partsLength).ToString("00") + "-" + ((((int)time.TotalHours + partsLength) / partsLength) * partsLength).ToString("00");
                    break;
                case PeriodType.Day:
                    tag = date.ToString("MMdd");
                    break;
                case PeriodType.Week:
                case PeriodType.Month:
                default:
                    tag = date.Month.ToString("00");
                    break;
            }


            switch (type)
            {
                case PeriodType.Minute:
                    var hour = int.Parse(tag.Substring(0, 2));
                    var minute = int.Parse(tag.Substring(3, 2));
                    var start = TimeSpan.FromMinutes(60 * hour + minute);
                    return (tag,(start, start.Add(TimeSpan.FromSeconds(59))));
                case PeriodType.Hour:
                    hour = int.Parse(tag.Substring(0, 2));
                    start = TimeSpan.FromMinutes(60 * hour);
                    return (tag,(start, start.Add(TimeSpan.FromSeconds(59 * 60 + 59))));
                case PeriodType.DayPart:
                    hour = int.Parse(tag.Substring(0, 2));
                    start = TimeSpan.FromMinutes(60 * hour);
                    return (tag,(start, start.Add(TimeSpan.FromSeconds(2 * 60 * 60 + 59 * 60 + 59))));
                case PeriodType.Day:
                    return (tag, (TimeSpan.FromMinutes(0), TimeSpan.FromDays(2) ) );
                case PeriodType.Week:
                    return (tag, (TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(0)));
                case PeriodType.Month:
                default:
                    return (tag, (TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(0)));
            }
        }

        public static IEnumerable<(DateTime date, (TimeSpan start, TimeSpan end) interval,string tag)> SplitInterval(DateTime date, (TimeSpan start, TimeSpan end) interval, PeriodType type)
        {
            var nbrOfCells =
                type == PeriodType.Minute ? ((int)interval.end.TotalMinutes - (int)interval.start.TotalMinutes) + 1 :
                type == PeriodType.Hour ? ((int)interval.end.TotalHours - (int)interval.start.TotalHours) + 1 :
                type == PeriodType.DayPart ? ((int)interval.end.TotalHours / 3 - (int)interval.start.TotalHours / 3) + 1 :
                type == PeriodType.Day ? 1 :
                type == PeriodType.Week ? 1 :
                type == PeriodType.Month ? 1 :
                throw new ArgumentException("");

            return Enumerable.Range(0, nbrOfCells).Select(x =>
            {
                var start = interval.start.Add(GetTimeSpan(type, x));
                var info = GetPeriodInfo(date, start, type);
                return (date, info.interval, info.tag);
            });
        }

        private static TimeSpan GetTimeSpan(PeriodType type, int unit)
        {
            switch (type)
            {
                case PeriodType.Minute:
                    return TimeSpan.FromMinutes(1 * unit);
                case PeriodType.Hour:
                    return TimeSpan.FromHours(1 * unit);
                case PeriodType.DayPart:
                    return TimeSpan.FromHours(3 * unit);
                case PeriodType.Day:
                    return TimeSpan.FromDays(1 * unit);
                case PeriodType.Week:
                case PeriodType.Month:
                default:
                    return TimeSpan.FromDays(30 * unit);
            }
        }
    }
}
