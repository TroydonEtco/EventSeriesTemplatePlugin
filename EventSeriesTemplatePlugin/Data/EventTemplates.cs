using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EventSeriesTemplatePlugin.Enums;

namespace EventSeriesTemplatePlugin.Data
{
    public class EventTemplates
    {
        public Entity CreateEntity(Entity eventSeriesTemplateEntity)
        {
            var eventTemplateEntity  = new Entity(Constants.EvtTemplate_Table);
            eventTemplateEntity["new_sequencenumber"] = this.SequenceNumber;
            eventTemplateEntity["new_offsetdays"] = this.OffsetDays;
            eventTemplateEntity["ownerid"] = this.Owner;
            eventTemplateEntity["new_name"] = this.Name;
            eventTemplateEntity["new_elementsassociated"] = this.ElementsAssociated;
            eventTemplateEntity["new_eventseriestemplate"] = eventSeriesTemplateEntity.LogicalName;

            Entity = eventTemplateEntity;
            return eventTemplateEntity;
        }

        public EventSeriesTemplate EventSeriesTemplate { get; set; }
        public int SequenceNumber { get; set; }
        public int OffsetDays { get; set; }
        public bool Owner { get; set; }
        public string Name { get; set; }
        public ElementsAssociatedOptions ElementsAssociated { get; set; }
        public Entity Entity { get; set; }
    }
}
