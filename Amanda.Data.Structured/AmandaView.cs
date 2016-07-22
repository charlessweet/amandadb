using System;
using System.Collections.Generic;
using System.Reflection;

namespace Amanda.Data.Structured
{
    public class AmandaView
    {
        private bool _initialized = false;
        private InstanceSet _dataset = null;

        public InstanceSet Dataset
        {
            get { return _dataset; }
            set
            {
                if (value != null)
                    _initialized = true;
                else
                    _initialized = false;
                _dataset = value;
            }
        }

        public bool Ready
        {
            get
            {
                return _initialized;
            }
        }

        public InstanceSet ConvertRecord(object record)
        {
            List<object> recordAsList = new List<object>();
            List<string> attributes = new List<string>();
            PropertyInfo[] properties = record.GetType().GetProperties();
            foreach (var prop in properties)
            {
                recordAsList.Add(prop.GetValue(record));
                attributes.Add(prop.Name);
            }
            return new InstanceSet()
            {
                Data = new object[][] { recordAsList.ToArray() },
                Labels = attributes.ToArray()
            };
        }

        public double ApplyFilter(IViewFilter filter)
        {
            double originalInstances = this.Dataset.InstanceCount;
            double originalAttributes = this.Dataset.AttributeCount;

            this.Dataset = filter.Apply(this.Dataset);

            double reducedInstances = this.Dataset.InstanceCount;
            double reducedAttributes = this.Dataset.AttributeCount;

            return (reducedInstances * reducedAttributes) / (originalInstances * originalAttributes);//reduction ratio
        }

        public void InitializeViewFromData(List<object> data)
        {
            var records = new List<object[]>();

            InstanceSet instances = null;
            foreach(var record in data)
            {
                instances = ConvertRecord(record);
                records.AddRange(instances.Data);//collect the data, but not the attributes
            }
            this.Dataset = new InstanceSet()
            {
                Data = records.ToArray(),
                Labels = instances.Labels
            };
        }
    }
}
