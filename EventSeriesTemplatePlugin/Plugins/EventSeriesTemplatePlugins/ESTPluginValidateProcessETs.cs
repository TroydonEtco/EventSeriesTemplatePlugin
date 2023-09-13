using EventSeriesTemplatePlugin.Data;
using EventSeriesTemplatePlugin.Helpers;
using Microsoft.Xrm.Sdk;
using System;
using System.IdentityModel.Metadata;
using System.ServiceModel;

namespace EventSeriesTemplatePlugin.Plugins.EventSeriesTemplatePlugins
{
    public class ESTPluginValidateProcessETs : IPlugin
    {
        private static int stageNumber = 0;

        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract context, tracingService, and organization service
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity evtSeriesTemplateEntity = (Entity)context.InputParameters["Target"];
                    tracingService.Trace($"Stage {++stageNumber}: Validating we are targeting the correct evt series template entity");

                    if (evtSeriesTemplateEntity.LogicalName != Constants.EvtSeriesTemplate_Table)
                    {
                        return; // Targeting incorrect evt series template entity
                    }
                    else
                    {
                        // Validate we are targeting the correct evt series template entity
                        tracingService.Trace($"Stage {++stageNumber}: About to create obj from preimage");

                        var evtSeriesTemplates = new EventSeriesTemplate(context, Enums.ImageTypes.PreImage);
                        if (evtSeriesTemplateEntity.Attributes.ContainsKey(Constants.EvtSeriesTemplate_CreateEvtTemplates))
                        {
                            evtSeriesTemplates.CreateEvtTemplates = (bool)evtSeriesTemplateEntity[Constants.EvtSeriesTemplate_CreateEvtTemplates];
                        }
                        // evtSeriesTemplates.UpdateEntityFromExecutionContext(evtSeriesTemplateEntity);

                        tracingService.Trace($"Stage {++stageNumber}: Conditions to create templates {nameof(evtSeriesTemplates.CreateEvtTemplates)} : {evtSeriesTemplates.CreateEvtTemplates} " +
                            $"{nameof(evtSeriesTemplates.EventTemplatesCreated)} : {evtSeriesTemplates.EventTemplatesCreated}");

                        // if we try to create event templates, and templates are already created, then throw error
                        if (evtSeriesTemplates.CreateEvtTemplates && evtSeriesTemplates.EventTemplatesCreated)
                        {
                            tracingService.Trace("Criteria to create ET not met: Event templates already exist, will not proceed to create Event templates.");
                            throw new InvalidPluginExecutionException("Criteria to create ET not met, will not proceed to create Event templates.");
                        }
                        // if we not creating event templates
                        else if (!evtSeriesTemplates.CreateEvtTemplates)
                        {
                            tracingService.Trace("Criteria to create ET not met: No Request to create event templates, will not proceed to create Event templates.");
                        }
                        else if (EventTemplateSeriesBusinessRules.ShouldCreateTemplates(evtSeriesTemplates))
                        {
                            tracingService.Trace($"Criteria to create ET met: Request to create templates, and templates not created will proceed.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("MyPlugin: error: {0}", ex.ToString());
                throw;
            }
        }

    }
}
