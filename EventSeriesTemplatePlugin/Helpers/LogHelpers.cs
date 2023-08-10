using EventSeriesTemplatePlugin.Data;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSeriesTemplatePlugin
{
    public static class LogHelpers
    {
        /// <summary>
        /// Display the response of an organization message request.
        /// </summary>
        /// <param name="organizationRequest">The organization message request.</param>
        /// <param name="organizationResponse">The organization message response.</param>
        public static void DisplayResponse(OrganizationRequest organizationRequest, OrganizationResponse organizationResponse, ref int successes)
        {
            Console.WriteLine("Created " + ((Entity)organizationRequest.Parameters["Target"]).LogicalName
                + " with codeable concept id as " + organizationResponse.Results["id"].ToString());
            successes++;
        }

        /// <summary>
        /// Display the fault that resulted from processing an organization message request.
        /// </summary>
        /// <param name="organizationRequest">The organization message request.</param>
        /// <param name="count">nth request number from ExecuteMultiple request</param>
        /// <param name="organizationServiceFault">A WCF fault.</param>
        public static void DisplayFault(OrganizationRequest organizationRequest, int count,
            OrganizationServiceFault organizationServiceFault, ref int failures)
        {
            Console.WriteLine("A fault occurred when processing {1} request, at index {0} in the request collection with a fault message: {2}", count + 1,
                organizationRequest.RequestName,
                organizationServiceFault.Message);
            failures++;
        }


        public static void TracePropertyData(ITracingService tracingService, EventSeriesTemplate parentProp)
        {
            foreach (var propertyInfo in parentProp.GetType().GetProperties())
            {
                tracingService.Trace($"Property name: {propertyInfo.Name} Property value: {propertyInfo.GetValue(parentProp)}");
            }
        }
    }
}
