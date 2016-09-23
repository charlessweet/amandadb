using Amanda.Data.Structured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AmandaStructuredStorage
{
    [DataContract]
    [Serializable]
    public class TestRecord<KeyType> : IAmandaRecord<KeyType>
    {
        public TestRecord()
        {
            Key = default(KeyType);
        }
        [DataMember]
        public KeyType Key
        {
            get;
            set;
        }
        [DataMember]
        public string PayLoad
        {
            get;
            set;
        }
    }
}
