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
using OSIsoft.AF.Asset;
using OSIsoft.AF.PI;
using OSIsoft.AF.Data;
using System.Threading;

namespace PIDevClub.Library.AFExtensions
{
    public static class Data
    {
        /// <summary>
        /// Returns the event-weighted count of a <see cref="PIPoint"/> over the requested <see cref="AFTimeRange"/>.
        /// </summary>
        /// <param name="tag">A PIPoint</param>
        /// <param name="timeRange">An AFTimeRange</param>
        /// <returns>A scalar int of the events within the time range.</returns>
        public static int GetEventCount(this PIPoint tag, AFTimeRange timeRange)
        {
            var summaries = tag.Summary(timeRange, AFSummaryTypes.Count, AFCalculationBasis.EventWeighted, AFTimestampCalculation.Auto);
            AFValue summary;
            if (summaries.TryGetValue(AFSummaryTypes.Count, out summary))
            {
                if (summary.IsGood)
                {
                    // One day this may have to be an Int64 or long
                    return Convert.ToInt32(summary.Value);
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns the event-weighted count of a <see cref="PIPoint"/> over the requested <see cref="AFTimeRange"/>.
        /// </summary>
        /// <param name="tag">A PIPoint</param>
        /// <param name="timeRange">An AFTimeRange</param>
        /// <returns>A scalar int of the events within the time range.</returns>
        public static async Task<int> GetEventCountAsync(this PIPoint tag, AFTimeRange timeRange, CancellationToken cancellationToken = default(CancellationToken))
        {
            var summaries = await tag.SummaryAsync(timeRange,
                                                   AFSummaryTypes.Count,
                                                   AFCalculationBasis.EventWeighted,
                                                   AFTimestampCalculation.Auto,
                                                   cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            cancellationToken.ThrowIfCancellationRequested();
            AFValue summary;
            if (summaries.TryGetValue(AFSummaryTypes.Count, out summary))
            {
                if (summary.IsGood)
                {
                    // One day this may have to be an Int64 or long
                    return Convert.ToInt32(summary.Value);
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns the event-weighted count of an <see cref="AFAttribute"/> over the requested <see cref="AFTimeRange"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="timeRange"></param>
        /// <returns>A scalar int of the events within the time range.</returns>
        public static int GetEventCount(this AFAttribute attribute, AFTimeRange timeRange)
        {
            var summaries = attribute.Data.Summary(timeRange, AFSummaryTypes.Count, AFCalculationBasis.EventWeighted, AFTimestampCalculation.Auto);
            AFValue summary;
            if (summaries.TryGetValue(AFSummaryTypes.Count, out summary))
            {
                if (summary.IsGood)
                {
                    // One day this may have to be an Int64 or long
                    return Convert.ToInt32(summary.Value);
                }
            }
            return 0;
        }


        /// <summary>
        /// Returns the event-weighted count of an <see cref="AFAttribute"/> over the requested <see cref="AFTimeRange"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="timeRange"></param>
        /// <returns>A scalar int of the events within the time range.</returns>
        public static async Task<int> GetEventCountAsync(this AFAttribute attribute, AFTimeRange timeRange, CancellationToken cancellationToken = default(CancellationToken))
        {
            var summaries = await attribute.Data.SummaryAsync(timeRange, 
                                                              AFSummaryTypes.Count, 
                                                              AFCalculationBasis.EventWeighted, 
                                                              AFTimestampCalculation.Auto, 
                                                              cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            cancellationToken.ThrowIfCancellationRequested();
            AFValue summary;
            if (summaries.TryGetValue(AFSummaryTypes.Count, out summary))
            {
                if (summary.IsGood)
                {
                    // One day this may have to be an Int64 or long
                    return Convert.ToInt32(summary.Value);
                }
            }
            return 0;
        }
    }
}
