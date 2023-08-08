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
        private static int stageNumber = 0;

        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract context, tracingService, and organization service
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity evtSeriesTemplateEntity = (Entity)context.InputParameters["Target"];
                if (evtSeriesTemplateEntity.LogicalName != Constants.EvtSeriesTemplate_Table) 
                {
                    return; // Targeting incorrect evt series template entity
                }

                // Create event templates
                CreateEventTemplates(tracingService, service, evtSeriesTemplateEntity);
            }
        }

        private void CreateEventTemplates(ITracingService tracingService, IOrganizationService service, Entity evtSeriesTemplateEntity)
        {
            try
            {
                // Validate we are targeting the correct evt series template entity
                tracingService.Trace($"Stage {++stageNumber}: Validating we are targeting the correct evt series template entity");

                var evtSeriesTemplates = new EventSeriesTemplate(evtSeriesTemplateEntity, tracingService);
                var evtTemplates = new List<EventTemplates>();

                if (evtSeriesTemplates.CreateEvtTemplates && !evtSeriesTemplates.EventTemplatesCreated)
                {
                    // Initialize execute multiple requests.
                    ExecuteMultipleRequest requestWithResults = new ExecuteMultipleRequest()
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
                    tracingService.Trace($"Stage {++stageNumber}: Intitialised execute multiple request objects");

                    // Create event templates
                    // TODO: Check if I need to do a post operation as a separate call or make this call postoperation only
                    // might need to use entity images, due to it being a related entity being updated and not the main entity, post entity image or pre entity image
                    // however might need to do the ET OK columns on the same operation too


                    CreateEventTemplatesInRange(tracingService, evtSeriesTemplates, requestWithResults);

                    // Execute all the requests in the request collection using a single web method call.
                    tracingService.Trace($"Stage {++stageNumber}: Executing all the requests in the request collection.");
                    ExecuteMultipleResponse responseWithResults = (ExecuteMultipleResponse)service.Execute(requestWithResults);
                    tracingService.Trace($"Stage {++stageNumber}: Finished Executing all the requests in the request collection.");

                    // Handle responses
                    HandleResponses(requestWithResults, responseWithResults, tracingService);


                    tracingService.Trace("Setting ET OK data in EST");
                    evtSeriesTemplates.ETWithOkElements = responseWithResults.Responses.Count(r => r.Response != null);
                    evtSeriesTemplates.AlLETOk = !responseWithResults.Responses.Any(r => r.Fault != null);

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
                //throw new InvalidPluginExecutionException(OperationStatus.Retry, 500, ex.Message);
            }
            catch (Exception ex)
            {
                tracingService.Trace("MyPlugin: error: {0}", ex.ToString());
                throw;
            }
        }

        private void CreateEventTemplatesInRange(ITracingService tracingService, EventSeriesTemplate evtSeriesTemplates, ExecuteMultipleRequest requestWithResults)
        {
            DateTime startDate = evtSeriesTemplates.StartDate;
            DateTime currentDate = evtSeriesTemplates.StartDate;
            DateTime endDate = DateTime.UtcNow.AddMonths(evtSeriesTemplates.NumberOfMonths);
            int offsetDays = 0; // Initialize the variable before the loop
            int loopCounter = 1; // Initialize the loop counter
            bool isFirstLoop = true; // Added flag to identify the first loop

            tracingService.Trace($"Stage {++stageNumber}: Initialized Create {nameof(CreateEventTemplatesInRange)} method request.");

            while (currentDate <= endDate)
            {
                tracingService.Trace($"Stage {stageNumber}: Creating event template for current date {currentDate:dd/mm/yyyy}. Offsetdays: {offsetDays}");

                var name = $"{evtSeriesTemplates.Name} {Enum.GetName(typeof(RecurrenceTypes), evtSeriesTemplates.RecurrenceType)} {loopCounter}";

                // Call the CalculateOffsetDaysForEventSeries function and store the result in offset
                OffsetHelper.CalculateOffsetDaysForEventSeries(evtSeriesTemplates, ref offsetDays, startDate, endDate, evtSeriesTemplates.DatesToSkipSerialized, isFirstLoop);

                var evtTemplate = new EventTemplates()
                {
                    SequenceNumber = loopCounter,
                    ElementsAssociated = ElementsAssociatedOptions.NotApplicable,
                    Name = name,
                    OffsetDays = offsetDays,
                };
                evtTemplate.CreateEntity(evtSeriesTemplates.Entity);

                CreateRequest createRequest = new CreateRequest { Target = evtTemplate.Entity };
                requestWithResults.Requests.Add(createRequest);

                loopCounter++;
                currentDate = currentDate.AddDays(1);
                isFirstLoop = false; // Set flag to false after the first loop
            }
        }


        private void HandleResponses(ExecuteMultipleRequest requestWithResults, ExecuteMultipleResponse responseWithResults, ITracingService tracingService)
        {
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
        }
    }
}
