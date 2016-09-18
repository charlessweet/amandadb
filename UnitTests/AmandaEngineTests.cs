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
        private const string LOREM_8K = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis eget auctor erat, eget rutrum felis. Nulla iaculis malesuada lorem et molestie. Nullam eget placerat mi. Curabitur iaculis mattis tristique. Sed porta nulla id nunc ultricies, quis auctor libero imperdiet. Sed et sem vel ligula lobortis commodo. Pellentesque ornare id nulla nec vestibulum. Pellentesque lobortis, neque et rhoncus fermentum, sapien orci iaculis lacus, vitae laoreet justo sem et neque. Fusce eleifend ac dui a dignissim. Aenean mattis bibendum sem et mattis. Morbi congue lacus eros, quis interdum lacus auctor non. Morbi et est leo. Nunc odio metus, luctus eget blandit ac, vestibulum eget mi. Sed hendrerit neque enim, varius pharetra neque fringilla vitae. In maximus hendrerit nulla non sodales. Vestibulum accumsan aliquet eros, et elementum orci accumsan non.

Nunc vehicula viverra diam in accumsan. Donec et risus venenatis velit fermentum scelerisque luctus at arcu. Praesent et facilisis velit. Aenean viverra sem augue. Ut ipsum elit, iaculis nec congue in, semper laoreet nulla. Proin semper pellentesque gravida. Pellentesque nec sodales odio. Ut pretium lacus lacus, quis ullamcorper risus sagittis non. Aenean lobortis erat sapien, in convallis dolor gravida sed.

Donec gravida eget ex sit amet elementum. Fusce lacinia tortor vel faucibus sagittis. Nam eget porta arcu, at venenatis lacus. Proin id ultrices velit. Quisque pretium commodo tempor. Quisque velit lorem, sollicitudin consequat ornare eget, elementum in risus. Nullam in feugiat odio, ut facilisis massa. Donec et nunc ut lacus sodales euismod a sed diam. Phasellus sollicitudin vitae sem ac tincidunt. Donec nec ex porttitor, cursus justo et, ultrices risus. Maecenas pulvinar eros vel urna rhoncus dictum.

Pellentesque iaculis, enim vel semper fringilla, orci arcu lacinia tortor, sagittis volutpat orci libero id lectus. Nulla blandit ligula ipsum, elementum aliquet purus tincidunt vel. Sed convallis ipsum et lacus interdum pharetra. Nulla ac condimentum tellus, eget congue urna. Donec ornare in ligula sed ornare. Integer gravida nibh vitae lorem scelerisque porttitor. Sed maximus malesuada convallis.

Nam egestas mi nec bibendum scelerisque. Vestibulum semper tincidunt accumsan. Phasellus lacus massa, volutpat et ligula vel, hendrerit ultricies diam. Donec venenatis, ex vel aliquam bibendum, orci magna ullamcorper velit, sed feugiat orci ante et augue. Donec hendrerit orci eu turpis mattis sagittis nec ac nisl. Sed viverra risus eget dolor volutpat, id dapibus lectus interdum. Sed dolor neque, porta semper venenatis sed, sollicitudin vitae enim. Phasellus sagittis volutpat efficitur. Quisque euismod laoreet tellus, nec accumsan ipsum gravida non. Donec sed mattis ipsum.

Nulla facilisi. Ut ornare sollicitudin massa imperdiet aliquet. Nam finibus aliquet urna quis molestie. Nunc in justo facilisis, interdum libero a, dignissim augue. Nullam congue consectetur massa sed tempor. Fusce massa diam, rhoncus in lacinia sed, tincidunt eget orci. Proin tortor libero, fermentum vitae ipsum eget, hendrerit vehicula mi. Vivamus ultrices non lectus at pretium. Praesent a urna feugiat, finibus nisl ac, convallis mauris. Aenean pulvinar orci ut molestie maximus. Proin ut turpis eu arcu consectetur interdum. Vivamus ornare sem risus, nec semper dolor malesuada ac. Sed rutrum lacus sed nibh posuere aliquet. Aliquam erat volutpat. Ut aliquam tempor arcu id sodales. In dignissim lacus eu aliquet sollicitudin.

Praesent fermentum ante in lectus varius, ac pharetra leo bibendum. Aenean vel diam in urna imperdiet tincidunt. Ut tincidunt lectus varius, aliquam ex eget, vehicula enim. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Curabitur egestas sit amet tortor et ultricies. Nam quam odio, sodales et rhoncus et, semper nec nulla. Etiam dictum, nisl nec elementum pharetra, mauris velit accumsan velit, at volutpat est eros id massa. Fusce luctus, ex ut dignissim convallis, ipsum lectus bibendum tellus, id ultricies ante arcu nec odio. Suspendisse bibendum turpis massa, eu efficitur erat rhoncus ac.

Duis nec odio volutpat, elementum ex ut, efficitur urna. Cras nec eros ex. Maecenas ex augue, efficitur ut ultricies eu, molestie nec metus. Nullam laoreet elementum eleifend. Vestibulum fringilla justo a sapien interdum pulvinar. Mauris euismod egestas consequat. Maecenas sapien tellus, congue non porta a, eleifend et diam. Aliquam lacinia ornare dui, eget bibendum massa interdum sed. Quisque et urna magna. Sed at augue at purus faucibus fringilla quis at diam. Suspendisse mattis porta nulla, a aliquet mi accumsan quis. Duis ac enim hendrerit, commodo enim vitae, varius est. Sed consectetur libero sit amet efficitur lacinia. In a tellus convallis, pellentesque turpis a, pellentesque neque. Duis massa orci, blandit a metus nec, vulputate dictum diam. Vivamus eu interdum nulla, non ullamcorper felis.

Suspendisse lectus risus, aliquet vel leo quis, sagittis posuere magna. Phasellus eu ligula lorem. Mauris id enim urna. Phasellus varius, sem non laoreet mollis, urna elit tincidunt nisi, ac finibus mauris sapien id tellus. Aenean varius quis felis a elementum. Suspendisse potenti. In accumsan lorem eu consequat vestibulum. Duis bibendum at ligula sit amet eleifend. Sed condimentum quis augue vitae interdum. Mauris condimentum est urna, et cursus lacus consequat in. Praesent sem diam, dictum vitae mi sit amet, mollis condimentum erat. Nunc maximus, ipsum at ultrices porta, odio ex tincidunt lorem, vel vulputate dui ligula in ex.

Vivamus aliquam pellentesque quam, a convallis nisi posuere ut. Curabitur id scelerisque urna. Cras vitae libero felis. Morbi viverra nulla ante. Duis auctor dictum nibh non vehicula. Sed facilisis nec arcu sed rhoncus. Sed efficitur sapien semper felis egestas euismod. Duis venenatis tortor at turpis consectetur eleifend. Nam porta arcu eu mi elementum dapibus. Donec pulvinar dictum ultrices. Nullam sagittis pretium posuere. Aliquam consectetur risus ac velit sodales, at bibendum est faucibus. Quisque sagittis lectus eu ultrices blandit. Curabitur et libero non elit hendrerit vulputate. Integer eu porta dui. Maecenas commodo, nibh eget volutpat ullamcorper, orci est porta libero, sed fermentum turpis augue et erat.

Ut dapibus elit at nibh sodales tempus. Donec cursus, turpis scelerisque dictum ornare, dui turpis ornare urna, vitae blandit tellus dui id nunc. Praesent nec aliquam nisi. Pellentesque gravida ligula quis enim sagittis tempus. Pellentesque tincidunt diam nec fringilla sollicitudin. Curabitur hendrerit convallis fringilla. Phasellus augue libero, gravida et sodales vulputate, sagittis eget sem. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Praesent nulla arcu, fermentum a consequat vitae, gravida vel risus.

Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Vestibulum sagittis pretium efficitur. Praesent lobortis fermentum nibh, a semper diam. Donec sodales fermentum massa quis elementum. Vestibulum a aliquam risus. Maecenas eleifend nibh eget aliquet volutpat. Quisque elementum condimentum quam, nec imperdiet magna ullamcorper nec. Sed quis dolor vel eros dapibus hendrerit. Fusce sodales enim a ipsum congue maximus. Pellentesque eu diam tristique, elementum purus sit amet, scelerisque augue. Donec vitae dolor laoreet, cursus nulla eget, accumsan orci. Nulla sollicitudin sit amet sapien varius tincidunt.

Cras euismod elit nisi, sit amet bibendum eros dapibus id. Proin ullamcorper sollicitudin odio vitae faucibus. Sed et sodales urna. Vivamus sagittis ac augue nec blandit. Vivamus imperdiet egestas tempor. Etiam tristique nec velit ut euismod. In non justo feugiat, dictum leo vitae, volutpat ligula. Vivamus venenatis lectus sem, imperdiet sodales sem pellentesque sit amet. Aliquam libero eros, pharetra vitae volutpat vel, dignissim ac eros. Nullam euismod gravida finibus. Suspendisse tempor nisl vel erat pulvinar, sed tincidunt tortor scelerisque. Nulla id magna in ante efficitur bibendum. Curabitur tristique, magna sed tristique imperdiet, magna odio feugiat nisl, sed facilisis augue massa posuere.";
        [TestMethod]
        [TestCategory("Amanda Structured Storage")]
        public void AmandaEngine_IOExceptionWhenParentFolderDoesNotExist()
        {
            AmandaEngine<TestRecord<Guid>, Guid> sqda = new AmandaEngine<TestRecord<Guid>, Guid>();

            var amandaParentFolder = new Amanda.IO.PhysicalDirectory(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().FullName) + @"\" + Guid.NewGuid().ToString());
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
            
            var amandaParentFolder = new Amanda.IO.PhysicalDirectory(@"c:\Windows\System32\FAKE");
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
            var amandaParentFolder = new PhysicalDirectory(parentFolder);
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
            amandaParentFolder = new PhysicalDirectory(parentFolder);
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
            var amandaParentFolder = new PhysicalDirectory(parentFolder);
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
            sw.Start();
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
        [TestCategory("8K")]
        public void AmandaEngine_StoreAndRetrieve1K_8KRecordsFromExistingData()
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
                    PayLoad = LOREM_8K
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
            double elapsedRetrieve = 0;
            int recordCount = 0;
            //new instance and reload folder
            t = new AmandaEngine<TestRecord<Guid>, Guid>();
            t.CreateOrUseAmanda(amandaParentFolder);
            Stopwatch sw = new Stopwatch();
            foreach (var rec in singleRecordList)
            {
                s.Reset();
                s.Start();
                var matches = t.HasRecord(rec.Key);
                s.Stop();
                Assert.IsTrue(matches);
                sw.Start();
                var r2 = t.GetMatchingEntries(rec.Key);
                sw.Stop();
                elapsedRetrieve += sw.ElapsedMilliseconds;
                elapsed += s.ElapsedMilliseconds;
                recordCount++;
            }
            Trace.WriteLine(elapsed / 1000f + "ms", "Time To Find Record");
            Trace.WriteLine(elapsedRetrieve / 1000f + "ms", "Time To Retrieve Record");
        }

        [TestMethod]
        [TestCategory("Amanda Structured Storage")]
        public void AmandaEngine_StoreAndRetrieve1KRecordsFromExistingData()
        {
            Assert.Inconclusive("This is not a valid test for in-memory only access.");
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
