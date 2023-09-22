using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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

        public EventTemplates(Entity entity, EventSeries evtSeries)
        {
            this.EventSeriesTemplate = evtSeries?.EventSeriesTemplate ?? null;

            if (entity.Attributes.ContainsKey(Constants.EvtTemplate_SequenceNumber))
            {
                this.SequenceNumber = (int)entity[Constants.EvtTemplate_SequenceNumber];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtTemplate_OffsetDays))
            {
                this.OffsetDays = (int)entity[Constants.EvtTemplate_OffsetDays];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtTemplate_OwnerId))
            {
                this.Owner = (EntityReference)entity[Constants.EvtTemplate_OwnerId];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeries_Name))
            {
                this.Name = (string)entity[Constants.EvtSeries_Name];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtTemplate_ElementsAssociated))
            {
                this.ElementsAssociated = (int)(YesOrNo)((OptionSetValue)entity[Constants.EvtTemplate_ElementsAssociated]).Value;
            }

            this.Entity = entity;
        }

        public Entity CreateEntity(Entity eventSeriesTemplateEntity)
        {
            var eventTemplateEntity = new Entity(Constants.EvtTemplate_Table);
            eventTemplateEntity[Constants.EvtTemplate_SequenceNumber] = this.SequenceNumber;
            eventTemplateEntity[Constants.EvtTemplate_OffsetDays] = this.OffsetDays;
            eventTemplateEntity[Constants.EvtTemplate_OwnerId] = this.Owner;
            eventTemplateEntity[Constants.EvtTemplate_Name] = this.Name;
            eventTemplateEntity[Constants.EvtTemplate_ElementsAssociated] = new OptionSetValue(this.ElementsAssociated);
            eventTemplateEntity[Constants.EvtTemplate_EvtSeriesTemplateKey] = new EntityReference(eventSeriesTemplateEntity.LogicalName, eventSeriesTemplateEntity.Id);
            //eventTemplateEntity[Constants.EvtTemplate_EvtSeriesTemplateId] = eventSeriesTemplateEntity.Id;

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
