using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using EventSeriesTemplatePlugin.Data;
using EventSeriesTemplatePlugin.Helpers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using static EventSeriesTemplatePlugin.Enums;

namespace EventSeriesTemplatePlugin
{
    public class EventSeriesTemplatePluginCode : IPlugin
    {
        private static int failures = 0;
        private static int successes = 0;
        private static int skipped = 0;
        
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            var targetEntity = (Entity)context.InputParameters["Target"];


       
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    // Obtain the target entity from the input parameters.
                    Entity evtSeriesTemplateEntity = (Entity)context.InputParameters["Target"];
                try
                {
                    int stageNumber = 0;
                    // Verify that the target entity represents an evt template
                    // If not, this plug-in was not registered correctly.
                    tracingService.Trace($"Stage {++stageNumber}: Validating we are targeting the correct evt series template entity");

                    if (evtSeriesTemplateEntity.LogicalName != "cr49c_eventseriestemplate")
                    {
                        tracingService.Trace($"Stage {++stageNumber}: Targeting incorrect evt series template entity");
                        return;
                    }

                    var evtSeriesTemplates = new EventSeriesTemplate(evtSeriesTemplateEntity);

                    var evtTemplates = new List<EventTemplates>();

                    #region Create multiple event templates logic
                    // Create a event template (Update later to create array of evt templs based on no.of events field)
                    if (evtSeriesTemplates.CreateEvtTemplates && !evtSeriesTemplates.EventTemplatesCreated)
                    {
                        tracingService.Trace($"Stage {++stageNumber}: Initializing execute multiple requests.");
                        ExecuteMultipleRequest requestWithResults = null;
                        requestWithResults = new ExecuteMultipleRequest()
                        {
                            // Assign settings that define execution behavior: continue on error, return responses. 
                            Settings = new ExecuteMultipleSettings()
                            {
                                ContinueOnError = false,
                                ReturnResponses = true
                            },
                            // Create an empty organization request collection.
                            Requests = new OrganizationRequestCollection()
                        };

                        // Create several (local, in memory) entities in a collection.
                        tracingService.Trace($"Stage {++stageNumber}: Commencing creation of event templates.");
                        DateTime currenUtcDT = DateTime.UtcNow;
                        for (int i = 1; i < evtSeriesTemplates.NumberOfEvents + 1; i++)
                        {
                            var name = $"{evtSeriesTemplates.Name} {Enum.GetName(typeof(RecurrenceTypes), evtSeriesTemplates.RecurrenceType)} {i}";
                            var offsetDays = OffsetHelper.CalculateOffsetDays(evtSeriesTemplates);

                            var evtTemplate = new EventTemplates() 
                            {
                                SequenceNumber = i,
                                ElementsAssociated = ElementsAssociatedOptions.NotApplicable,
                                Name = name,
                                OffsetDays = offsetDays,
                            };
                            var evtTemplateEntity = evtTemplate.CreateEntity(evtSeriesTemplateEntity);

                            tracingService.Trace($"Stage {++stageNumber}: Creation of event template. Sequence no - {i}");
                            CreateRequest createRequest = new CreateRequest { Target = evtTemplateEntity };
                            requestWithResults.Requests.Add(createRequest);
                        }

                        // Execute all the requests in the request collection using a single web method call.
                        tracingService.Trace($"Stage {++stageNumber}: Executing all the requests in the request collection.");
                        ExecuteMultipleResponse responseWithResults =
                            (ExecuteMultipleResponse)service.Execute(requestWithResults);

                        // Display the results returned in the responses.
                        foreach (var responseItem in responseWithResults.Responses)
                        {
                            // A valid response.
                            if (responseItem.Response != null)
                                LogHelpers.DisplayResponse(requestWithResults.Requests[responseItem.RequestIndex], responseItem.Response, ref successes);

                            // An error has occurred.
                            else if (responseItem.Fault != null)
                            {
                                LogHelpers.DisplayFault(requestWithResults.Requests[responseItem.RequestIndex], responseItem.RequestIndex, responseItem.Fault, ref failures);
                                throw new InvalidPluginExecutionException($"A fault in Plugin Event Series Template - when upserting multiple records in " +
                                    $"{responseWithResults.Responses.Count} at sequence '{responseItem.RequestIndex + 1}' request, with a fault message: {responseItem.Fault.Message}");
                            }
                        }

                        // TODO: Create a post operation followup plugin class to get the count of elements with ET=OK then set the count in the EST and All Elements=OK
                        tracingService.Trace("FollowupPlugin: Successfully created the event templates.");

                        #endregion

                        evtSeriesTemplates.EventTemplates.AddRange(evtTemplates);
                        evtSeriesTemplates.Update(evtSeriesTemplateEntity); // update the referenced entity
                        service.Update(evtSeriesTemplateEntity); // then use org service to update in dataverse
                    }
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    // Throw this only when using synchronous prevalidation operation. This is displayed to end user.
                    // Throw exception like this when plugin failed.
                    throw new InvalidPluginExecutionException("The following error occurred in MyPlugin.", ex);
                    throw new InvalidPluginExecutionException(OperationStatus.Retry, 500, ex.Message);
                }
                catch (Exception ex)
                {
                    tracingService.Trace("MyPlugin: error: {0}", ex.ToString());
                    throw;
                }
            }

        }
    }
}
