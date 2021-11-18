using JsonFlatFileDataStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectionManager.Common.Models;
using Windows.Storage;

namespace ConnectionManager.Common.Services
{
    public class ProjectService : DataStoreServiceBase
    {
        public ProjectService()
        {
        }

        public async Task<bool> DeleteByGuidAsync(Guid guid)
        {
            bool result;
            IDocumentCollection<Project> collection;

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<Project>();
                result = await collection.DeleteOneAsync(x => x.Guid == guid);
            }

            return result;
        }

        public async Task<bool> CreateAsync(Project project)
        {
            IDocumentCollection<Project> collection;
            bool result;

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<Project>();
                result = await collection.InsertOneAsync(project);
            }
            return result;
        }

        public async Task<bool> UpdateAsync(Project project)
        {
            IDocumentCollection<Project> collection;
            bool result;

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<Project>();
                result = await collection.UpdateOneAsync(x => x.Guid == project.Guid, project);
            }

            return result;
        }

        public async Task<Project> GetByGuidAsync(Guid guid)
        {
            Project project;
            IDocumentCollection<Project> collection;

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<Project>();
                project = collection.AsQueryable().FirstOrDefault(x => x.Guid == guid);
            }

            await Task.CompletedTask;
            return project;
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            IDocumentCollection<Project> collection;
            IEnumerable<Project> projects;

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<Project>();
                projects = collection.AsQueryable().ToList();
            }

            await Task.CompletedTask;
            return projects;
        }
    }
}
