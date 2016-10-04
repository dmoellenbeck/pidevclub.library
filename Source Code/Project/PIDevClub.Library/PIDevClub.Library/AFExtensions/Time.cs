// Copyright 2016 OSIsoft, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OSIsoft.AF.Time;

namespace PIDevClub.Library.AFExtensions
{
    public static class Time
    {
        public const int PITicksPerSecond = 65536;
        public const double PISubsecond = 1.0 / (double)PITicksPerSecond;

        /// <summary>
        /// Returns a new <see cref="AFTime"/> that adds the specified number of 100 nanosecond ticks to the value of this instance.
        /// </summary>
        public static AFTime AddTicks(this AFTime time, long value) => new AFTime(time.UtcTime.AddTicks(value));

        /// <summary>
        /// Returns a new <see cref="AFTime"/> that adds the specified number of ~15.26 microsecond ticks to the value of this instance.
        /// </summary>
        public static AFTime AddPITicks(this AFTime time, long value) => new AFTime(time.UtcSeconds + (double)(value * PISubsecond));

        /// <summary>
        /// Returns a new <see cref="AFTime"/> that adds the specified number of milliseconds to the value of this instance.
        /// </summary>
        public static AFTime AddMilliseconds(this AFTime time, double value) => AddSeconds(time, value / 1000.0);

        /// <summary>
        /// Returns a new <see cref="AFTime"/> that adds the specified number of seconds to the value of this instance.
        /// </summary>
        public static AFTime AddSeconds(this AFTime time, double value) => new AFTime(time.UtcSeconds + value);

        /// <summary>
        /// Returns a new <see cref="AFTime"/> that adds the specified number of minutes to the value of this instance.
        /// </summary>
        public static AFTime AddMinutes(this AFTime time, double value) => AddSeconds(time, value * 60.0);
  
        /// <summary>
        /// Returns a new <see cref="AFTime"/> that adds the specified number of hours to the value of this instance.
        /// </summary>
        public static AFTime AddHours(this AFTime time, double value) => AddSeconds(time, value * (60 * 60));

        /// <summary>
        /// Returns a new <see cref="AFTime"/> that adds the specified number of days to the value of this instance.
        /// </summary>
        public static AFTime AddDays(this AFTime time, double value) => new AFTime(time.LocalTime.AddDays(value));

        /// <summary>
        /// Returns a new <see cref="AFTime"/> that adds the specified number of months to the value of this instance.
        /// </summary>
        public static AFTime AddMonths(this AFTime time, int value) => new AFTime(time.LocalTime.AddMonths(value));

        /// <summary>
        /// Returns a new <see cref="AFTime"/> that adds the specified number of years to the value of this instance.
        /// </summary>
        public static AFTime AddYears(this AFTime time, int value) => new AFTime(time.LocalTime.AddYears(value));

        /// <summary>
        /// Indicates whether this instance of <see cref="AFTime"/> contains subseconds.
        /// </summary>
        public static bool HasSubseconds(this AFTime time)
        {
            var seconds = time.UtcSeconds;
            var wholeSeconds = (int)Math.Truncate(seconds);
            return seconds > wholeSeconds;
        }

        /// <summary>
        /// Indicates whether this instance of <see cref="DateTime"/> contains subseconds.
        /// </summary>
        public static bool HasSubseconds(this DateTime time)
        {
            var seconds = time.TimeOfDay.TotalSeconds;
            var wholeSeconds = (int)Math.Truncate(seconds);
            return seconds > wholeSeconds;
        }

        /// <summary>
        /// Rounds the <see cref="StartTime"/> and <see cref="EndTime"/> properties of the <see cref="TimeSpan"/> object to the precision supported by the <see cref="PIServer"/>.
        /// </summary>
        public static TimeSpan ToPIPrecision(this TimeSpan span)
        {
            var totalSeconds = span.TotalSeconds;
            var wholeSeconds = (int)Math.Truncate(totalSeconds);
            var subseconds = totalSeconds - wholeSeconds;
            if (subseconds == 0) return TimeSpan.FromSeconds(totalSeconds);

            var totalTicks = subseconds * PITicksPerSecond;
            var wholeTicks = (int)Math.Truncate(totalTicks);
            var partialTicks = totalTicks - wholeTicks;

            if (partialTicks > 0) ++wholeTicks;

            // https://msdn.microsoft.com/en-us/library/system.timespan.fromseconds(v=vs.110).aspx
            // Returns a number of seconds, accurate to the nearest millisecond.
            return TimeSpan.FromSeconds(wholeSeconds + (wholeTicks * PISubsecond));
        }

        /// <summary>
        /// Partitions an <see cref="AFTimeRange"/> into non-overlapping subranges.  Useful to chop up one monolithic RecordedValues call 
        /// into several smaller calls.  Assumes a faily even distribution of events across the time range.  
        /// It's likely that subsequent calls to RecordedValues may return more eventsPerPartition than requested.
        /// </summary>
        /// <param name="timeRange"></param>
        /// <param name="eventCount">expected number events within the full time range.  This may require a prior event-weighted Summary Count.</param>
        /// <param name="eventsPerPartition">The typical number of events desired per subrange.</param>
        /// <returns>IEnumerable collection of AFTimeRange</returns>
        public static IEnumerable<AFTimeRange> PartitionIntoSubranges(this AFTimeRange timeRange, int eventCount, int eventsPerPartition)
        {
            // CRITICAL WORKING ASSUMPTION:
            // (1) Fairly evenly distrubuted events across the timeRange.
            var partitionCount = eventCount / eventsPerPartition;
            if (eventCount % eventsPerPartition != 0) ++partitionCount;

            return PartitionIntoSubranges(timeRange, partitionCount);
        }

        /// <summary>
        /// Partitions an <see cref="AFTimeRange"/> into non-overlapping subranges.  Useful to chop up one monolithic RecordedValues call 
        /// into several smaller calls.  
        /// </summary>
        /// <param name="timeRange"></param>
        /// <param name="count">the number of subranges desired</param>
        public static IEnumerable<AFTimeRange> PartitionIntoSubranges(this AFTimeRange timeRange, int count)
        {
            Func<AFTimeRange, int, IEnumerable<AFTimeRange>> function;
            
            if (timeRange.EndTime < timeRange.StartTime)
            {
                function = GetSubrangesBackward;
            }
            else
            {
                function = GetSubrangesForward;
            }

            return function(timeRange, count);
        }

        private static IEnumerable<AFTimeRange> GetSubrangesForward(AFTimeRange timeRange, int count)
        {
            if (count == 1)
            {
                yield return timeRange;
            }

            if (count < 2)
            {
                yield break;
            }

            var totalSeconds = timeRange.Span.TotalSeconds;
            var secondsPerSubrange = (totalSeconds / count) + 1; // to be safe

            var rangeStart = timeRange.StartTime;

            while (rangeStart <= timeRange.EndTime)
            {
                var rangeEnd = new AFTime(rangeStart.UtcSeconds + secondsPerSubrange);
                if (rangeEnd > timeRange.EndTime) rangeEnd = timeRange.EndTime;
                yield return new AFTimeRange(rangeStart, rangeEnd);
                rangeStart = rangeEnd.AddPITicks(1);
            }
        }

        private static IEnumerable<AFTimeRange> GetSubrangesBackward(AFTimeRange timeRange, int count)
        {
            if (count == 1)
            {
                yield return timeRange;
            }

            if (count < 2)
            {
                yield break;
            }

            var totalSeconds = timeRange.Span.TotalSeconds;
            var secondsPerSubrange = (totalSeconds / count) + 1; // to be safe

            var rangeStart = timeRange.StartTime;

            // This differs from Forward in that we flip + for - and < for >
            while (rangeStart >= timeRange.EndTime)
            {
                var rangeEnd = new AFTime(rangeStart.UtcSeconds - secondsPerSubrange);
                if (rangeEnd < timeRange.EndTime) rangeEnd = timeRange.EndTime;
                yield return new AFTimeRange(rangeStart, rangeEnd);
                rangeStart = rangeEnd.AddPITicks(-1);
            }
        }

    }
}
