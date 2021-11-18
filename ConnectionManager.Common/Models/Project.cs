using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectionManager.Common.Models
{
    public class Project
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public string ConfigurationPath { get; set; }

        public override string ToString()
        {
            return $"{Name} - {Description}";
        }
    }
}
