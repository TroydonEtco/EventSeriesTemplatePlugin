using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using static EventSeriesTemplatePlugin.Enums;

namespace EventSeriesTemplatePlugin.Data
{
    public class EventSeriesTemplate
    {
        public EventSeriesTemplate()
        {
        }
        /// <summary>
        /// Creates an event series template object, and passes in data based on the image type.
        /// It ensures the image contains the key prop before assigning it the value.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="imageType"></param>
        public EventSeriesTemplate(IPluginExecutionContext context, ImageTypes imageType)
        {
            if (imageType == ImageTypes.PreImage)
            {
                if (context.PreEntityImages.ContainsKey(Constants.ESTPreImage) &&
                    context.PreEntityImages[Constants.ESTPreImage].Contains(Constants.EvtSeriesTemplate_NumberOfEvts))
                {
                    NumberOfEvents = (int)context.PreEntityImages[Constants.ESTPreImage][Constants.EvtSeriesTemplate_NumberOfEvts];
                }
                if (context.PreEntityImages.ContainsKey(Constants.ESTPreImage) &&
                    context.PreEntityImages[Constants.ESTPreImage].Contains(Constants.EvtSeriesTemplate_EvtTemplatesCreated))
                {
                    EventTemplatesCreated = (bool)context.PreEntityImages[Constants.ESTPreImage][Constants.EvtSeriesTemplate_EvtTemplatesCreated];
                }
                if (context.PreEntityImages.ContainsKey(Constants.ESTPreImage) &&
                    context.PreEntityImages[Constants.ESTPreImage].Contains(Constants.EvtSeriesTemplate_CreateEvtTemplates))
                {
                    CreateEvtTemplates = (bool)context.PreEntityImages[Constants.ESTPreImage][Constants.EvtSeriesTemplate_CreateEvtTemplates];
                }
                if (context.PreEntityImages.ContainsKey(Constants.ESTPreImage) && context.PreEntityImages[Constants.ESTPreImage].Contains(Constants.EvtSeriesTemplate_Names))
                {
                    Name = (string)context.PreEntityImages[Constants.ESTPreImage][Constants.EvtSeriesTemplate_Names];
                }
                if (context.PreEntityImages.ContainsKey(Constants.ESTPreImage) && context.PreEntityImages[Constants.ESTPreImage].Contains(Constants.EvtSeriesTemplate_DatesToSkip))
                {
                    DatesToSkipSerialized = (string)context.PreEntityImages[Constants.ESTPreImage][Constants.EvtSeriesTemplate_DatesToSkip];
                }
                if (context.PreEntityImages.ContainsKey(Constants.ESTPreImage) && context.PreEntityImages[Constants.ESTPreImage].Contains(Constants.EvtSeriesTemplate_StartDate))
                {
                    StartDate = Convert.ToDateTime(context.PreEntityImages[Constants.ESTPreImage][Constants.EvtSeriesTemplate_StartDate]);
                }
                if (context.PreEntityImages.ContainsKey(Constants.ESTPreImage) && context.PreEntityImages[Constants.ESTPreImage].Contains(Constants.EvtSeriesTemplate_NumberOfMonths))
                {
                    NumberOfMonths = (int)context.PreEntityImages[Constants.ESTPreImage][Constants.EvtSeriesTemplate_NumberOfMonths];
                }
                if (context.PreEntityImages.ContainsKey(Constants.ESTPreImage) && context.PreEntityImages[Constants.ESTPreImage].Contains(Constants.EvtSeriesTemplate_ETWithOkElements))
                {
                    ETWithOkElements = (int)context.PreEntityImages[Constants.ESTPreImage][Constants.EvtSeriesTemplate_ETWithOkElements];
                }
                if (context.PreEntityImages.ContainsKey(Constants.ESTPreImage) && context.PreEntityImages[Constants.ESTPreImage].Contains(Constants.EvtSeriesTemplate_AllETElementOk))
                {
                    AllETOk = (bool)context.PreEntityImages[Constants.ESTPreImage][Constants.EvtSeriesTemplate_AllETElementOk];
                }
                if (context.PreEntityImages.ContainsKey(Constants.ESTPreImage) && context.PreEntityImages[Constants.ESTPreImage].Contains(Constants.EvtSeriesTemplate_RecurrenceType))
                {
                    RecurrenceType = (RecurrenceTypes)((OptionSetValue)context.PreEntityImages[Constants.ESTPreImage][Constants.EvtSeriesTemplate_RecurrenceType]).Value;
                }
            }
        }


        public void CreateEntityFromPreImage(Entity entity)
        {
            // when create evt templates is true, and templates not created, we update
            if (this.CreateEvtTemplates && !this.EventTemplatesCreated)
            {
                this.UpdateEntityAfterCreateEvtTemplates();
            }
        }

        public void Update(Entity entity)
        {
            // when create evt templates is true, and templates not created, we update
            if (this.CreateEvtTemplates && !this.EventTemplatesCreated)
            {
                this.UpdateEntityAfterCreateEvtTemplates();
            }
        }

        // TODO: Check if it will update entity passed in param, considering it is ref type
        private void UpdateEntityAfterCreateEvtTemplates()
        {
            this.Entity[Constants.EvtSeriesTemplate_NumberOfEvts] = this.NumberOfEvents;
            this.Entity[Constants.EvtSeriesTemplate_EvtTemplatesCreated] = true;
            this.Entity[Constants.EvtSeriesTemplate_CreateEvtTemplates] = this.CreateEvtTemplates;
            this.Entity[Constants.EvtSeriesTemplate_ETWithOkElements] = this.ETWithOkElements;
            this.Entity[Constants.EvtSeriesTemplate_AllETElementOk] = this.AllETOk;
        }

        /// <summary>
        /// Updates this objects props based on attributes passed in the step.
        /// </summary>
        /// <param name="entity"></param>
        public void UpdateEntityFromExecutionContext(Entity entity)
        {
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_NumberOfEvts))
            {
                this.NumberOfEvents = (int)entity[Constants.EvtSeriesTemplate_NumberOfEvts];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_EvtTemplatesCreated))
            {
                this.EventTemplatesCreated = (bool)entity[Constants.EvtSeriesTemplate_EvtTemplatesCreated];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_CreateEvtTemplates))
            {
                this.CreateEvtTemplates = (bool)entity[Constants.EvtSeriesTemplate_CreateEvtTemplates];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_Names))
            {
                this.Name = (string)entity[Constants.EvtSeriesTemplate_Names];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_Description))
            {
                this.Description = (string)entity[Constants.EvtSeriesTemplate_Description];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_DatesToSkip))
            {
                this.DatesToSkipSerialized = (string)entity[Constants.EvtSeriesTemplate_DatesToSkip];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_StartDate))
            {
                this.StartDate = DateTime.Parse((string)entity[Constants.EvtSeriesTemplate_StartDate]);
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_NumberOfMonths))
            {
                this.NumberOfMonths = (int)entity[Constants.EvtSeriesTemplate_NumberOfMonths];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_ETWithOkElements))
            {
                this.ETWithOkElements = (int)entity[Constants.EvtSeriesTemplate_ETWithOkElements];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_AllETElementOk))
            {
                this.AllETOk = (bool)entity[Constants.EvtSeriesTemplate_AllETElementOk];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_RecurrenceType))
            {
                this.RecurrenceType = (RecurrenceTypes)((OptionSetValue)entity[Constants.EvtSeriesTemplate_RecurrenceType]).Value;
            }

            if (entity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_CourseType))
            {
                this.CourseType = (CourseType)((OptionSetValue)entity[Constants.EvtSeriesTemplate_CourseType]).Value;
            }

            this.Entity = entity;
        }

        public List<EventTemplates> EventTemplates { get; set; }
        public int NumberOfEvents { get; set; }
        public int NumberOfMonths { get; set; }
        public DateTime StartDate { get; set; }
        public bool EventTemplatesCreated { get; set; }
        public bool CreateEvtTemplates { get; set; }
        public CourseType? CourseType { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string DatesToSkipSerialized { get; set; }
        public RecurrenceTypes RecurrenceType { get; set; }
        public int ETWithOkElements { get; set; }
        public bool AllETOk { get; set; }
        public Entity Entity { get; set; }
    }
}
