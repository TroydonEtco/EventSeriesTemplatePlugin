using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSeriesTemplatePlugin
{
    public class Enums
    {
        public enum ElementsAssociatedOptions
        {
            [Description("Yes")]
            Yes = 0,
            [Description("No")]
            No = 1,
            [Description("Not Applicable")]
            NotApplicable = 2,
        }

        public enum RecurrenceTypes
        {
            Daily = 0,
            Weekly = 1,
            Fortnightly = 2,
            MonToWed = 3,
            WedToFri = 4,
            Monthly = 5,
            OtherAdhoc = 6,
        }

        public enum ImageTypes
        {
            PreImage = 0,
            PostImage = 1,
        }

        public enum YesOrNo
        {
            No = 0,
            Yes = 1,
        }

        public enum CourseType
        {
            EliteClasses = 0,
            BlockCourseClasses = 1,
            NightClasses = 2,
            SiteSafeClasses = 3,
            ConstructSafeClasses = 4,
            EWPClasses = 5,
            DayReleaseClasses = 6,
            FirstAidClasses = 7,
            Foundation = 8,
            Other = 9,
        }
    }
}
