using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amanda.Data.Structured;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Amanda.IO.MemoryOnly;
using Amanda.IO;
using System.Threading.Tasks;

namespace AmandaStructuredStorage
{
    [TestClass]
    public class AmandaEngineTests
    {
        [TestMethod]
        [TestCategory("Amanda Structured Storage")]
        public void AmandaEngine_IOExceptionWhenParentFolderDoesNotExist()
        {
            AmandaEngine<TestRecord<Guid>, Guid> sqda = new AmandaEngine<TestRecord<Guid>, Guid>();

            var amandaParentFolder = new Amanda.IO.AmandaDirectory(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().FullName) + @"\" + Guid.NewGuid().ToString());
            try
            {
                //change to a new parent folder we know is invalid
                sqda.CreateOrUseAmanda(amandaParentFolder);
            }
            catch (System.IO.IOException ex)
            {
                Assert.AreEqual("The specified engine parent directory is null or does not exist.", ex.Message);
            }
        }
        [TestMethod]
        [TestCategory("Amanda Structured Storage")]
        public void AmandaEngine_IOExceptionWhenAmandaFolderCreationFails()
        {
            AmandaEngine<TestRecord<Guid>, Guid> t = new AmandaEngine<TestRecord<Guid>, Guid>();
            
            var amandaParentFolder = new Amanda.IO.AmandaDirectory(@"c:\Windows\System32\FAKE");
            try
            {
                //try to create folder on invalid drive
                t.CreateOrUseAmanda(amandaParentFolder);
            }
            catch (System.IO.IOException ex)
            {
                Assert.AreEqual("The specified engine parent directory is null or does not exist.", ex.Message);
            }
        }
        [TestMethod]
        [TestCategory("Amanda Structured Storage")]
        public void AmandaEngine_CreateOrUseAmandaSucceeds()
        {
            AmandaEngine<TestRecord<Guid>, Guid> t = new AmandaEngine<TestRecord<Guid>, Guid>();
            string parentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var amandaParentFolder = new MemoryOnlyDirectory(parentFolder);
            var amandaFolder = t.CreateOrUseAmanda(amandaParentFolder);

            //now verify folderpaths are appropriate
            Assert.AreEqual(Path.Combine(amandaParentFolder.Path, ".amanda"), amandaFolder.Path);
            Assert.IsTrue(t.IsInitialized);
        }
        [TestMethod]
        [TestCategory("Amanda Structured Storage")]
        public void AmandaEngine_RetainsDataAcrossInitializations()
        {
            Guid root = Guid.NewGuid();
            string parentFolder = Environment.CurrentDirectory + "\\testing";
            System.IO.Directory.CreateDirectory(parentFolder);
            Trace.WriteLine(parentFolder, "Root Folder");
            AmandaEngine<TestRecord<Guid>, Guid> t = new AmandaEngine<TestRecord<Guid>, Guid>();
            var amandaParentFolder = new AmandaDirectory(parentFolder);
            var amandaFolder = t.CreateOrUseAmanda(amandaParentFolder);
            List<TestRecord<Guid>> toSave= new List<TestRecord<Guid>>();
            for(int i = 0; i < 100; i++)
            {
                toSave.Add(new TestRecord<Guid>()
                {
                    Key = Guid.NewGuid(),
                    PayLoad = Guid.NewGuid().ToString()
                });
            }
            Stopwatch s = new Stopwatch();
            s.Start();
            t.SaveEntries(toSave);
            s.Stop();
            Trace.WriteLine((double)s.ElapsedMilliseconds/(double)100 + "ms", "Write Time With Disk Per Record");

            t = new AmandaEngine<TestRecord<Guid>, Guid>();
            amandaParentFolder = new AmandaDirectory(parentFolder);
            amandaFolder = t.CreateOrUseAmanda(amandaParentFolder);

            //now add another one
            List<TestRecord<Guid>> toSave2 = new List<TestRecord<Guid>>();
            for (int i = 0; i < 100; i++)
            {
                toSave2.Add(new TestRecord<Guid>()
                {
                    Key = Guid.NewGuid(),
                    PayLoad = Guid.NewGuid().ToString()
                });
            }
            t.SaveEntries(toSave2);
            foreach (var rec in toSave2)
            {
                Assert.AreEqual(1, t.GetMatchingEntries(rec.Key).Count);
            }
            foreach (var rec in toSave)
            {
                Assert.AreEqual(1, t.GetMatchingEntries(rec.Key).Count);
            }
        }
        [TestMethod]
        [TestCategory("Amanda Structured Storage")]
        public void AmandaEngine_FrequentWritesIntroduceRaceConditions()
        {
            Guid root = Guid.NewGuid();
            string parentFolder = Environment.CurrentDirectory + "\\testing-2";
            System.IO.Directory.CreateDirectory(parentFolder);
            Trace.WriteLine(parentFolder, "Root Folder");
            List<Task> tasks = new List<Task>();
            var amandaParentFolder = new AmandaDirectory(parentFolder);
            Stopwatch sw = new Stopwatch();
            double numInstances = 38;
            double numRecords = 1;
            for (int i = 0; i < numInstances; i++)
            {
                var engine = new AmandaEngine<TestRecord<Guid>, Guid>();
                engine.CreateOrUseAmanda(amandaParentFolder);
                List<TestRecord<Guid>> toSave = new List<TestRecord<Guid>>();
                for(int j = 0; j < numRecords; j++)
                {
                    toSave.Add(new TestRecord<Guid>()
                    {
                        Key = Guid.NewGuid(),
                        PayLoad = Guid.NewGuid().ToString()
                    });
                }
                sw.Start();
                Task task = new Task(() => engine.SaveEntries(toSave));
                task.Start();
                sw.Stop();
                toSave.Clear();
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            sw.Stop();
            Trace.WriteLine(((double)sw.ElapsedMilliseconds / (numInstances * numRecords)) + "ms", "Per Write To Disk");
        }
        [TestMethod]
        [TestCategory("Amanda Structured Storage")]
        public void AmandaEngine_StoreAndRetrieveSingleRecord()
        {
            AmandaEngine<TestRecord<Guid>, Guid> t = new AmandaEngine<TestRecord<Guid>, Guid>();
            string parentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var amandaParentFolder = new MemoryOnlyDirectory(parentFolder);
            var amandaFolder = t.CreateOrUseAmanda(amandaParentFolder);

            //save entries
            var testRecord = new TestRecord<Guid>()
            {
                Key = Guid.NewGuid(),
                PayLoad = Guid.NewGuid().ToString()
            };
            List<TestRecord<Guid>> singleRecordList = new List<TestRecord<Guid>>();
            singleRecordList.Add(testRecord);
            Stopwatch s = new Stopwatch();
            s.Start();
            var locations = t.SaveEntries(singleRecordList);
            s.Stop();
            Trace.WriteLine(s.ElapsedMilliseconds+ "ms", "Time To Write Record");
            Assert.AreEqual(1, locations.Count, "Expected the number of entries for which we have locations to match the number of entries saved.");

            s.Reset();
            s.Start();
            TestRecord<Guid> actual = t.GetMatchingEntries(testRecord.Key).FirstOrDefault();
            s.Stop();
            Trace.WriteLine(s.ElapsedMilliseconds + "ms", "Time To Retrieve Record");
            Assert.AreEqual(testRecord.Key, actual.Key);
            Assert.AreEqual(testRecord.PayLoad, actual.PayLoad);
        }
        [TestMethod]
        [TestCategory("Amanda Structured Storage")]
        public void AmandaEngine_StoreAndRetrieveSingleRecordOnMultipleThreads()
        {
            AmandaEngine<TestRecord<Guid>, Guid> t = new AmandaEngine<TestRecord<Guid>, Guid>();
            string parentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var amandaParentFolder = new MemoryOnlyDirectory(parentFolder);
            var amandaFolder = t.CreateOrUseAmanda(amandaParentFolder);

            //save entries
            var testRecord = new TestRecord<Guid>()
            {
                Key = Guid.NewGuid(),
                PayLoad = Guid.NewGuid().ToString()
            };
            List<TestRecord<Guid>> singleRecordList = new List<TestRecord<Guid>>();
            singleRecordList.Add(testRecord);
            Stopwatch s = new Stopwatch();
            s.Start();
            var locations = t.SaveEntries(singleRecordList);
            s.Stop();
            Trace.WriteLine(s.ElapsedMilliseconds + "ms", "Time To Write Record");
            Assert.AreEqual(1, locations.Count, "Expected the number of entries for which we have locations to match the number of entries saved.");

            s.Reset();
            s.Start();
            TestRecord<Guid> actual = t.GetMatchingEntries(testRecord.Key).FirstOrDefault();
            s.Stop();
            Trace.WriteLine(s.ElapsedMilliseconds + "ms", "Time To Retrieve Record");
            Assert.AreEqual(testRecord.Key, actual.Key);
            Assert.AreEqual(testRecord.PayLoad, actual.PayLoad);
        }
        [TestMethod]
        [TestCategory("Amanda Structured Storage")]
        public void AmandaEngine_StoreAndRetrieve1KRecords()
        {
            AmandaEngine<TestRecord<Guid>, Guid> t = new AmandaEngine<TestRecord<Guid>, Guid>();
            string parentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var amandaParentFolder = new MemoryOnlyDirectory(parentFolder);
            var amandaFolder = t.CreateOrUseAmanda(amandaParentFolder);

            //save entries
            List<TestRecord<Guid>> singleRecordList = new List<TestRecord<Guid>>();
            for(int i = 0; i < 1000; i++)
            {
                singleRecordList.Add(new TestRecord<Guid>()
                {
                    Key = Guid.NewGuid(),
                    PayLoad = Guid.NewGuid().ToString()
                });
            }
            Stopwatch s = new Stopwatch();
            s.Start();
            var locations = t.SaveEntries(singleRecordList);
            s.Stop();
            double elapsed = s.ElapsedMilliseconds;
            Trace.WriteLine(elapsed/1000f + "ms", "Average Time To Write Record");
            Assert.AreEqual(1000, locations.Count, "Expected the number of entries for which we have locations to match the number of entries saved.");

            elapsed = 0;
            int recordCount = 0;
            foreach(var rec in singleRecordList)
            {
                s.Reset();
                s.Start();
                var matches = t.GetMatchingEntries(rec.Key);
                TestRecord<Guid> actual = matches.FirstOrDefault();
                Assert.AreEqual(rec.Key, actual.Key);
                Assert.AreEqual(rec.PayLoad, actual.PayLoad);
                s.Stop();
                elapsed += s.ElapsedMilliseconds;
                recordCount++;
            }
            Trace.WriteLine(elapsed/1000f + "ms", "Time To Retrieve Record");
        }
        [TestMethod]
        [TestCategory("Amanda Structured Storage")]
        public void AmandaEngine_StoreAndRetrieve5KRecords()
        {
            Assert.Inconclusive("This test method is for benchmarking only.  Do not run regularly.");
            AmandaEngine<TestRecord<Guid>, Guid> t = new AmandaEngine<TestRecord<Guid>, Guid>();
            string parentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var amandaParentFolder = new MemoryOnlyDirectory(parentFolder);
            var amandaFolder = t.CreateOrUseAmanda(amandaParentFolder);

            //save entries
            List<TestRecord<Guid>> singleRecordList = new List<TestRecord<Guid>>();
            for (int i = 0; i < 5000; i++)
            {
                singleRecordList.Add(new TestRecord<Guid>()
                {
                    Key = Guid.NewGuid(),
                    PayLoad = Guid.NewGuid().ToString()
                });
            }
            Stopwatch s = new Stopwatch();
            s.Start();
            var locations = t.SaveEntries(singleRecordList);
            s.Stop();
            double elapsed = s.ElapsedMilliseconds;
            Trace.WriteLine(elapsed / 5000f + "ms", "Average Time To Write Record");
            Assert.AreEqual(5000, locations.Count, "Expected the number of entries for which we have locations to match the number of entries saved.");

            elapsed = 0;
            int recordCount = 0;
            foreach (var rec in singleRecordList)
            {
                s.Reset();
                s.Start();
                var matches = t.GetMatchingEntries(rec.Key);
                TestRecord<Guid> actual = matches.FirstOrDefault();
                Assert.AreEqual(rec.Key, actual.Key);
                Assert.AreEqual(rec.PayLoad, actual.PayLoad);
                s.Stop();
                elapsed += s.ElapsedMilliseconds;
                recordCount++;
            }
            Trace.WriteLine(elapsed / 5000f + "ms", "Average Time To Retrieve Record");
        }

        [TestMethod]
        [TestCategory("Amanda Structured Storage")]
        public void AmandaEngine_StoreAndRetrieve1KRecordsInChunksOf100()
        {
            AmandaEngine<TestRecord<Guid>, Guid> t = new AmandaEngine<TestRecord<Guid>, Guid>();
            string parentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var amandaParentFolder = new MemoryOnlyDirectory(parentFolder);
            var amandaFolder = t.CreateOrUseAmanda(amandaParentFolder);

            //save entries
            Stopwatch s = new Stopwatch();
            List<TestRecord<Guid>> singleRecordList = new List<TestRecord<Guid>>();
            List<TestRecord<Guid>> allRecords = new List<TestRecord<Guid>>();
            double elapsed = 0;
            for(int j = 0; j < 10; j++)
            {
                for (int i = 0; i < 100; i++)
                {
                    singleRecordList.Add(new TestRecord<Guid>()
                    {
                        Key = Guid.NewGuid(),
                        PayLoad = Guid.NewGuid().ToString()
                    });
                }
                allRecords.AddRange(singleRecordList);
                s.Reset();
                s.Start();
                var locations = t.SaveEntries(singleRecordList);
                s.Stop();
                elapsed += s.ElapsedMilliseconds;
                singleRecordList.Clear();
            }
            Trace.WriteLine(elapsed / 1000f + "ms", "Average Time To Write Record");

            elapsed = 0;
            int recordCount = 0;
            foreach (var rec in allRecords)
            {
                s.Reset();
                s.Start();
                var matches = t.GetMatchingEntries(rec.Key);
                TestRecord<Guid> actual = matches.FirstOrDefault();
                Assert.AreEqual(rec.Key, actual.Key);
                Assert.AreEqual(rec.PayLoad, actual.PayLoad);
                s.Stop();
                elapsed += s.ElapsedMilliseconds;
                recordCount++;
            }
            Trace.WriteLine(elapsed / 1000f + "ms", "Time To Retrieve Record");
        }

        [TestMethod]
        public void AmandaEngine_StoreAndRetrieveRangeOf50RecordsIn1k()
        {
            AmandaEngine<TestRecord<long>, long> t = new AmandaEngine<TestRecord<long>, long>();
            string parentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var amandaParentFolder = new MemoryOnlyDirectory(parentFolder);
            var amandaFolder = t.CreateOrUseAmanda(amandaParentFolder);

            //save entries
            Stopwatch s = new Stopwatch();
            List<TestRecord<long>> singleRecordList = new List<TestRecord<long>>();
            List<TestRecord<long>> allRecords = new List<TestRecord<long>>();
            double elapsed = 0;
            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < 100; i++)
                {
                    singleRecordList.Add(new TestRecord<long>()
                    {
                        Key = (long)(i + 10 * j),
                        PayLoad = Guid.NewGuid().ToString()
                    });
                }
                allRecords.AddRange(singleRecordList);
                s.Reset();
                s.Start();
                var locations = t.SaveEntries(singleRecordList);
                s.Stop();
                elapsed += s.ElapsedMilliseconds;
                singleRecordList.Clear();
            }
            Trace.WriteLine(elapsed / 1000f + "ms", "Average Time To Write Record");
            s.Reset();
            s.Start();
            List<TestRecord<long>> foundRecords = t.GetRecordsInRange(25, 35);
            s.Stop();
            elapsed = s.ElapsedMilliseconds;
            Trace.WriteLine(elapsed / 1000f + "ms", "Average Time To Read Record");

            //if we have exactly as many distinct records as records, then the records must be distinct
            Assert.AreEqual(foundRecords.Distinct().Count(), foundRecords.Count);
            //if all records are in the range (24,36), then we've covered them all
            Assert.IsTrue(foundRecords.All(f => f.Key > 24 && f.Key < 36));
        }

        [TestMethod]
        public void AmandaEngine_StoreAndRetrieveRangeOf50RecordsIn1kForEntireDay()
        {
            AmandaEngine<TestRecord<long>, long> t = new AmandaEngine<TestRecord<long>, long>();
            string parentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var amandaParentFolder = new MemoryOnlyDirectory(parentFolder);
            var amandaFolder = t.CreateOrUseAmanda(amandaParentFolder);

            //save entries
            Stopwatch s = new Stopwatch();
            List<TestRecord<long>> singleRecordList = new List<TestRecord<long>>();
            List<TestRecord<long>> allRecords = new List<TestRecord<long>>();
            double elapsed = 0;
            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < 100; i++)
                {
                    singleRecordList.Add(new TestRecord<long>()
                    {
                        Key = (long)(i + 10 * j),
                        PayLoad = Guid.NewGuid().ToString()
                    });
                }
                allRecords.AddRange(singleRecordList);
                s.Reset();
                s.Start();
                var locations = t.SaveEntries(singleRecordList);
                s.Stop();
                elapsed += s.ElapsedMilliseconds;
                singleRecordList.Clear();
            }
            Trace.WriteLine(elapsed / 1000f + "ms", "Average Time To Write Record");
            s.Reset();
            s.Start();
            List<TestRecord<long>> foundRecords = t.GetEntriesBetweenDates(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));
            s.Stop();
            elapsed = s.ElapsedMilliseconds;
            Trace.WriteLine(elapsed / (double)foundRecords.Count + "ms", "Average Time To Read Record");

            //if we have exactly as many distinct records as records, then the records must be distinct
            Assert.AreEqual(foundRecords.Distinct().Count(), foundRecords.Count);
        }

        [TestMethod]
        [TestCategory("Amanda Structured Storage")]
        public void AmandaEngine_StoreAndRetrieve1KRecordsFromExistingData()
        {
            AmandaEngine<TestRecord<Guid>, Guid> t = new AmandaEngine<TestRecord<Guid>, Guid>();
            string parentFolder = Environment.CurrentDirectory + "\\testing-3";
            var amandaParentFolder = new MemoryOnlyDirectory(parentFolder);
            var amandaFolder = t.CreateOrUseAmanda(amandaParentFolder);

            //save entries
            List<TestRecord<Guid>> singleRecordList = new List<TestRecord<Guid>>();
            for (int i = 0; i < 1000; i++)
            {
                singleRecordList.Add(new TestRecord<Guid>()
                {
                    Key = Guid.NewGuid(),
                    PayLoad = Guid.NewGuid().ToString()
                });
            }
            Stopwatch s = new Stopwatch();
            s.Start();
            var locations = t.SaveEntries(singleRecordList);
            s.Stop();
            double elapsed = s.ElapsedMilliseconds;
            Trace.WriteLine(elapsed / 1000f + "ms", "Average Time To Write Record");
            Assert.AreEqual(1000, locations.Count, "Expected the number of entries for which we have locations to match the number of entries saved.");

            elapsed = 0;
            int recordCount = 0;
            //new instance and reload folder
            t = new AmandaEngine<TestRecord<Guid>, Guid>();
            t.CreateOrUseAmanda(amandaParentFolder);
            foreach (var rec in singleRecordList)
            {
                s.Reset();
                s.Start();
                var matches = t.GetMatchingEntries(rec.Key);
                TestRecord<Guid> actual = matches.FirstOrDefault();
                Assert.AreEqual(rec.Key, actual.Key);
                Assert.AreEqual(rec.PayLoad, actual.PayLoad);
                s.Stop();
                elapsed += s.ElapsedMilliseconds;
                recordCount++;
            }
            Trace.WriteLine(elapsed / 1000f + "ms", "Time To Retrieve Record");
        }
    }
}
