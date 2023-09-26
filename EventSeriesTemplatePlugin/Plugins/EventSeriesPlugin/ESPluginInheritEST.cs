using EventSeriesTemplatePlugin.Data;
using EventSeriesTemplatePlugin.Services.BPF;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using static EventSeriesTemplatePlugin.Enums;

namespace EventSeriesTemplatePlugin.Plugins.EventSeriesPlugin
{
    /// <summary>
    /// Plugin to inherit Event Series Template properties
    /// </summary>
    public class ESPluginInheritEST : IPlugin
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
                    if (evtSeriesEntity.LogicalName != Constants.EvtSeries_Table)
                    {
                        tracingService.Trace($"Stage {++stageNumber}: Invalid target entity. Entity used: {evtSeriesEntity.LogicalName}, Entity expedcted: {Constants.EvtSeries_Table}");
                        return;
                    }

                    tracingService.Trace($"Stage {++stageNumber}: Initialising Event Series object");
                    if (context.MessageName == "Create")
                    {
                        tracingService.Trace($"Stage {++stageNumber}: Context message name is: {context.MessageName}");
                        var evtSeries = new EventSeries(evtSeriesEntity);
                        tracingService.Trace($"Stage {++stageNumber}: Updating Event Series From Execution Context");
                        evtSeries.UpdateEntityFromExecutionContext(evtSeriesEntity);
                        // only inherit when primary series template is selected, and when we are on the New Stage part of the BPF
                        if (evtSeries.PrimarySeriesTemplate != null)
                        {
                            tracingService.Trace($"Stage {++stageNumber}: Primary series template object available");
                            InheritEventSeriesTemplate(context, tracingService, service, evtSeries);
                        }
                        else
                        {
                            tracingService.Trace("No Primary Series Template selected, inheritance will not be done");
                        }
                    } 
                    else if (context.MessageName == "Update")
                    {
                        tracingService.Trace($"Stage {++stageNumber}: Context message name is: {context.MessageName}");
                        tracingService.Trace($"Stage {++stageNumber}: Updating Event Series From Execution Context");
                        var evtSeries = new EventSeries(context, ImageTypes.PreImage);
                        evtSeries.UpdateEntityFromExecutionContext(evtSeriesEntity);
                        // only inherit when primary series template is selected, and when we are on the New Stage part of the BPF
                        if (evtSeries.PrimarySeriesTemplate != null)
                        {
                            tracingService.Trace($"Stage {++stageNumber}: Primary series template object available");
                            InheritEventSeriesTemplate(context, tracingService, service, evtSeries);
                        }
                        else
                        {
                            tracingService.Trace("No Primary Series Template selected, inheritance will not be done");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("MyPlugin: error: {0}", ex.ToString());
            }
        }

        private void InheritEventSeriesTemplate(IPluginExecutionContext context, ITracingService tracingService, IOrganizationService service, EventSeries evtSeries)
        {
            // only process inheritance when the active stage is at new
            var bpfService = new BPFService();
            tracingService.Trace($"Stage {++stageNumber}: Attempting to check the active stage of the BPF");
            var isNewStage = bpfService.IsNewStageOnBPF(tracingService, service, evtSeries.Entity, Constants.EvtSeries_Stage_New);
            evtSeries.IsNewStage = isNewStage;

            if (isNewStage || context.MessageName == "Create")
            {
                // Retrieve evt series template data
                tracingService.Trace($"Stage {++stageNumber}: Attempting to retreive event series template data entity");
                ColumnSet evtSeriesColumns = new ColumnSet(Constants.EvtSeries_CourseType, Constants.EvtSeriesTemplate_RecurrenceType, Constants.EvtSeriesTemplate_Description,
                    Constants.EvtSeriesTemplate_DatesToSkip, Constants.EvtSeriesTemplate_StartDate);
                Entity evtSeriesTemplateEntity = service.Retrieve(evtSeries.PrimarySeriesTemplate.LogicalName, evtSeries.PrimarySeriesTemplate.Id, evtSeriesColumns);
                tracingService.Trace($"Stage {++stageNumber}: Attempting to update event series template data from execution context");
                evtSeries.EventSeriesTemplate.UpdateEntityFromExecutionContext(evtSeriesTemplateEntity);

                // pass inheritable data to evtseries
                tracingService.Trace($"Stage {++stageNumber}: Attempting to update event series data from event series template data");
                evtSeries.Update();
                tracingService.Trace($"Stage {++stageNumber}: Attempting to call the update API");
                service.Update(evtSeries.Entity);
            }
            else
            {
                tracingService.Trace($"Stage {++stageNumber}: Not in the required stage, inheritance will not be done");
            }
        }
    }
}
