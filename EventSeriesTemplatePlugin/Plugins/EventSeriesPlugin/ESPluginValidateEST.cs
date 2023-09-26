using EventSeriesTemplatePlugin.Data;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;
using static EventSeriesTemplatePlugin.Enums;

namespace EventSeriesTemplatePlugin.Plugins.EventSeriesPlugin
{
    /// <summary>
    /// Plugin to validate the Event Series Template is valid, by ensuring the data it needs to fill in exist.
    /// Throws an error and prevents the post operation that creates Event templates from running.
    /// </summary>
    public class ESPluginValidateEST : IPlugin
    {
        private static int stageNumber = 0;

        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract context, tracingService, and organization service
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity evtSeriesEntity = (Entity)context.InputParameters["Target"];
                    tracingService.Trace($"Stage {++stageNumber}: Validating we are targeting the correct evt series template entity");

                    if (evtSeriesEntity.LogicalName != Constants.EvtSeries_Table)
                    {
                        tracingService.Trace($"Stage {++stageNumber}: Invalid entity being targeted");
                        return; // Targeting incorrect evt series template entity
                    }
                    else
                    {
                        // Validate we are targeting the correct evt series template entity
                        if (context.MessageName == "Create")
                        {
                            tracingService.Trace($"Stage {++stageNumber}: Context message name is: {context.MessageName}");
                            tracingService.Trace($"Stage {++stageNumber}: About to create obj from context entity");
                            var evtSeries = new EventSeries(evtSeriesEntity);
                            evtSeries.UpdateEntityFromExecutionContext(evtSeriesEntity);
                            tracingService.Trace($"Stage {++stageNumber}: Created obj from context entity");

                            if (evtSeries.PrimarySeriesTemplate != null)
                            {
                                ValidateESTTemplate(tracingService, service, evtSeries);
                            }
                            else
                            {
                                tracingService.Trace("Criteria to create Events not met: No Primary series templates elected to be used");
                            }
                        }
                        else if (context.MessageName == "Update")
                        {
                            tracingService.Trace($"Stage {++stageNumber}: Context message name is: {context.MessageName}");
                            tracingService.Trace($"Stage {++stageNumber}: About to create obj from preimage");
                            var evtSeries = new EventSeries(context, ImageTypes.PreImage);
                            evtSeries.UpdateEntityFromExecutionContext(evtSeriesEntity);
                            tracingService.Trace($"Stage {++stageNumber}: Created obj from preimage");

                            if (evtSeries.PrimarySeriesTemplate != null)
                            {
                                ValidateESTTemplate(tracingService, service, evtSeries);
                            }
                            else
                            {
                                tracingService.Trace("Criteria to create Events not met: No Primary series templates elected to be used");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("MyPlugin: error: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("An error ocurred when attempting to save Event Series.");
            }
        }

        private static void ValidateESTTemplate(ITracingService tracingService, IOrganizationService service, EventSeries evtSeries)
        {
            // retreive template
            ColumnSet columns = new ColumnSet(Constants.EvtSeries_CourseType, Constants.EvtSeriesTemplate_RecurrenceType, Constants.EvtSeriesTemplate_Description); // List the properties you want to retrieve

            tracingService.Trace($"Stage {++stageNumber}: Retrieving related evt series template");
            // Retrieve the entity record based on the EntityReference, then update the props
            Entity entity = service.Retrieve(evtSeries.PrimarySeriesTemplate.LogicalName, evtSeries.PrimarySeriesTemplate.Id, columns);

            tracingService.Trace($"Stage {++stageNumber}: About to create evt series template obj from retrieved entity");
            evtSeries.EventSeriesTemplate.UpdateEntityFromExecutionContext(entity);
            tracingService.Trace($"Stage {++stageNumber}: Created evt series template obj from retrieved entity");

            // pull data from evt series template, these attrs below need to check the template not the evt series
            tracingService.Trace($"Stage {++stageNumber}: Conditions to create events require the following to contain data: " +
                                $"{nameof(evtSeries.EventSeriesTemplate.Description)} : {(evtSeries.EventSeriesTemplate.Description != null ? evtSeries.EventSeriesTemplate.Description : "NULL")} " +
                                $"{nameof(evtSeries.EventSeriesTemplate.CourseType)} : {(evtSeries.EventSeriesTemplate.CourseType != null ? Enum.GetName(typeof(CourseType), evtSeries.EventSeriesTemplate.CourseType) : "NULL")} " +
                                $"{nameof(evtSeries.EventSeriesTemplate.RecurrenceType)} : {(evtSeries.EventSeriesTemplate.RecurrenceType != null ? Enum.GetName(typeof(RecurrenceTypes), evtSeries.EventSeriesTemplate.RecurrenceType) : "NULL")}");

            // if we try to create event templates, and templates are already created, then throw error
            if (string.IsNullOrEmpty(evtSeries.EventSeriesTemplate.Description) || evtSeries.EventSeriesTemplate.CourseType == null || evtSeries.EventSeriesTemplate.RecurrenceType == null || evtSeries.EvtsCreated)
            {
                tracingService.Trace("Criteria to create Events not met: One or more required fields from Event series templates is missing.");
                throw new InvalidPluginExecutionException("Criteria to create Events not met: One or more required fields from Event series templates is missing.");
            }
            else
            {
                tracingService.Trace("Criteria to create Events has been met: All required fields include data required to create events.");
            }
        }
    }
}
