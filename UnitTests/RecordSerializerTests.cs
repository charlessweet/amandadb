using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amanda.IO;

namespace AmandaStructuredStorage
{
    [TestClass]
    public class RecordSerializerTests
    {
        private class TestClass
        {
            private int PropertInt { get; set; }
            public int PropertyReadOnly { get; private set; }
            public int PropertyWriteOnly { private get; set; }
        }

        private class TestAllPropertyTypes
        {
            public bool BoolType { get; set; }
            public Int32 IntType { get; set; }
            public UInt32 UIntType { get; set; }
            public Int64 Int64Type { get; set; }
            public UInt64 UInt64Type { get; set; }
            public Guid GuidType { get; set; }
            public string StringType { get; set; }
        }
        [TestMethod]
        public void GetMaxSize()
        {
            uint fullSize = RecordSerializer.BOOL_MAX_SIZE + 2 * RecordSerializer.WORD_MAX_SIZE +
                2 * RecordSerializer.DWORD_MAX_SIZE + RecordSerializer.GUID_MAX_SIZE + RecordSerializer.STRING_MAX_SIZE;
            RecordSerializer t = new RecordSerializer();
            Assert.AreEqual(fullSize, t.CalculateRecordMaxSize(typeof(TestAllPropertyTypes)));

            Assert.AreEqual((uint)0, t.CalculateRecordMaxSize(typeof(TestClass)));
        }


        [TestMethod]
        public void SerializeTestClass()
        {
            uint fullSize = RecordSerializer.BOOL_MAX_SIZE + 2 * RecordSerializer.WORD_MAX_SIZE +
                2 * RecordSerializer.DWORD_MAX_SIZE + RecordSerializer.GUID_MAX_SIZE + RecordSerializer.STRING_MAX_SIZE;
            RecordSerializer t = new RecordSerializer();
            byte[] serialized = t.SerializeRecord(typeof(TestAllPropertyTypes), new TestAllPropertyTypes());
            Assert.IsNotNull(serialized);
            Assert.AreEqual((int)fullSize, serialized.Length);

            TestAllPropertyTypes p2 = new TestAllPropertyTypes();
            p2.BoolType = false;
            p2.GuidType = Guid.NewGuid();
            p2.Int64Type = Int64.MaxValue;
            p2.UInt64Type = UInt64.MaxValue;
            p2.IntType = Int32.MaxValue;
            p2.UIntType = UInt32.MaxValue;
            p2.StringType = AmandaEngineTests.LOREM_8K;
            serialized = t.SerializeRecord(typeof(TestAllPropertyTypes), p2);
            Assert.IsNotNull(serialized);
            Assert.AreEqual((int)fullSize, serialized.Length);

            TestAllPropertyTypes p3 = (TestAllPropertyTypes)t.DeserializeRecord(typeof(TestAllPropertyTypes), serialized, 0);
            Assert.IsNotNull(p3);
            Assert.AreEqual(p2.BoolType, p3.BoolType);
            Assert.AreEqual(p2.Int64Type, p3.Int64Type);
            Assert.AreEqual(p2.IntType, p3.IntType);
            Assert.AreEqual(p2.UIntType, p3.UIntType);
            Assert.AreEqual(p2.UInt64Type, p3.UInt64Type);
            Assert.AreEqual(p2.StringType.Substring(0, (int)RecordSerializer.STRING_MAX_SIZE), p3.StringType);
            Assert.AreEqual(p2.GuidType, p3.GuidType);
        }
    }
}
