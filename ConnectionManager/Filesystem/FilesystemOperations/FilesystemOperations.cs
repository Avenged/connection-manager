using ConnectionManager.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using ConnectionManager.Common;
using Newtonsoft.Json;

namespace ConnectionManager.Filesystem
{
    public class FilesystemOperations
    {
        public async Task<(FilesystemResult, GenericOperationResult)> ReplaceConnectionStringAsync(string filePath, string connectionStrings)
        {
            return await PerformAdminOperation(new ValueSet()
            {
                { "Arguments", "FileOperation" },
                { "fileop", "ReplaceConnectionString" },
                { "filepath", filePath },
                { "connectionstring", connectionStrings }
            });
        }

        public async Task<(FilesystemResult, GenericOperationResult)> RestartServiceAsync(string serviceName, int timeout = 90000)
        {
            return await PerformAdminOperation(new ValueSet()
            {
                { "Arguments", "FileOperation" },
                { "fileop", "RestartService" },
                { "servicename", serviceName },
                { "timeout", timeout.ToString() }
            });
        }

        public async Task<(FilesystemResult, GenericOperationResult)> PerformAdminOperation(ValueSet operation)
        {
            var connection = await AppServiceConnectionHelper.Instance;
            if (connection != null && await connection.Elevate())
            {
                // Try again with fulltrust process (admin)
                connection = await AppServiceConnectionHelper.Instance;
                if (connection != null)
                {
                    operation.Add("operationID", Guid.NewGuid().ToString());
                    operation.Add("HWND", NativeWinApiHelper.CoreWindowHandle.ToInt64());
                    var (status, response) = await connection.SendMessageForResponseAsync(operation);
                    var fsResult = (FilesystemResult)(status == AppServiceResponseStatus.Success
                        && response.Get("Success", false));
                    var genericOperationResult = JsonConvert.DeserializeObject<GenericOperationResult>(response.Get("Result", "{}"));
                    fsResult &= (FilesystemResult)(genericOperationResult?.Succeeded);
                    return (fsResult, genericOperationResult);
                }
            }

            return ((FilesystemResult)false, null);
        }
    }
}
