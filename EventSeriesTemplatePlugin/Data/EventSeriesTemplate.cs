using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EventSeriesTemplatePlugin.Enums;

namespace EventSeriesTemplatePlugin.Data
{
    public class EventSeriesTemplate
    {
        public EventSeriesTemplate(Entity entity)
        {
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_NumberOfEvts))
            {
                NumberOfEvents = (int)entity[Constants.EvtSeriesTemplate_NumberOfEvts];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_EvtTemplatesCreated))
            {
                EventTemplatesCreated = (bool)entity[Constants.EvtSeriesTemplate_EvtTemplatesCreated];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_CreateEvtTemplates))
            {
                CreateEvtTemplates = (bool)entity[Constants.EvtSeriesTemplate_CreateEvtTemplates];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_Names))
            {
                Name = (string)entity[Constants.EvtSeriesTemplate_Names];
            }
        }

        public void Update(Entity entity)
        {
            // when create evt templates is true, and templates not created, we update
            if (this.CreateEvtTemplates && !this.EventTemplatesCreated)
            {
                this.UpdateEntityAfterCreateEvtTemplates(entity);
            }
        }

        // TODO: Check if it will update entity passed in param, considering it is ref type
        private void UpdateEntityAfterCreateEvtTemplates(Entity entity)
        {
            entity[Constants.EvtSeriesTemplate_NumberOfEvts] = this.NumberOfEvents;
            entity[Constants.EvtSeriesTemplate_EvtTemplatesCreated] = !this.EventTemplatesCreated; // invert current selection
            entity[Constants.EvtSeriesTemplate_CreateEvtTemplates] = this.CreateEvtTemplates;
        }

        public List<EventTemplates> EventTemplates { get; set; }
        public int NumberOfEvents { get; set; }
        public bool EventTemplatesCreated { get; set; }
        public bool CreateEvtTemplates { get; set; }
        public string Name { get; set; }
        public RecurrenceTypes RecurrenceType { get; set; }
        public int ETWithOkElements { get; set; }
        public bool AlLETOk { get; set; }
        public Entity Entity { get; set; }

    }
}
