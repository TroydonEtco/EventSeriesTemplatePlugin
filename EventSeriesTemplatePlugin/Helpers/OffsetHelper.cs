using EventSeriesTemplatePlugin.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EventSeriesTemplatePlugin.Enums;
using System.Text.Json;

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

        public static void CalculateOffsetDaysForEventSeries(EventSeriesTemplate eventSeriesTemplate, ref int offsetDays, DateTime startDate, DateTime currentDay, DateTime endDate, string datesToSkipSerialized = null, bool isFirstLoop = false)
        {
            // Deserialize the string into a List<DateTime>
            List<DateTime> datesToSkip = !string.IsNullOrEmpty(datesToSkipSerialized) ? JsonSerializer.Deserialize<List<DateTime>>(datesToSkipSerialized) : null;

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

                // Skip any additional dates provided in the datesToSkip list
                if (datesToSkip != null && datesToSkip.Contains(currentDay.Date))
                {
                    offsetDays += 1;
                }
            }
        }


    }
}
