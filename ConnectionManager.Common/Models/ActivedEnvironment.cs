using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectionManager.Common.Models
{
    public class ActivedEnvironment
    {
        public Guid ProjectGuid { get; set; }
        public Guid? EnvironmentGuid { get; set; }
    }
}
