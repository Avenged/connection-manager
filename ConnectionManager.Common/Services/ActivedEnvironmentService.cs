using JsonFlatFileDataStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ConnectionManager.Common.Models;

namespace ConnectionManager.Common.Services
{
    public class ActivedEnvironmentService : DataStoreServiceBase
    {
        public ActivedEnvironmentService()
        {
        }

        public async Task<bool> DeleteByProjectGuidAsync(Guid guid)
        {
            bool result;
            IDocumentCollection<ActivedEnvironment> collection;

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<ActivedEnvironment>();
                result = await collection.DeleteOneAsync(x => x.ProjectGuid == guid);
            }

            return result;
        }

        public async Task<bool> Update(ActivedEnvironment activedEnvironment)
        {
            IDocumentCollection<ActivedEnvironment> collection;
            bool result;

            //await UpdateStaticResource(activedEnvironment);

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<ActivedEnvironment>();
                bool exists = collection.Find(x => x.ProjectGuid == activedEnvironment.ProjectGuid) != null;
                
                if (!exists)
                {
                    return await collection.InsertOneAsync(activedEnvironment);
                }

                result = await collection
                    .UpdateOneAsync(x => x.ProjectGuid == activedEnvironment.ProjectGuid, activedEnvironment);
            }

            await Task.CompletedTask;
            return result;
        }

        private async Task UpdateStaticResource(ActivedEnvironment activedEnvironment)
        {
            var projectService = new ProjectService();
            var environmentService = new EnvironmentService();
            var project = await projectService.GetByGuidAsync(activedEnvironment.ProjectGuid);
            var environment = await environmentService.GetByGuidAsync((Guid)activedEnvironment.EnvironmentGuid);

            XmlDocument xmlDocument;

            try
            {
                xmlDocument = new XmlDocument();
                xmlDocument.Load(project.ConfigurationPath);

                XmlNode connStrsNode = null;

                foreach (XmlNode node in xmlDocument)
                {
                    if (node.Name == "connectionStrings")
                    {
                        connStrsNode = node;
                        break;
                    }
                }

                connStrsNode.RemoveAll();

                XmlNode childNode = xmlDocument.CreateTextNode(environment.ConnectionStrings);

                connStrsNode.AppendChild(childNode);
            }
            catch (XmlException ex)
            {
                throw new XmlException("The selected file is not a valid xml file.");
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException("File not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                throw ex;
            }
        }

        public async Task<ActivedEnvironment> GetByProjectGuidAsync(Guid projectGuid)
        {
            ActivedEnvironment activedEnvironment;
            IDocumentCollection<ActivedEnvironment> collection;

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<ActivedEnvironment>();
                activedEnvironment = collection
                    .Find(x => x.ProjectGuid == projectGuid)
                    .FirstOrDefault();

                if (activedEnvironment == null)
                {
                    activedEnvironment = new ActivedEnvironment()
                    {
                        ProjectGuid = projectGuid,
                        EnvironmentGuid = null,
                    };

                    await collection.InsertOneAsync(activedEnvironment);
                }
            }

            await Task.CompletedTask;
            return activedEnvironment;
        }

        
    }
}
