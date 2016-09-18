using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Amanda.Data
{
    [DataContract]
    [Serializable]
    public class RowLocation
    {
        [DataMember]
        public int Offset { get; set; }
        [DataMember]
        public string CurrentPageFile { get; set; }
    }
}
