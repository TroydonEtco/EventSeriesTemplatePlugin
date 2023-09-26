using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSeriesTemplatePlugin.Data
{
    public class Event
    {
        public Event()
        {
        }
        public Event(EventTemplates evtTemplate, EventSeries evtSeries)
        {
            this.Name = evtTemplate.Name;
            this.EventSeries = evtSeries;
            this.SequenceNumber = evtTemplate.SequenceNumber;
        }

        public Entity CreateEntity(Entity eventSeriesEntity)
        {
            var eventEntity = new Entity(Constants.Evt_Table);
            eventEntity[Constants.Evt_SequenceNumber] = this.SequenceNumber;
            eventEntity[Constants.OwnerId] = this.Owner;
            eventEntity[Constants.Evt_Name] = this.Name;
            eventEntity[Constants.Evt_EvtSeriesKey] = new EntityReference(eventSeriesEntity.LogicalName, eventSeriesEntity.Id);

            Entity = eventEntity;
            return eventEntity;
        }

        public EventSeries EventSeries { get; set; }
        public EntityReference Owner { get; set; }
        public EntityReference Room { get; set; }
        public EntityReference Venue { get; set; }
        public string Name { get; set; }
        public int SequenceNumber { get; set; }
        public DateTime EventDate { get; set; }

        public Entity Entity { get; set; }
    }
}
