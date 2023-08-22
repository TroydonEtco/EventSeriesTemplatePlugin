using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
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
        public EventTemplates()
        {
        }
        public EventTemplates(EventTemplates eventTemplates)
        {
            this.EventSeriesTemplate = eventTemplates.EventSeriesTemplate;
            this.SequenceNumber = eventTemplates.SequenceNumber;
            this.OffsetDays = eventTemplates.OffsetDays;
            this.Owner = eventTemplates.Owner;
            this.Name = eventTemplates.Name;
            this.ElementsAssociated = eventTemplates.ElementsAssociated;
            this.Entity = eventTemplates.Entity;
        }

        public Entity CreateEntity(Entity eventSeriesTemplateEntity)
        {
            var eventTemplateEntity  = new Entity(Constants.EvtTemplate_Table);
            eventTemplateEntity["new_sequencenumber"] = this.SequenceNumber;
            eventTemplateEntity["new_offsetdays"] = this.OffsetDays;
            eventTemplateEntity["ownerid"] = this.Owner;
            eventTemplateEntity["new_name"] = this.Name;
            eventTemplateEntity["new_elementsassociated"] = new OptionSetValue(this.ElementsAssociated);
            eventTemplateEntity["new_eventseriestemplate"] = new EntityReference(eventSeriesTemplateEntity.LogicalName, eventSeriesTemplateEntity.Id);

            Entity = eventTemplateEntity;
            return eventTemplateEntity;
        }

        public EventSeriesTemplate EventSeriesTemplate { get; set; }
        public int SequenceNumber { get; set; }
        public int OffsetDays { get; set; }
        public EntityReference Owner { get; set; }
        public string Name { get; set; }
        public int ElementsAssociated { get; set; }
        public Entity Entity { get; set; }
    }
}
