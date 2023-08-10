using EventSeriesTemplatePlugin.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSeriesTemplatePlugin.Helpers
{
    public static class EventTemplateSeriesBusinessRules
    {
        public static bool ShouldCreateTemplates(EventSeriesTemplate evtSeriesTemplates)
        {
            return evtSeriesTemplates.CreateEvtTemplates && !evtSeriesTemplates.EventTemplatesCreated;
        }
    }
}
