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
        public EventSeriesTemplate(Entity entity, ITracingService tracingService)
        {
            tracingService.Trace($"Attempting to create Entity Template object using evt template entity");

            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_NumberOfEvts))
            {
                NumberOfEvents = (int)entity[Constants.EvtSeriesTemplate_NumberOfEvts];
            }
            tracingService.Trace($"{nameof(NumberOfEvents)} successfully assigned");

            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_EvtTemplatesCreated))
            {
                EventTemplatesCreated = (bool)entity[Constants.EvtSeriesTemplate_EvtTemplatesCreated];
            }
            tracingService.Trace($"{nameof(EventTemplatesCreated)} successfully assigned");

            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_CreateEvtTemplates))
            {
                CreateEvtTemplates = (bool)entity[Constants.EvtSeriesTemplate_CreateEvtTemplates];
            }
            tracingService.Trace($"{nameof(CreateEvtTemplates)} successfully assigned");
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_Names))
            {
                Name = (string)entity[Constants.EvtSeriesTemplate_Names];
            }
            tracingService.Trace($"{nameof(Name)} successfully assigned");
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_DatesToSkip))
            {
                DatesToSkipSerialized = (string)entity[Constants.EvtSeriesTemplate_DatesToSkip];
            }
            tracingService.Trace($"{nameof(DatesToSkipSerialized)} successfully assigned");
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_StartDate))
            {
                StartDate = (DateTime)entity[Constants.EvtSeriesTemplate_StartDate];
            }
            tracingService.Trace($"{nameof(StartDate)} successfully assigned");
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_NumberOfMonths))
            {
                NumberOfMonths = (int)entity[Constants.EvtSeriesTemplate_NumberOfMonths];
            }
            tracingService.Trace($"{nameof(NumberOfMonths)} successfully assigned");
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_ETWithOkElements))
            {
                ETWithOkElements = (int)entity[Constants.EvtSeriesTemplate_ETWithOkElements];
            }
            tracingService.Trace($"{nameof(ETWithOkElements)} successfully assigned");
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_AllETElementOk))
            {
                AlLETOk = (bool)entity[Constants.EvtSeriesTemplate_AllETElementOk];
            }
            tracingService.Trace($"{nameof(AlLETOk)} successfully assigned");

            Entity = entity;
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
            entity[Constants.EvtSeriesTemplate_EvtTemplatesCreated] = true;
            entity[Constants.EvtSeriesTemplate_CreateEvtTemplates] = this.CreateEvtTemplates;
            entity[Constants.EvtSeriesTemplate_ETWithOkElements] = this.ETWithOkElements;
            entity[Constants.EvtSeriesTemplate_AllETElementOk] = this.AlLETOk;
        }

        public List<EventTemplates> EventTemplates { get; set; }
        public int NumberOfEvents { get; set; }
        public int NumberOfMonths { get; set; }
        public DateTime StartDate { get; set; }
        public bool EventTemplatesCreated { get; set; }
        public bool CreateEvtTemplates { get; set; }
        public string Name { get; set; }
        public string DatesToSkipSerialized { get; set; }
        public RecurrenceTypes RecurrenceType { get; set; }
        public int ETWithOkElements { get; set; }
        public bool AlLETOk { get; set; }
        public Entity Entity { get; set; }
    }
}
