using EventSeriesTemplatePlugin.Data;
using EventSeriesTemplatePlugin.Helpers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using static EventSeriesTemplatePlugin.Enums;

namespace EventSeriesTemplatePlugin.Plugins.EventSeriesPlugin
{
    public class ESPluginProcessEvts : IPlugin
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
            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity evtSeriesEntity = (Entity)context.InputParameters["Target"];
                    if (evtSeriesEntity.LogicalName != Constants.EvtSeriesTemplate_Table)
                    {
                        tracingService.Trace($"Stage {++stageNumber}: Invalid target entity. Entity used: {evtSeriesEntity.LogicalName}, Entity expedcted: {Constants.EvtSeries_Table}");
                        return; // Targeting incorrect evt series template entity
                    }

                    // Create events
                    tracingService.Trace($"Stage {++stageNumber}: Initialising Event Series object");
                    var evtSeries = new EventSeries(context, ImageTypes.PreImage);
                    tracingService.Trace($"Stage {++stageNumber}: Updating Event Series From Execution Context");
                    evtSeries.UpdateEntityFromExecutionContext(evtSeriesEntity);

                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("MyPlugin: error: {0}", ex.ToString());
                throw;
            }
        }

        private void CreateEvents(IPluginExecutionContext context, ITracingService tracingService, IOrganizationService service, EventSeries evtSeries)
        {
  
            try
            {
                // Validate we are targeting the correct evt series template entity
                tracingService.Trace($"Stage {++stageNumber}: Validating we are targeting the correct evt series template entity");
                tracingService.Trace($"Stage {++stageNumber}: Conditions to create templates {nameof(evtSeries.CreateEvents)} : {evtSeries.CreateEvents}";
                LogHelpers.TracePropertyData(tracingService, evtSeries);

                if (evtSeries.CreateEvents && !evtSeries.EvtsCreated)
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

                    // Create event templates
                    var evts = CreateEventsInRange(context, tracingService, evtSeries, requestWithResults);

                    evtSeries.Events = new List<Event>();
                    evtSeries.Events.AddRange(evts);
                    // Execute all the requests in the request collection using a single web method call.
                    tracingService.Trace($"Stage {++stageNumber}: Executing all the requests in the request collection.");
                    ExecuteMultipleResponse responseWithResults = (ExecuteMultipleResponse)service.Execute(requestWithResults);
                    tracingService.Trace($"Stage {++stageNumber}: Finished Executing all the requests in the request collection.");

                    // Handle responses
                    HandleResponses(requestWithResults, responseWithResults, tracingService);

                    // make updates to event series
                    evtSeries.EvtsCreated = responseWithResults.Responses.Any(r => r.Response != null);
                    evtSeries.Update(); // update the referenced entity
                    service.Update(evtSeries.Entity); // then use org service to update in dataverse
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, 500, ex.Message);
            }
        }

        private IEnumerable<Event> CreateEventsInRange(IPluginExecutionContext context, ITracingService tracingService, EventSeries evtSeries, ExecuteMultipleRequest requestWithResults)
        {
            tracingService.Trace($"Stage {++stageNumber}: Intializing Create {nameof(CreateEventsInRange)} method request.");
            DateTime startDate = evtSeries.StartDate;
            DateTime currentDate = evtSeriesTemplates.StartDate;
            DateTime endDate = startDate.AddMonths(evtSeriesTemplates.NumberOfMonths);
            int offsetDays = 0; // Initialize the variable before the loop
            int oldOffsetDays = 0; // Initialize the variable to store the previous offset
            int loopCounter = 1; // Initialize the loop counter
            bool isFirstLoop = true; // Added flag to identify the first loop
            tracingService.Trace($"Stage {++stageNumber}: Initialized Create {nameof(CreateEventsInRange)} method request." +
                $"{nameof(startDate)} : {startDate} {nameof(currentDate)} : {currentDate} - {nameof(endDate)} : {endDate} - {nameof(evtSeriesTemplates.NumberOfMonths)} : {evtSeriesTemplates.NumberOfMonths}");
            var evtTemplates = new List<EventTemplates>();
            while (currentDate <= endDate)
            {
                tracingService.Trace($"Stage {stageNumber}: Creating event template for current date {currentDate:dd/MM/yyyy}. Offsetdays: {offsetDays}. {nameof(currentDate)} : {currentDate} - {nameof(endDate)} : {endDate}");
                var name = $"{evtSeriesTemplates.Name} {Enum.GetName(typeof(RecurrenceTypes), evtSeriesTemplates.RecurrenceType)} {loopCounter}";

                // Call the CalculateOffsetDaysForEventSeries function and store the result in offset
                OffsetHelper.CalculateOffsetDaysForEventSeries(evtSeriesTemplates, ref offsetDays, startDate, currentDate, endDate, evtSeriesTemplates.DatesToSkipSerialized, isFirstLoop);
                // Calculate the difference in offset days
                int offsetDifference = offsetDays - oldOffsetDays;

                var evtTemplate = new EventTemplates()
                {
                    SequenceNumber = loopCounter,
                    ElementsAssociated = (int)ElementsAssociatedOptions.NotApplicable,
                    Name = name,
                    OffsetDays = offsetDays,
                    Owner = new EntityReference("systemuser", context.UserId) // AttributeTypeCode.Owner //new AttributeTypeDisplayName() { Value = "OwnerType" }
                };

                // Update oldOffsetDays for the next iteration
                oldOffsetDays = offsetDays;

                evtTemplate.CreateEntity(evtSeriesTemplates.Entity);
                CreateRequest createRequest = new CreateRequest { Target = evtTemplate.Entity };
                requestWithResults.Requests.Add(createRequest);
                evtTemplates.Add(evtTemplate);
                loopCounter++;
                currentDate = currentDate.AddDays(offsetDifference);
                isFirstLoop = false; // Set flag to false after the first loop
            }
            return evtTemplates;
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
