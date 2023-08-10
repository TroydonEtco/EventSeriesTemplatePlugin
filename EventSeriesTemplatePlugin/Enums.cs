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
            Monthly = 2,
            OtherAdhoc = 3,
        }

        public enum ImageTypes
        {
            PreImage = 0,
            PostImage = 1,
        }
    }
}
