using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ConnectionManager.Common;
using Newtonsoft.Json;
using Windows.Foundation.Collections;
using System.ServiceProcess;
using ConnectionManagerFullTrust.Extensions;

namespace ConnectionManagerFullTrust.MessageHandlers
{
    public class FileOperationsHandler : IMessageHandler
    {
        private ProgressHandler progressHandler;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Initialize(PipeStream connection)
        {
            progressHandler = new ProgressHandler(connection);
        }

        public async Task ParseArgumentsAsync(PipeStream connection, Dictionary<string, object> message, string arguments)
        {
            switch (arguments)
            {
                case "FileOperation":
                    await ParseFileOperationAsync(connection, message);
                    break;
            }
        }

        public void WaitForCompletion()
        {
            progressHandler.WaitForCompletion();
        }

        private async Task ParseFileOperationAsync(PipeStream connection, Dictionary<string, object> message)
        {
            switch (message.Get("fileop", ""))
            {
                case "RestartService":
                    await HandleRestartServiceAsync(connection, message);
                    break;
                case "ReplaceConnectionString":
                    await HandleReplaceConnectionStringAsync(connection, message);
                    break;
            }
        }

        private async Task HandleRestartServiceAsync(PipeStream connection, Dictionary<string, object> message)
        {
            GenericOperationResult result;

            try
            {              
                var serviceName = (string)message["servicename"];
                var timeout = TimeSpan.FromMilliseconds(int.Parse((string)message["timeout"]));

                var millisec1 = Environment.TickCount;

                var service = new ServiceController(serviceName);

                if (service.Status == ServiceControllerStatus.Running)
                {
                    var process = service.GetProcess();
                    process.Kill();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }

                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(60000 - (millisec2 - millisec1));

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);

                result = new GenericOperationResult(true, null);
                await Win32API.SendMessageAsync(connection, new ValueSet() {
                    { "Success", true },
                    { "Result", JsonConvert.SerializeObject(result) }
                }, message.Get("RequestID", (string)null));
            }
            catch (Exception ex)
            {
                Program.Logger.Error(ex.Message);
                result = new GenericOperationResult(false, ex.Message);
                await Win32API.SendMessageAsync(connection, new ValueSet() {
                    { "Success", false },
                    { "Result", JsonConvert.SerializeObject(result) }
                }, message.Get("RequestID", (string)null));
            }
        }

        private async Task HandleReplaceConnectionStringAsync(PipeStream connection, Dictionary<string, object> message)
        {
            static XmlNode SearchConnectionStringXmlNode(XmlNode rootNode)
            {
                foreach (XmlNode xmlNode in rootNode)
                {
                    if (xmlNode.Name.ToLower() == "connectionstrings")
                    {
                        return xmlNode;
                    }
                    else if (xmlNode.HasChildNodes)
                    {
                        var node = SearchConnectionStringXmlNode(xmlNode);
                        if (node != null)
                        {
                            return node;
                        }
                    }
                }
                return null;
            }

            try
            {
                GenericOperationResult result = null;
                var filePath = (string)message["filepath"];
                var connectionString = (string)message["connectionstring"];

                XmlDocument xmlDocument = new XmlDocument();
                XmlNode cnnStrNode = null;

                xmlDocument.Load(filePath);

                cnnStrNode = SearchConnectionStringXmlNode(xmlDocument);

                if (cnnStrNode is null)
                {
                    var errorMsg = $"The ConnectionStrings node was not found in the document: {filePath}";
                    result = new GenericOperationResult(false, errorMsg);
                    await Win32API.SendMessageAsync(connection, new ValueSet() {
                        { "Success", false },
                        { "Result", JsonConvert.SerializeObject(result) }
                    }, message.Get("RequestID", (string)null));
                    return;
                }

                cnnStrNode.InnerXml = connectionString;
                xmlDocument.Save(filePath);

                result = new GenericOperationResult(true, null);

                await Win32API.SendMessageAsync(connection, new ValueSet() {
                    { "Success", true },
                    { "Result", JsonConvert.SerializeObject(result) }
                }, message.Get("RequestID", (string)null));
            }
            catch (Exception ex)
            {
                var result = new GenericOperationResult(false, ex.Message);
                await Win32API.SendMessageAsync(connection, new ValueSet() {
                    { "Success", false },
                    { "Result", JsonConvert.SerializeObject(result) }
                }, message.Get("RequestID", (string)null));
            }
        }
    }
}
