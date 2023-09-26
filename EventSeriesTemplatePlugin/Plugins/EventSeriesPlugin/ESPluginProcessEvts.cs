using EventSeriesTemplatePlugin.Data;
using EventSeriesTemplatePlugin.Helpers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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
                    if (evtSeriesEntity.LogicalName != Constants.EvtSeries_Table)
                    {
                        tracingService.Trace($"Stage {++stageNumber}: Invalid target entity. Entity used: {evtSeriesEntity.LogicalName}, Entity expedcted: {Constants.EvtSeries_Table}");
                        return; // Targeting incorrect evt series template entity
                    }

                    // Create events
                    tracingService.Trace($"Stage {++stageNumber}: Initialising Event Series object");
                    var evtSeries = new EventSeries(context, ImageTypes.PreImage);
                    tracingService.Trace($"Stage {++stageNumber}: Updating Event Series From Execution Context");
                    evtSeries.UpdateEntityFromExecutionContext(evtSeriesEntity);
                    CreateEvents(context, tracingService, service, evtSeries);
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
                tracingService.Trace($"Stage {++stageNumber}: Conditions to create templates {nameof(evtSeries.CreateEvents)} : {evtSeries.CreateEvents}");
                LogHelpers.TracePropertyData(tracingService, evtSeries);

                if (evtSeries.CreateEvents == YesOrNo.Yes && !evtSeries.EvtsCreated)
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

                    // retrieve event templates
                    var evtSeriesTemplateEntity = service.Retrieve(Constants.EvtSeriesTemplate_Table, evtSeries.PrimarySeriesTemplate.Id, new ColumnSet(true));
                    var eventSeriesTemplate = new EventSeriesTemplate(evtSeriesTemplateEntity);
                    evtSeries.EventSeriesTemplate = eventSeriesTemplate;

                    List<EventTemplates> evtTemplates = RetrieveEventTemplatesFromDataverse(service, evtSeries);

                    // Create events from relevant event templates retrieved
                    var evts = CreateEventsFromTemplate(context, tracingService, evtTemplates, evtSeries, requestWithResults);
                    evtSeries.Events.AddRange(evts);

                    // Execute all the requests in the request collection using a single web method call.
                    tracingService.Trace($"Stage {++stageNumber}: Executing all the requests in the request collection.");
                    ExecuteMultipleResponse responseWithResults = (ExecuteMultipleResponse)service.Execute(requestWithResults);
                    tracingService.Trace($"Stage {++stageNumber}: Finished Executing all the requests in the request collection.");

                    // Handle responses
                    HandleResponses(requestWithResults, responseWithResults, tracingService);

                    // now retrieve the updated version of the event series
                    var evtSeriesUpdatedEntity = service.Retrieve(Constants.EvtSeries_Table, evtSeries.Entity.Id, new ColumnSet(true));
                    var evtSeriesUpdated = new EventSeries(evtSeriesUpdatedEntity);

                    // make updates to event series
                    evtSeriesUpdated.EvtsCreated = responseWithResults.Responses.Any(r => r.Response != null);
                    evtSeriesUpdated.Update(); // update the referenced entity
                    service.Update(evtSeriesUpdated.Entity); // then use org service to update in dataverse
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, 500, ex.Message);
            }
        }



        private IEnumerable<Event> CreateEventsFromTemplate(IPluginExecutionContext context, ITracingService tracingService, IEnumerable<EventTemplates> eventTemplates, EventSeries eventSeries, ExecuteMultipleRequest requestWithResults)
        {
            tracingService.Trace($"Stage {++stageNumber}: Intializing Create {nameof(CreateEventsFromTemplate)} method request.");

            var events = new List<Event>();
            var firstLoop = true;
            var evtDate = new DateTime();

            foreach (var evtTemplate in eventTemplates)
            {
                if (firstLoop)
                {
                    evtDate = evtTemplate.EventSeriesTemplate.StartDate.AddDays(evtTemplate.OffsetDays);
                }
                var evt = new Event(evtTemplate, eventSeries)
                {
                    EventDate = evtDate,
                    Owner = new EntityReference("systemuser", context.UserId) // AttributeTypeCode.Owner //new AttributeTypeDisplayName() { Value = "OwnerType" }
                };

                evt.CreateEntity(eventSeries.Entity);
                events.Add(evt);
                CreateRequest createRequest = new CreateRequest { Target = evt.Entity };
                requestWithResults.Requests.Add(createRequest);
            }

            return events;
        }

        private List<EventTemplates> RetrieveEventTemplatesFromDataverse(IOrganizationService service, EventSeries evtSeries)
        {
            ConditionExpression conditionExpression = new ConditionExpression();
            conditionExpression.AttributeName = Constants.EvtTemplate_EvtSeriesTemplateKey;
            conditionExpression.Operator = ConditionOperator.Equal;
            conditionExpression.Values.Add(evtSeries.PrimarySeriesTemplate.Id);

            // FilterExpression - contains ConditionaExpression
            FilterExpression filterExpression = new FilterExpression();
            filterExpression.Conditions.Add(conditionExpression);

            // QueryExpression - ColumnSet, Table
            QueryExpression queryExpression = new QueryExpression();
            queryExpression.ColumnSet.AllColumns = true;
            queryExpression.EntityName = Constants.EvtTemplate_Table;
            queryExpression.Criteria.AddFilter(filterExpression);

            EntityCollection retrievedEvtTemplateResults = service.RetrieveMultiple(queryExpression);

            var evtTemplates = new List<EventTemplates>();
            foreach (Entity entity in retrievedEvtTemplateResults.Entities)
            {
                var evtTemplate = new EventTemplates(entity, evtSeries);
                evtTemplates.Add(evtTemplate);
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
