﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using EventSeriesTemplatePlugin.Data;
using EventSeriesTemplatePlugin.Helpers;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using static EventSeriesTemplatePlugin.Enums;

namespace EventSeriesTemplatePlugin
{
    public class ESTPluginProcessETs : IPlugin
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
                    Entity evtSeriesTemplateEntity = (Entity)context.InputParameters["Target"];
                    if (evtSeriesTemplateEntity.LogicalName != Constants.EvtSeriesTemplate_Table) 
                    {
                        return; // Targeting incorrect evt series template entity
                    }

                    // Create event templates
                    var evtSeriesTemplates = new EventSeriesTemplate(context, ImageTypes.PreImage);
                    evtSeriesTemplates.UpdateEntityFromExecutionContext(evtSeriesTemplateEntity);
                    if (EventTemplateSeriesBusinessRules.ShouldCreateTemplates(evtSeriesTemplates))
                    {
                        CreateEventTemplates(context, tracingService, service, evtSeriesTemplates);
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("MyPlugin: error: {0}", ex.ToString());
                throw;
            }
        }

        private void CreateEventTemplates(IPluginExecutionContext context, ITracingService tracingService, IOrganizationService service, EventSeriesTemplate evtSeriesTemplates)
        {
            try
            {
                // Validate we are targeting the correct evt series template entity
                tracingService.Trace($"Stage {++stageNumber}: Validating we are targeting the correct evt series template entity");
                tracingService.Trace($"Stage {++stageNumber}: Conditions to create templates {nameof(evtSeriesTemplates.CreateEvtTemplates)} : {evtSeriesTemplates.CreateEvtTemplates} " +
                    $"{nameof(evtSeriesTemplates.EventTemplatesCreated)} : {evtSeriesTemplates.EventTemplatesCreated}");
                LogHelpers.TracePropertyData(tracingService, evtSeriesTemplates);

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

                    // Create event templates
                    var evtTemplates = CreateEventTemplatesInRange(context, tracingService, evtSeriesTemplates, requestWithResults);

                    evtSeriesTemplates.EventTemplates = new List<EventTemplates>();
                    evtSeriesTemplates.EventTemplates.AddRange(evtTemplates);
                    // Execute all the requests in the request collection using a single web method call.
                    tracingService.Trace($"Stage {++stageNumber}: Executing all the requests in the request collection.");
                    ExecuteMultipleResponse responseWithResults = (ExecuteMultipleResponse)service.Execute(requestWithResults);
                    tracingService.Trace($"Stage {++stageNumber}: Finished Executing all the requests in the request collection.");

                    // Handle responses
                    HandleResponses(requestWithResults, responseWithResults, tracingService);

                    tracingService.Trace("Setting ET OK data in EST");
                    evtSeriesTemplates.ETWithOkElements = responseWithResults.Responses.Count(r => r.Response != null);
                    evtSeriesTemplates.AllETOk = !responseWithResults.Responses.Any(r => r.Fault != null);
                    evtSeriesTemplates.EventTemplatesCreated = responseWithResults.Responses.Any(r => r.Response != null);

                    evtSeriesTemplates.Update(evtSeriesTemplates.Entity); // update the referenced entity
                    service.Update(evtSeriesTemplates.Entity); // then use org service to update in dataverse
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, 500, ex.Message);
            }
        }

        private IEnumerable<EventTemplates> CreateEventTemplatesInRange(IPluginExecutionContext context, ITracingService tracingService, EventSeriesTemplate evtSeriesTemplates, ExecuteMultipleRequest requestWithResults)
        {
            tracingService.Trace($"Stage {++stageNumber}: Intializing Create {nameof(CreateEventTemplatesInRange)} method request.");
            DateTime startDate = evtSeriesTemplates.StartDate;
            DateTime currentDate = evtSeriesTemplates.StartDate;
            DateTime endDate = startDate.AddMonths(evtSeriesTemplates.NumberOfMonths);
            int offsetDays = 0; // Initialize the variable before the loop
            int oldOffsetDays = 0; // Initialize the variable to store the previous offset
            int loopCounter = 1; // Initialize the loop counter
            bool isFirstLoop = true; // Added flag to identify the first loop
            tracingService.Trace($"Stage {++stageNumber}: Initialized Create {nameof(CreateEventTemplatesInRange)} method request." +
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
