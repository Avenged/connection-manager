using JsonFlatFileDataStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace ConnectionManager.Common.Services
{
    using Common.Models;

    public class EnvironmentService : DataStoreServiceBase
    {
        public EnvironmentService() : base()
        {
        }

        public async Task<bool> UpdateAsync(Environment environment)
        {
            IDocumentCollection<Environment> collection;
            bool result;

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<Environment>();
                result = await collection.UpdateOneAsync(x => x.Guid == environment.Guid, environment);
            }

            return result;
        }

        public async Task<bool> DeleteAllByProjectGuidAsync(Guid guid)
        {
            bool result;
            IDocumentCollection<Environment> collection;

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<Environment>();
                result = await collection.DeleteManyAsync(x => x.ProjectGuid == guid);
            }

            return result;
        }

        public async Task<bool> DeleteByGuidAsync(Guid guid)
        {
            bool result;
            IDocumentCollection<Environment> collection;

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<Environment>();
                result = await collection.DeleteOneAsync(x => x.Guid == guid);
            }

            return result;
        }

        public async Task<IEnumerable<Environment>> GetByProjectGuidAsync(Guid guid)
        {
            IEnumerable<Environment> environments;
            IDocumentCollection<Environment> collection;

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<Environment>();
                environments = collection.Find(x => x.ProjectGuid == guid);
            }

            await Task.CompletedTask;
            return environments;
        }

        public async Task<bool> CreateAsync(Environment environment)
        {
            IDocumentCollection<Environment> collection;
            bool result;

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<Environment>();
                result = await collection.InsertOneAsync(environment);
            }
            return result;
        }

        public async Task<Environment> GetByGuidAsync(Guid guid)
        {
            IDocumentCollection<Environment> collection;
            Environment environment;

            using (DataStore dataStore = GetDataStore())
            {
                collection = dataStore.GetCollection<Environment>();
                environment = collection
                    .Find(x => x.Guid == guid)
                    .ToList()
                    .FirstOrDefault();
            }

            await Task.CompletedTask;
            return environment;
        }
    }
}
