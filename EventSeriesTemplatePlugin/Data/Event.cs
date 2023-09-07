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
        public Event(Event _event)
        {
            this.Name = _event.Name;
            this.EventSeries = _event.EventSeries;
            this.EventDate = _event.EventDate;
            this.Name = _event.Name;
            this.Name = _event.Name;
            this.Name = _event.Name;
        }

        public Entity CreateEntity(Entity eventSeriesEntity)
        {
            var eventTemplateEntity = new Entity(Constants.EvtTemplate_Table);
            eventTemplateEntity["new_sequencenumber"] = this.SequenceNumber;
            eventTemplateEntity["new_offsetdays"] = this.OffsetDays;
            eventTemplateEntity["ownerid"] = this.Owner;
            eventTemplateEntity["oas_room"] = this.Room;
            eventTemplateEntity["new_name"] = this.Name;
           // eventTemplateEntity["new_elementsassociated"] = new OptionSetValue(this.ElementsAssociated);
            eventTemplateEntity["new_eventseriestemplate"] = new EntityReference(eventSeriesEntity.LogicalName, eventSeriesEntity.Id);

            Entity = eventTemplateEntity;
            return eventTemplateEntity;
        }

        public EventSeries EventSeries { get; set; }
        public EntityReference Owner { get; set; }
        public EntityReference Room { get; set; }
        public string Name { get; set; }
        public DateTime EventDate { get; set; }

        public Entity Entity { get; set; }
    }
}
