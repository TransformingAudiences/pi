using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;

namespace tapi
{
    public class TimeHelpersTest
    {
        [Fact]
        public void TestIsOverlapping()
        {
            var interval = (5, 10);

            Assert.Equal(false, IsOverlapping(interval,(1, 2)));
            Assert.Equal(true, IsOverlapping(interval, (1, 6)));
            Assert.Equal(true, IsOverlapping(interval, (1, 11)));
            Assert.Equal(true, IsOverlapping(interval, (5, 9)));
            Assert.Equal(true, IsOverlapping(interval, (8, 11)));
            Assert.Equal(false, IsOverlapping(interval, (11, 12)));

        }

        [Fact]
        public void TestGetOverlapping()
        {
            var interval = (5, 10);
            
            AssertException(() => GetOverlapping(interval, (1, 2)));
            Assert.Equal((5,6), GetOverlapping(interval, (1, 6)));
            Assert.Equal((5,10), GetOverlapping(interval, (1, 11)));
            Assert.Equal((5,9), GetOverlapping(interval, (5, 9)));
            Assert.Equal((8,10), GetOverlapping(interval, (8, 11)));
             AssertException(()=> GetOverlapping(interval, (11, 12)));

        }
        [Fact]
        public void TestSplitInterval()
        {
            TestSplitInterval(PeriodType.Minute, "03:33:00", "03:33:01", 1);
            TestSplitInterval(PeriodType.Minute, "03:33:00", "03:34:59", 2);
            TestSplitInterval(PeriodType.Minute, "03:33:00", "03:35:59", 3);

            TestSplitInterval(PeriodType.Hour, "03:33:00", "03:33:01", 1);
            TestSplitInterval(PeriodType.Hour, "03:33:00", "04:34:59", 2);
            TestSplitInterval(PeriodType.Hour, "03:33:00", "05:35:59", 3);

            TestSplitInterval(PeriodType.DayPart, "03:33:00", "03:33:01", 1);
            TestSplitInterval(PeriodType.DayPart, "03:33:00", "06:34:59", 2);
            TestSplitInterval(PeriodType.DayPart, "03:33:00", "09:35:59", 3);

            TestSplitInterval(PeriodType.Day, "03:33:00", "03:33:01", 1);
            TestSplitInterval(PeriodType.Day, "03:33:00", "06:34:59", 1);
            TestSplitInterval(PeriodType.Day, "03:33:00", "09:35:59", 1);
        }

        private static void TestSplitInterval(PeriodType type, string start, string end, int nbr)
        {
            var s = TimeSpan.Parse(start, System.Globalization.CultureInfo.InvariantCulture);
            var e = TimeSpan.Parse(end, System.Globalization.CultureInfo.InvariantCulture);
            var res = TimeHelpers.SplitInterval(DateTime.Today, (s, e), type);
            Assert.Equal(nbr, res.Count());
        }

        private static bool IsOverlapping((int s, int e) l, (int s, int e) r)
        {
            return TimeHelpers.IsOverlapping((TimeSpan.FromMinutes(l.s), TimeSpan.FromMinutes(l.e)), (TimeSpan.FromMinutes(r.s), TimeSpan.FromMinutes(r.e)));
        }
        private static (int s, int e) GetOverlapping((int s, int e) l, (int s, int e) r)
        {
            var res = TimeHelpers.GetOverlapping((TimeSpan.FromMinutes(l.s), TimeSpan.FromMinutes(l.e)), (TimeSpan.FromMinutes(r.s), TimeSpan.FromMinutes(r.e)));
            return ((int)res.start.TotalMinutes,(int)res.end.TotalMinutes);
        }

        private static void AssertException(Action a)
        {
            bool didThrow = false;
            try
            {
                a();
            }
            catch
            {
                didThrow = true;
            }

            Assert.Equal(true, didThrow); ;
        }
    }
}
