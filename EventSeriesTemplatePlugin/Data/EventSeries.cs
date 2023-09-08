using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using static EventSeriesTemplatePlugin.Enums;

namespace EventSeriesTemplatePlugin.Data
{
    public class EventSeries
    {
        public EventSeries(IPluginExecutionContext context, ImageTypes imageType)
        {
            if (imageType == ImageTypes.PreImage)
            {
                if (context.PreEntityImages.ContainsKey(Constants.ESPreImage) &&
                    context.PreEntityImages[Constants.ESPreImage].Contains(Constants.EvtSeries_Name))
                {
                    Name = (string)context.PreEntityImages[Constants.ESPreImage][Constants.EvtSeries_Name];
                }
            }

            if (imageType == ImageTypes.PreImage)
            {
                if (context.PreEntityImages.ContainsKey(Constants.ESPreImage) &&
                    context.PreEntityImages[Constants.ESPreImage].Contains(Constants.EvtSeries_Description))
                {
                    Description = (string)context.PreEntityImages[Constants.ESPreImage][Constants.EvtSeries_Description];
                }
            }

            if (context.PreEntityImages.ContainsKey(Constants.ESPreImage) && context.PreEntityImages[Constants.ESPreImage].Contains(Constants.EvtSeries_CourseType))
            {
                CourseType = (CourseType)((OptionSetValue)context.PreEntityImages[Constants.ESPreImage][Constants.EvtSeries_CourseType]).Value;
            }

            if (context.PreEntityImages.ContainsKey(Constants.ESPreImage) && context.PreEntityImages[Constants.ESPreImage].Contains(Constants.EvtSeries_ClassFrequency))
            {
                ClassFrequency = (RecurrenceTypes)((OptionSetValue)context.PreEntityImages[Constants.ESPreImage][Constants.EvtSeries_ClassFrequency]).Value;
            }

            if (context.PreEntityImages.ContainsKey(Constants.ESPreImage) && context.PreEntityImages[Constants.ESPreImage].Contains(Constants.EvtSeries_PrimarySeriesTemplate))
            {
                PrimarySeriesTemplate = (EntityReference)context.PreEntityImages[Constants.ESPreImage][Constants.EvtSeries_PrimarySeriesTemplate];
                EventSeriesTemplate = new EventSeriesTemplate();
            }

            if (context.PreEntityImages.ContainsKey(Constants.ESPreImage) &&
                context.PreEntityImages[Constants.ESPreImage].Contains(Constants.EvtSeries_CreateEvts))
            {
                CreateEvents = (bool)context.PreEntityImages[Constants.ESPreImage][Constants.EvtSeries_CreateEvts];
            }

            if (context.PreEntityImages.ContainsKey(Constants.ESPreImage) &&
                context.PreEntityImages[Constants.ESPreImage].Contains(Constants.EvtSeries_EvtsCreated))
            {
                EvtsCreated = (bool)context.PreEntityImages[Constants.ESPreImage][Constants.EvtSeries_EvtsCreated];
            }
        }

        public void UpdateEntityFromExecutionContext(Entity entity)
        {
            if (entity.Attributes.ContainsKey(Constants.EvtSeries_Name))
            {
                this.Name = (string)entity[Constants.EvtSeries_Name];
            }

            if (entity.Attributes.ContainsKey(Constants.EvtSeries_Description))
            {
                this.Description = (string)entity[Constants.EvtSeries_Description];
            }

            if (entity.Attributes.ContainsKey(Constants.EvtSeries_CourseType))
            {
                this.CourseType = (CourseType)(int)entity[Constants.EvtSeries_CourseType];
            }

            if (entity.Attributes.ContainsKey(Constants.EvtSeries_ClassFrequency))
            {
                this.ClassFrequency = (RecurrenceTypes)(int)entity[Constants.EvtSeries_ClassFrequency];
            }

            if (entity.Attributes.ContainsKey(Constants.EvtSeries_PrimarySeriesTemplate))
            {
                this.PrimarySeriesTemplate = (EntityReference)entity[Constants.EvtSeries_PrimarySeriesTemplate];
            }
            if (entity.Attributes.ContainsKey(Constants.EvtSeries_CreateEvts))
            {
                this.CreateEvents = (bool)entity[Constants.EvtSeries_CreateEvts];
            }

            if (entity.Attributes.ContainsKey(Constants.EvtSeries_EvtsCreated))
            {
                this.EvtsCreated = (bool)entity[Constants.EvtSeries_EvtsCreated];
            }

            this.Entity = entity;
        }

        public void Update()
        {
            if (this.PrimarySeriesTemplate != null && this.IsNewStage)
            {
                this.UpdateEvtSeriesFromTemplate(this.EventSeriesTemplate);
            }
            else
            {
                this.UpdateEvtSeriesAfterCreateEvts();
            }
        }

        private void UpdateEvtSeriesAfterCreateEvts()
        {
            this.Entity[Constants.EvtSeries_EvtsCreated] = this.EvtsCreated;// ? YesOrNo.Yes : YesOrNo.No;
        }

        private void UpdateEvtSeriesFromTemplate(EventSeriesTemplate eventSeriesTemplate)
        {
            this.Description = eventSeriesTemplate.Description;
            this.ClassFrequency = eventSeriesTemplate.RecurrenceType;
            this.CourseType = eventSeriesTemplate.CourseType;
            this.FirstEventDate = eventSeriesTemplate.StartDate;

            this.Entity[Constants.EvtSeries_Description] = eventSeriesTemplate.Description;
            this.Entity[Constants.EvtSeries_FirstEvtDate] = eventSeriesTemplate.StartDate;
            //this.Entity[Constants.EvtSeries_ClassFrequency] = eventSeriesTemplate.RecurrenceType;
            this.Entity[Constants.EvtSeries_ClassFrequency] = new OptionSetValue((int)eventSeriesTemplate.RecurrenceType);
            this.Entity[Constants.EvtSeries_CourseType] = new OptionSetValue((int)eventSeriesTemplate.CourseType);
        }

        public string Name { get; set; }
        public bool IsNewStage { get; set; }
        public string Description { get; set; }
        public bool CreateEvents { get; set; }
        public bool EvtsCreated { get; set; }
        public DateTime FirstEventDate { get; set; }
        public CourseType? CourseType { get; set; }
        public RecurrenceTypes? ClassFrequency { get; set; }
        public EventSeriesTemplate EventSeriesTemplate { get; set; }
        public EntityReference PrimarySeriesTemplate { get; set; }
        public List<Event> Events { get; set; }
        public Entity Entity { get; set; }
    }
}
