using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManagerFullTrust.Extensions
{
    public static class ServiceControllerExtensions
    {
        public static Process GetProcess(this ServiceController service)
        {
            var qry = $"SELECT PROCESSID FROM WIN32_SERVICE WHERE NAME = '{service.ServiceName}'";
            var managementObjects = new ManagementObjectSearcher(qry).Get();

            if (managementObjects.Count != 1)
            {
                throw new InvalidOperationException($"In attempt to kill a service '{service.ServiceName}', expected to find one process for service but found {managementObjects.Count}.");
            }

            int processId = 0;

            foreach (var mngntObj in managementObjects)
            {
                processId = (int)(uint)mngntObj["PROCESSID"];
            }

            if (processId == 0)
            {
                throw new InvalidOperationException($"In attempt to kill a service '{service.ServiceName}', process ID for service is 0. Possible reason is the service is already stopped.");
            }

            var process = Process.GetProcessById(processId);

            return process;
        }
    }
}
