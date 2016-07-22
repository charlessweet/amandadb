using Amanda.Data;
using Amanda.IO.MemoryOnly;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmandaStructuredStorage
{
    [TestClass]
    public class FileSystemAccessTests
    {
        [TestMethod]
        [TestCategory("File System Access")]
        public void FileSystemAccess_IOExceptionWhenWorkingFolderDoesNotExist()
        {
            var t = new Amanda.IO.FileSystemAccess<TestRecord<Guid>>();
            try
            {
                t.CreateOrUseFileAccess(null);
                Assert.Fail("Expected exception when creating with null working folder.");
            }
            catch (System.IO.IOException ex)
            {
                Assert.AreEqual("The specified file access working directory is null or does not exist.", ex.Message);
            }

            var workingFolder = new MemoryOnlyDirectory("test_path", false);
            try
            {
                t.CreateOrUseFileAccess(workingFolder);
                Assert.Fail("Expected exception when creating with non-existent working folder.");
            }
            catch (System.IO.IOException ex)
            {
                Assert.AreEqual("The specified file access working directory is null or does not exist.", ex.Message);
            }

            workingFolder = new MemoryOnlyDirectory("test_path", true);
            t.CreateOrUseFileAccess(workingFolder);
        }

        [TestMethod]
        [TestCategory("File System Access")]
        public void FileSystemAccess_AppendBeforeInitialized()
        {
            var t = new Amanda.IO.FileSystemAccess<TestRecord<Guid>>();
            var workingFolder = new MemoryOnlyDirectory("test_path", true);
            TestRecord<Guid> tr = new TestRecord<Guid>()
            {
                Key = Guid.NewGuid(),
                PayLoad = Guid.NewGuid().ToString()
            };
            var trlist = new List<TestRecord<Guid>>();
            trlist.Add(tr);
            try
            {
                t.AppendToNewestFile(trlist);
                Assert.Fail("Expected exception when calling AppendToNewestFile.");
            }catch(InvalidOperationException ex)
            {
                Assert.AreEqual("Please call CreateOrUseFileAccess prior to using this method.", ex.Message);
            }
        }

        [TestMethod]
        [TestCategory("File System Access")]
        public void FileSystemAccess_AppendInvalidRecords()
        {
            var t = new Amanda.IO.FileSystemAccess<TestRecord<Guid>>();
            var workingFolder = new MemoryOnlyDirectory("test_path", true);
            t.CreateOrUseFileAccess(workingFolder);
            try
            {
                t.AppendToNewestFile(null);
                Assert.Fail("Expected exception when calling AppendToNewestFile.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("Please ensure that the records list passed in is not null."));
            }
            var trlist = new List<TestRecord<Guid>>();
            var actual = t.AppendToNewestFile(trlist);
            Assert.AreEqual(0, actual.Count);

            TestRecord<Guid> tr = new TestRecord<Guid>()
            {
                Key = Guid.NewGuid(),
                PayLoad = Guid.NewGuid().ToString()
            };
            trlist.Add(tr);
        }

        [TestMethod]
        [TestCategory("File System Access")]
        public void FileSystemAccess_AppendAndRetrieveSingleRecord()
        {
            var t = new Amanda.IO.FileSystemAccess<TestRecord<Guid>>();
            var workingFolder = new MemoryOnlyDirectory("test_path", true);
            t.CreateOrUseFileAccess(workingFolder);
            var trlist = new List<TestRecord<Guid>>();
            TestRecord<Guid> tr = new TestRecord<Guid>()
            {
                Key = Guid.NewGuid(),
                PayLoad = Guid.NewGuid().ToString()
            };
            trlist.Add(tr);
            var actual = t.AppendToNewestFile(trlist);
            Assert.AreEqual(1, actual.Count);

            var rec = t.ReadAtLocation(actual.First().Value);
            Assert.AreEqual(tr.Key, rec.First().Key);
            Assert.AreEqual(tr.PayLoad, rec.First().PayLoad);
        }

        [TestMethod]
        [TestCategory("File System Access")]
        public void FileSystemAccess_AppendAndRetrieve1KRecords()
        {
            var t = new Amanda.IO.FileSystemAccess<TestRecord<Guid>>();
            var workingFolder = new MemoryOnlyDirectory("test_path", true);
            t.CreateOrUseFileAccess(workingFolder);
            var trlist = new List<TestRecord<Guid>>();
            for(var i = 0; i < 1000; i++)
            {
                trlist.Add(new TestRecord<Guid>()
                {
                    Key = Guid.NewGuid(),
                    PayLoad = Guid.NewGuid().ToString()
                });
            }
            Stopwatch s = new Stopwatch();
            s.Start();
            var actual = t.AppendToNewestFile(trlist);
            s.Stop();
            double elapsed = s.ElapsedMilliseconds;
            Trace.WriteLine(elapsed/1000 + "ms Average Write Time");
            Assert.AreEqual(1000, actual.Count);

            elapsed = 0;
            foreach(var locVal in actual)
            {
                s.Reset();
                s.Start();
                var rec = t.ReadAtLocation(locVal.Value);
                s.Stop();
                elapsed += s.ElapsedMilliseconds;
                Assert.AreEqual(locVal.Key.Key, rec.First().Key);
                Assert.AreEqual(locVal.Key.PayLoad, rec.First().PayLoad);
            }
            Trace.WriteLine(elapsed / 1000 + "ms Average Read Time");
        }

        [TestMethod]
        [TestCategory("File System Access")]
        public void FileSystemAccess_AppendAndRetrieve5KRecords()
        {
            Assert.Inconclusive("This test method is for benchmarking only.  Do not run regularly.");
            var t = new Amanda.IO.FileSystemAccess<TestRecord<Guid>>();
            var workingFolder = new MemoryOnlyDirectory("test_path", true);
            t.CreateOrUseFileAccess(workingFolder);
            var trlist = new List<TestRecord<Guid>>();
            for (var i = 0; i < 5000; i++)
            {
                trlist.Add(new TestRecord<Guid>()
                {
                    Key = Guid.NewGuid(),
                    PayLoad = Guid.NewGuid().ToString()
                });
            }
            Stopwatch s = new Stopwatch();
            s.Start();
            var actual = t.AppendToNewestFile(trlist);
            s.Stop();
            double elapsed = s.ElapsedMilliseconds;
            Trace.WriteLine(elapsed / 5000 + "ms Average Write Time");
            Assert.AreEqual(5000, actual.Count);

            elapsed = 0;
            foreach (var locVal in actual)
            {
                s.Reset();
                s.Start();
                var rec = t.ReadAtLocation(locVal.Value);
                s.Stop();
                elapsed += s.ElapsedMilliseconds;
                Assert.AreEqual(locVal.Key.Key, rec.First().Key);
                Assert.AreEqual(locVal.Key.PayLoad, rec.First().PayLoad);
            }
            Trace.WriteLine(elapsed / 5000 + "ms Average Read Time");
        }

        [TestMethod]
        [TestCategory("File System Access")]
        public void FileSystemAccess_GetFirstRecordBeforeInitialized()
        {
            var t = new Amanda.IO.FileSystemAccess<TestRecord<Guid>>();
            var workingFolder = new MemoryOnlyDirectory("test_path", true);
            try
            {
                t.GetFirstRecord();
                Assert.Fail("Expected failure before FileSystemAccess is initialized.");
            }catch(InvalidOperationException ex)
            {
                Assert.AreEqual("Please call CreateOrUseFileAccess prior to attempting to read.", ex.Message);
            }
        }

        [TestMethod]
        [TestCategory("File System Access")]
        public void FileSystemAccess_GetFirstRecordWhenNoRecordsExist()
        {
            var t = new Amanda.IO.FileSystemAccess<TestRecord<Guid>>();
            var workingFolder = new MemoryOnlyDirectory("test_path", true);
            t.CreateOrUseFileAccess(workingFolder);
            var record = t.GetFirstRecord();
            Assert.IsNull(record);
        }
        [TestMethod]
        [TestCategory("File System Access")]
        public void FileSystemAccess_GetFirstRecordSucceeds()
        {
            var t = new Amanda.IO.FileSystemAccess<TestRecord<Guid>>();
            var workingFolder = new MemoryOnlyDirectory("test_path", true);
            t.CreateOrUseFileAccess(workingFolder);
            var trlist = new List<TestRecord<Guid>>();
            for (var i = 0; i < 10; i++)
            {
                trlist.Add(new TestRecord<Guid>()
                {
                    Key = Guid.NewGuid(),
                    PayLoad = Guid.NewGuid().ToString()
                });
            }
            t.AppendToNewestFile(trlist);
            var record = t.GetFirstRecord();
            Assert.AreEqual(trlist.First().Key, record.Key);
            Assert.AreEqual(trlist.First().PayLoad, record.PayLoad);
        }

        [TestMethod]
        [TestCategory("File System Access")]
        public void FileSystemAccess_ReadAtLocationInvalidLocation()
        {
            var t = new Amanda.IO.FileSystemAccess<TestRecord<Guid>>();
            var workingFolder = new MemoryOnlyDirectory("test_path", true);
            t.CreateOrUseFileAccess(workingFolder);
            try
            {
                t.ReadAtLocation(null);
                Assert.Fail("Expected an exception when reading a null location.");
            }catch(ArgumentNullException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("The location provided to ReadAtLocation was null."));
            }
            var location = new RowLocation() {
                CurrentPageFile = "20.adf",
                Offset = 19
            };
            var records = t.ReadAtLocation(location);
            Assert.AreEqual(0, records.Count);
        }

        [TestMethod]
        [TestCategory("File System Access")]
        public void FileSystemAccess_AppendAndRetrieve1KRecordsInBulk()
        {
            var t = new Amanda.IO.FileSystemAccess<TestRecord<Guid>>();
            var workingFolder = new MemoryOnlyDirectory("test_path", true);
            t.CreateOrUseFileAccess(workingFolder);
            var trlist = new List<TestRecord<Guid>>();
            for (var i = 0; i < 1000; i++)
            {
                trlist.Add(new TestRecord<Guid>()
                {
                    Key = Guid.NewGuid(),
                    PayLoad = Guid.NewGuid().ToString()
                });
            }
            Stopwatch s = new Stopwatch();
            s.Start();
            var actual = t.AppendToNewestFile(trlist);
            s.Stop();
            double elapsed = s.ElapsedMilliseconds;
            Trace.WriteLine(elapsed / 1000 + "ms Average Write Time");
            Assert.AreEqual(1000, actual.Count);

            elapsed = 0;
            var locVal = actual.First();
            s.Reset();
            s.Start();
            var recs = t.ReadAtLocation(locVal.Value, 1000);
            s.Stop();
            Assert.AreEqual(1000, recs.Count);
            elapsed += s.ElapsedMilliseconds;
            foreach(var rec in recs)
            {
                var match = trlist.First(tr => tr.Key == rec.Key);
                Assert.AreEqual(rec.Key, match.Key);
                Assert.AreEqual(rec.PayLoad, match.PayLoad);
            }
            Trace.WriteLine(elapsed / 1000 + "ms Average Read Time");
        }
        [TestMethod]
        [TestCategory("File System Access")]
        public void FileSystemAccess_AppendAndRetrieve5KRecordsInBulk()
        {
            var t = new Amanda.IO.FileSystemAccess<TestRecord<Guid>>();
            var workingFolder = new MemoryOnlyDirectory("test_path", true);
            t.CreateOrUseFileAccess(workingFolder);
            var trlist = new List<TestRecord<Guid>>();
            for (var i = 0; i < 5000; i++)
            {
                trlist.Add(new TestRecord<Guid>()
                {
                    Key = Guid.NewGuid(),
                    PayLoad = Guid.NewGuid().ToString()
                });
            }
            Stopwatch s = new Stopwatch();
            s.Start();
            var actual = t.AppendToNewestFile(trlist);
            s.Stop();
            double elapsed = s.ElapsedMilliseconds;
            Trace.WriteLine(elapsed / 5000 + "ms Average Write Time");
            Assert.AreEqual(5000, actual.Count);

            elapsed = 0;
            var locVal = actual.First();
            s.Reset();
            s.Start();
            var recs = t.ReadAtLocation(locVal.Value, 5000);
            s.Stop();
            Assert.AreEqual(5000, recs.Count);
            elapsed += s.ElapsedMilliseconds;
            foreach (var rec in recs)
            {
                var match = trlist.First(tr => tr.Key == rec.Key);
                Assert.AreEqual(rec.Key, match.Key);
                Assert.AreEqual(rec.PayLoad, match.PayLoad);
            }
            Trace.WriteLine(elapsed / 5000 + "ms Average Read Time");
        }

        [TestMethod]
        [TestCategory("File System Access")]
        public void FileSystemAccess_AppendAndRetrieve1KRecordsExceedingMaxFileSize()
        {
            var t = new Amanda.IO.FileSystemAccess<TestRecord<Guid>>(0.00001);
            var workingFolder = new MemoryOnlyDirectory("test_path", true);
            t.CreateOrUseFileAccess(workingFolder);
            var trlist = new List<TestRecord<Guid>>();
            double averageWrite = 0;
            double averageRead = 0;
            for(var j = 0; j < 10; j++)
            {
                for (var i = 0; i < 100; i++)
                {
                    trlist.Add(new TestRecord<Guid>()
                    {
                        Key = Guid.NewGuid(),
                        PayLoad = Guid.NewGuid().ToString()
                    });
                }
                Stopwatch s = new Stopwatch();
                s.Start();
                var actual = t.AppendToNewestFile(trlist);
                s.Stop();
                double loopIndex = j;
                averageWrite = (s.ElapsedMilliseconds / 100 + (averageWrite * loopIndex)) / (loopIndex + 1);
                Assert.AreEqual(100, actual.Count);
                averageRead = 0;
                double elapsedRead = 0;
                foreach (var locVal in actual)
                {
                    s.Reset();
                    s.Start();
                    var rec = t.ReadAtLocation(locVal.Value);
                    s.Stop();
                    elapsedRead += s.ElapsedMilliseconds;
                    Assert.AreEqual(locVal.Key.Key, rec.First().Key);
                    Assert.AreEqual(locVal.Key.PayLoad, rec.First().PayLoad);
                }
                trlist.Clear();
                averageRead = ((elapsedRead / 100) + (averageRead * loopIndex)) / (loopIndex + 1);
            }
            Trace.WriteLine(averageWrite + "ms", "Average Write Time");
            Trace.WriteLine(averageRead + "ms", "Average Read Time");
        }
    }
}
