using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSeriesTemplatePlugin.Services.BPF
{
    public class BPFService
    {
        public Entity activeProcessInstance = null;
        public Guid _processId;
        public Guid _activeStageId;
        public string _activeStageName;
        public string _procInstanceLogicalName;
        public int _activeStagePosition;

        public bool IsNewStageOnBPF(ITracingService tracingService, IOrganizationService service, Entity entity, string stageName)
        {
            var activetageName = GetActiveStageName(tracingService, service, entity);

            return activetageName != null ? stageName.Equals(activetageName, StringComparison.OrdinalIgnoreCase) : false;
        }

        private string GetActiveStageName(ITracingService tracingService, IOrganizationService service, Entity entity)
        {
            RetrieveProcessInstancesRequest retrieveProcessInstancesReq = new RetrieveProcessInstancesRequest
            {
                EntityId = entity.Id,
                EntityLogicalName = entity.LogicalName
            };

            RetrieveProcessInstancesResponse retrieveProcessInstancesResp = (RetrieveProcessInstancesResponse)service.Execute(retrieveProcessInstancesReq);

            if (retrieveProcessInstancesResp.Processes.Entities.Count > 0)
            {
                activeProcessInstance = retrieveProcessInstancesResp.Processes.Entities[0]; // First record is the active process instance
                _processId = activeProcessInstance.Id; // Id of the active process instance, which will be used
                                                       // later to retrieve the active path of the process instance

                tracingService.Trace("Current active process instance for the record: '{0}'", activeProcessInstance["name"].ToString());
                _procInstanceLogicalName = activeProcessInstance["name"].ToString().Replace(" ", string.Empty).ToLower();
            }
            else
            {
                tracingService.Trace("No process instances found for the record; aborting the sample.");
                return null;
            }

            // Retrieve the active stage ID of the active process instance
            _activeStageId = new Guid(activeProcessInstance.Attributes["processstageid"].ToString());

            // Retrieve the process stages in the active path of the current process instance
            RetrieveActivePathRequest pathReq = new RetrieveActivePathRequest
            {
                ProcessInstanceId = _processId
            };
            RetrieveActivePathResponse pathResp = (RetrieveActivePathResponse)service.Execute(pathReq);
            tracingService.Trace("\nRetrieved stages in the active path of the process instance:");
            for (int i = 0; i < pathResp.ProcessStages.Entities.Count; i++)
            {
                tracingService.Trace("\tBPF Stage {0}: {1} (BPF StageId: {2})", i + 1,
                    pathResp.ProcessStages.Entities[i].Attributes["stagename"], pathResp.ProcessStages.Entities[i].Attributes["processstageid"]);


                // Retrieve the active stage name and active stage position based on the activeStageId for the process instance
                if (pathResp.ProcessStages.Entities[i].Attributes["processstageid"].ToString() == _activeStageId.ToString())
                {
                    _activeStageName = pathResp.ProcessStages.Entities[i].Attributes["stagename"].ToString();
                    _activeStagePosition = i;
                }
            }

            // Display the active stage name and Id
            tracingService.Trace("\nActive stage for the process instance: '{0}' (StageID: {1})", _activeStageName, _activeStageId);

            return _activeStageName;
        }
    }
}
