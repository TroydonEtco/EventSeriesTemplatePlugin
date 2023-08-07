using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSeriesTemplatePlugin
{
    public class Enums
    {
        public enum ElementsAssociatedOptions
        {
            Yes = 0,
            No = 1,
            NotApplicable = 2,
        }

        public enum RecurrenceTypes
        {
            Daily = 0,
            Weekly = 1,
            Monthly = 2,
            OtherAdhoc = 3,
        }
    }
}
