using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amanda.Data.Structured;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace AmandaStructuredStorage
{
    [TestClass]
    public class AmandaViewTests
    {
        private AmandaView InitView()
        {
            AmandaView t = new AmandaView();
            List<string> ca = new List<string>();
            List<TestRecord<Guid>> records = new List<TestRecord<Guid>>();
            for (int i = 0; i < 50000; i++)
            {
                ca.Add(Guid.NewGuid().ToString());
            }
            string[] categoricals = ca.ToArray();
            Random r = new Random();
            TestRecord<Guid> testRecord = null;
            for (int i = 0; i < 1000; i++)
            {
                testRecord = new TestRecord<Guid>()
                {
                    Key = Guid.NewGuid(),
                    PayLoad = categoricals[r.Next(0, categoricals.Length)]
                };
                records.Add(testRecord);
            }
            t.InitializeViewFromData(records.ToList<object>());
            return t;
        }

        [TestMethod]
        public void ConvertRecord_Test()
        {
            AmandaView t = new AmandaView();
            List<string> ca = new List<string>();
            List<TestRecord<Guid>> records = new List<TestRecord<Guid>>();
            for (int i = 0; i < 50000; i++)
            {
                ca.Add(Guid.NewGuid().ToString());
            }
            string[] categoricals = ca.ToArray();
            Random r = new Random();
            TestRecord<Guid> testRecord = null;
            for (int i = 0; i < 1000; i++)
            {
                testRecord = new TestRecord<Guid>()
                {
                    Key = Guid.NewGuid(),
                    PayLoad = categoricals[r.Next(0, categoricals.Length)]
                };
                records.Add(testRecord);
            }

            InstanceSet actual = t.ConvertRecord(testRecord);

            Assert.AreEqual(1, actual.Labels.Count(a => a.Equals("Key")));
            Assert.AreEqual(1, actual.Labels.Count(a => a.Equals("PayLoad")));
        }

        [TestMethod]
        public void InitializeAmandaView_Test()
        {
            AmandaView t = new AmandaView();
            List<string> ca = new List<string>();
            List<TestRecord<Guid>> records = new List<TestRecord<Guid>>();
            for (int i = 0; i < 50000; i++)
            {
                ca.Add(Guid.NewGuid().ToString());
            }
            string[] categoricals = ca.ToArray();
            Random r = new Random();
            TestRecord<Guid> testRecord = null;
            for (int i = 0; i < 1000; i++)
            {
                testRecord = new TestRecord<Guid>()
                {
                    Key = Guid.NewGuid(),
                    PayLoad = categoricals[0]
                };
                records.Add(testRecord);
            }
            t.InitializeViewFromData(records.ToList<object>());
            Assert.AreEqual(records.Count, t.Dataset.InstanceCount);
            Assert.AreEqual(2, t.Dataset.AttributeCount);
        }

        [TestMethod]
        public void ReduceFeatures_Test()
        {
            var t = InitView();
            Stopwatch swatch = new Stopwatch();
            swatch.Start();
            double ratio = t.ApplyFilter(new RandomSelectionFilter());
            swatch.Stop();
            Trace.WriteLine(swatch.ElapsedMilliseconds + "ms", "Reduce to 100");
            Assert.AreEqual(0.1, ratio);
            Assert.AreEqual(100, t.Dataset.InstanceCount);
        }


    }
}
