using System;
using Windows.UI.Xaml;

namespace ConnectionManager.Common.Models
{
    public class Environment
    {
        public Guid Guid { get; set; }
        public Guid ProjectGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConnectionStrings { get; set; }
        //public string Path { get; set; }
    }
}
