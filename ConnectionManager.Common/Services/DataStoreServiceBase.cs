using JsonFlatFileDataStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Windows.Storage;

namespace ConnectionManager.Common.Services
{
    public abstract class DataStoreServiceBase
    {
        protected DataStore GetDataStore()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var path = Path.Combine(localFolder.Path, "data.json");
                return new DataStore(path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
