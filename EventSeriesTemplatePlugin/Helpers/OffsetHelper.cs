using EventSeriesTemplatePlugin.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EventSeriesTemplatePlugin.Enums;
using Microsoft.Xrm.Sdk;
using EventSeriesTemplatePlugin.Models;
using Newtonsoft.Json;

namespace EventSeriesTemplatePlugin.Helpers
{
    public static class OffsetHelper
    {
        // Update offset based on recurrence pattern chosen
        public static int CalculateOffsetDaysBasedOnRecurrence(EventSeriesTemplate eventSeriesTemplate, ref int offsetDays)
        {
            switch (eventSeriesTemplate.RecurrenceType)
            {
                case RecurrenceTypes.Daily:
                    offsetDays += 1;
                    break;

                case RecurrenceTypes.Weekly:
                    offsetDays += 7;
                    break;
                case RecurrenceTypes.Fortnightly:
                    offsetDays += 7;
                    break;

                case RecurrenceTypes.Monthly:
                    // Assuming you want to add 30 days for monthly recurrence
                    offsetDays += 30;
                    break;

                case RecurrenceTypes.OtherAdhoc:
                    // Replace this with the desired offset days for the ad-hoc recurrence type
                    offsetDays += 14;
                    break;
            }

            return offsetDays;
        }

        public static void CalculateOffsetDaysForEventSeries(EventSeriesTemplate eventSeriesTemplate, ref int offsetDays, ITracingService tracingService, DateTime currentDay, DateTime endDate, string datesToSkipSerialized = null, bool isFirstLoop = false)
        {
            // Deserialize the string into a List<DateTime>
            tracingService.Trace("Attempting to derserialize dates to skip");
            List<DateRange> datesToSkip = !string.IsNullOrEmpty(datesToSkipSerialized) ? JsonConvert.DeserializeObject<List<DateRange>>(datesToSkipSerialized) : null;

            // Calculate the offset based on the recurrence type
            CalculateOffsetDaysBasedOnRecurrence(eventSeriesTemplate, ref offsetDays);

            // Skip weekends (Saturday and Sunday) and additional dates provided in the datesToSkip list

            if (isFirstLoop) // Check if it's the first loop and the first day
            {
                offsetDays = 0; // Set offset to 0 for the first day
            }
            else
            {
                if (eventSeriesTemplate.RecurrenceType == RecurrenceTypes.MonToWed)
                {
                    if (currentDay.DayOfWeek == DayOfWeek.Saturday)
                    {
                        offsetDays += 2; // Skip Saturday and Sunday
                    }
                    else if (currentDay.DayOfWeek == DayOfWeek.Sunday)
                    {
                        offsetDays += 1; // Skip Sunday
                    }
                    else if (currentDay.DayOfWeek == DayOfWeek.Wednesday)
                    {
                        offsetDays += 1; // Skip Wednesday
                    }
                }
                else if (eventSeriesTemplate.RecurrenceType == RecurrenceTypes.WedToFri)
                {
                    if (currentDay.DayOfWeek == DayOfWeek.Saturday)
                    {
                        offsetDays += 2; // Skip Saturday and Sunday
                    }
                    else if (currentDay.DayOfWeek == DayOfWeek.Sunday)
                    {
                        offsetDays += 1; // Skip Sunday
                    }
                    else if (currentDay.DayOfWeek == DayOfWeek.Friday)
                    {
                        offsetDays += 1; // Skip Friday
                    }
                }

                // Check if the current day falls within any of the date ranges to skip
                if (datesToSkip != null)
                {
                    foreach (var dateRange in datesToSkip)
                    {
                        tracingService.Trace($"Checking if currentDay >= dateRange.From : {currentDay} >= {dateRange.From} " +
                            $"\n && currentDay <= dateRange.To: {currentDay} <=  {dateRange.To}");

                        // Check if currentDay falls within the date range (including From and To)
                        if (currentDay >= dateRange.From && currentDay <= dateRange.To)
                        {
                            // Increment initial offset for the first day
                            offsetDays += 1;

                            // Calculate the number of days between From and To, and increment offsetDays accordingly
                            var daysInDateRange = (dateRange.To - dateRange.From).Days + 1;
                            tracingService.Trace($"Condition met, days in date range to increment will be: {daysInDateRange}");
                            offsetDays += daysInDateRange - 1; // Subtract 1 to account for the initial day
                        }
                    }
                }
            }
        }


    }
}
