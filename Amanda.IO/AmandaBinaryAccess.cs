using Amanda.Data;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Amanda.IO
{
    public class BinaryFileSystemAccess<TRecordType>
    {
        private Amanda.IO.IAmandaDirectory _workingFolder;

        private double _maxFileSizeInGigabytes;
        private const long READ_BLOCK_SIZE = 1024;

        JsonSerializer serializer = new JsonSerializer();

        public bool IsInitialized
        {
            get;
            protected set;
        }
        public BinaryFileSystemAccess(double maxFileSizeInGB = 5)
        {
            IsInitialized = false;
            _maxFileSizeInGigabytes = maxFileSizeInGB;
        }

        /// <summary>
        /// PageSize is measured in gigabytes.
        /// </summary>
        public void CreateOrUseFileAccess(Amanda.IO.IAmandaDirectory workingFolder)
        {
            _workingFolder = workingFolder;
            if (workingFolder == null || !_workingFolder.Exists)
                throw new IOException("The specified file access working directory is null or does not exist.");

            IsInitialized = true;
        }

        public TRecordType GetFirstRecord()
        {
            RowLocation location = new RowLocation() { Offset = 0 };
            location.CurrentPageFile = "0.adf";
            return this.ReadAtLocation(location).FirstOrDefault();
        }

        /// <remarks>
        /// Page file names are just numbers, in the order of creation.
        /// </remarks>
        /// <exception cref="InvalidOperationException">When FileSystemAccess is not initialized (CreateOrUseFileAccess has not been called).</exception>
        private RowLocation DetermineInsertionPoint()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Please call CreateOrUseFileAccess prior to using this method.");
            Amanda.IO.IAmandaFile file = _workingFolder.GetNewestFile("*.adf");
            RowLocation location = new RowLocation() { Offset = 0 };
            if (file == null) //if we don't have any files, let's use 0.adf
            {
                location.CurrentPageFile = "0.adf";
            }
            else if (file.IsLargerThan(_maxFileSizeInGigabytes)) //create a new file in this case
            {
                string relativeFileName = file.GetNameWithoutExtension();
                int index = Int32.Parse(relativeFileName);
                relativeFileName = (++index).ToString() + ".adf";
                file = _workingFolder.CreateNewFile(relativeFileName);
                location.CurrentPageFile = relativeFileName;
                location.Offset = 0;
            }
            else //we can use the currentfile
            {
                location.CurrentPageFile = file.GetRelativeFileName();
                location.Offset = GetNextWriteLocation(file);
            }
            return location;
        }

        private int GetNextWriteLocation(IAmandaFile file)
        {
            return (int)(file.Size / READ_BLOCK_SIZE);
        }

        public RowLocation TruncateAndOverwriteCurrentFile(TRecordType record)
        {
            var location = DetermineInsertionPoint();
            var tempFile = location.CurrentPageFile;
            location.Offset = 0;
            List<TRecordType> records = new List<TRecordType>();
            records.Add(record);
            _workingFolder.DeleteFile(tempFile);
            IAmandaFile file = _workingFolder.Touch(tempFile);
            AppendRecordsToFile(records, file);
            return location;
        }

        private void AppendRecordsToFile(List<TRecordType> records, IAmandaFile file)
        {
            bool written = false;
            int count = 10;
            int loop = 0;
            do
            {
                loop++;
                try
                {
                    using (var stream = file.OpenWrite())
                    {
                        stream.Position = stream.Length;//position at end of stream
                        using (StreamWriter sw = new StreamWriter(stream))
                        {
                            using (JsonTextWriter jw = new JsonTextWriter(sw))
                            {
                                foreach (var rec in records)
                                    serializer.Serialize(jw, rec);
                            }
                        }
                        written = true;
                    }
                }catch(IOException)
                {
                    if(loop == count)
                        throw;
                }
            } while (!written);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when records paremeter is null.</exception>
        /// <returns>
        /// An empty list, if no records match.  Any exceptions while preparing to insert are not handled here.
        /// </returns>
        public Dictionary<TRecordType, RowLocation> AppendToNewestFile(List<TRecordType> records)
        {
            if (records == null)
                throw new ArgumentNullException("records", "Please ensure that the records list passed in is not null.");

            Dictionary<TRecordType, RowLocation> insertedRecords = new Dictionary<TRecordType, RowLocation>();
            if (!records.Any())
                return insertedRecords;//empty though

            //find next available page
            var location = DetermineInsertionPoint();

            //get preceding bytes
            var file = _workingFolder.Touch(location.CurrentPageFile);

            List<string> rows = new List<string>();
            int offset = location.Offset;
            byte[] newLine = new byte[] { (byte)'\n' };
            string tempName = location.CurrentPageFile + ".tmp";
            var tempFile = _workingFolder.CreateNewFile(tempName);
            AppendRecordsToFile(records, tempFile);
            foreach(var rec in records)
            {
                insertedRecords.Add(rec, new RowLocation() { Offset = offset++, CurrentPageFile = location.CurrentPageFile});
            }
            try
            {
                //merge the temp file into the current file
                if(insertedRecords.Count > 0)
                {
                    _workingFolder.MergeFileIntoExisting(tempName, location.CurrentPageFile);
                }
                return insertedRecords;
            }
            catch (Exception)
            {
                throw;//to be replaced with instrumentation
            }
        }

        public List<TRecordType> ReadAtLocation(RowLocation location, int numberOfRecordsToRead = 1)
        {
            if (location == null)
                throw new ArgumentNullException("location", "The location provided to ReadAtLocation was null.");
            if (!IsInitialized)
                throw new InvalidOperationException("Please call CreateOrUseFileAccess prior to attempting to read.");
            //this only works because we're appending or updating, and not inserting
            List<TRecordType> recordsToReturn = new List<TRecordType>();
            IAmandaFile file = _workingFolder.GetFile(location.CurrentPageFile);
            if (!file.Exists)
                return recordsToReturn;
            int numRecordsRead = 0;
            int position = 0;
            using (var stream = file.OpenRead())
            {
                stream.Position = 0;
                using(StreamReader sr = new StreamReader(stream))
                {
                    using(JsonTextReader jr= new JsonTextReader(sr))
                    {
                        jr.SupportMultipleContent = true;
                        while(position < location.Offset)
                        {
                            if (!jr.Read())
                                break;
                            serializer.Deserialize<TRecordType>(jr);//read and throw away
                            position++;
                        }
                        while (numRecordsRead < numberOfRecordsToRead)
                        {
                            if (!jr.Read())
                                break;
                            TRecordType record = serializer.Deserialize<TRecordType>(jr);
                            recordsToReturn.Add(record);
                            numRecordsRead++;
                        }
                    }
                }
            }
            return recordsToReturn;
        }
    }
}
