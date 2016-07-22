using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amanda.Data.Structured
{
    public class InstanceSet
    {
        public long AttributeCount
        {
            get { return Labels.Length; }
        }
        public long InstanceCount
        {
            get { return Data.Length; }
        }
        public object[][] Data { get; set; }
        public string[] Labels { get; set; }
    }
}
