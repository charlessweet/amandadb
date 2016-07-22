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
    internal class TestRecord<KeyType> : IAmandaRecord<KeyType>
    {
        internal TestRecord()
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
