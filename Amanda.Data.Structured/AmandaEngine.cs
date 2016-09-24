using Amanda.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Amanda.Time;
using Amanda.IO;

namespace Amanda.Data.Structured
{
    public class AmandaEngine<TRecordType,TKeyField> where TRecordType :IAmandaRecord<TKeyField>
    {
        IAmandaIndex<TKeyField> _defaultIndex;
        IAmandaIndex<long> _timeIndex;
        AmandaDb<TRecordType> _database;
        private Amanda.IO.IAmandaDirectory _amandaFolder;
        public bool IsInitialized
        {
            get;
            protected set;
        }
        public Amanda.IO.IAmandaDirectory RootFolder
        {
            get { return _amandaFolder; }
        }
        public AmandaEngine()
        {
            this.IsInitialized = false;
        }

        /// <summary>
        /// Initializes the directory as an Amanda directory.
        /// </summary>
        /// <remarks>
        /// The main part of this is to create the .amanda subfolder.  It will throw an
        /// exception if it fails.  If it succeeds, it will return a new IAmandaFolder
        /// which is the newly createdsubfolder where Amanda data files will live.
        /// 
        /// We claim no ownership over the root directory.  Therefore, if it doesn't exist,
        /// we throw an IOException.
        /// </remarks>
        public Amanda.IO.IAmandaDirectory CreateOrUseAmanda(Amanda.IO.IAmandaDirectory parentRootFolder)
        {
            //check for existence
            if (parentRootFolder == null || !parentRootFolder.Exists)
                throw new IOException("The specified engine parent directory is null or does not exist.");

            _amandaFolder = parentRootFolder.CreateOrUseSubdirectory(".amanda");

            //create the default index
            _defaultIndex = new FileBasedIndex<TKeyField>();
            _defaultIndex.CreateOrUseIndex(_amandaFolder, ".keyFieldIndex");

            _timeIndex = new FileBasedIndex<long>();
            _timeIndex.CreateOrUseIndex(_amandaFolder, ".timeBasedIndex");

            //create the database
            _database = new AmandaDb<TRecordType>();
            _database.CreateOrUseAmandaDb(_amandaFolder);
            
            this.IsInitialized = true;
            return _amandaFolder;
        }

        /// <summary>
        /// Must be initialized first.  Call CreateOrUseAmanda().
        /// </summary>
        /// <param name="records"></param>
        public Dictionary<TRecordType, RowLocation> SaveEntries(List<TRecordType> records)
        {
            try
            {
                if (!this.IsInitialized)
                    throw new InvalidOperationException("Amanda Structured Storage must be initialized prior to use.");

                using(var lf = new LockFile("save_entries", _amandaFolder))
                {
                    var inserted = _database.AddRecords(records);
                    foreach (var rec in inserted)
                    {
                        _defaultIndex.AddReference(rec.Key.Key, rec.Value);
                        _timeIndex.AddReference(DateTime.UtcNow.Ticks, rec.Value);
                    }
                    return inserted;
                }
            }
            catch (Exception)
            {
                    throw;
                //throw new IOException("Failed to store values.  Please verify that the current user has permission to the folder.", ex);
            }
        }

        /// <summary>
        /// Gets records between the supplied dates (inclusive).
        /// </summary>
        public List<TRecordType> GetEntriesBetweenDates(DateTime startDateUtc, DateTime endDateUtc)
        {
            List<TRecordType> records = new List<TRecordType>();
            
            List<RowLocation> locations = _timeIndex.FindRecordsBetween(startDateUtc.Ticks, endDateUtc.Ticks);
            foreach (var location in locations)
            {
                records.Add(_database.GetRecordsAtLocation(location).First());
            }
            
            return records;
        }

        /// <summary>
        /// Creates a new view based the keys provided.  Anything strictly less
        /// than the firstRecord key is excluded, and anything strictly greater than
        /// the lastRecordKey is omitted.  Exact matches are included.
        /// </summary>
        /// <remarks>
        /// This uses the <see cref="GetRecordsInRange(TKeyField, TKeyField)"/> method to 
        /// determine what is included, and what's not.
        /// </remarks>
        public AmandaView BeginView(List<object> records)
        {
            AmandaView amandaView = new AmandaView();
            amandaView.InitializeViewFromData(records);
            return amandaView;
        }

        public bool HasRecord(TKeyField key)
        {
            return _defaultIndex.FindRecord(key).Count > 0;
        }

        public List<TRecordType> GetMatchingEntries(TKeyField key)
        {
            var records = _defaultIndex.FindRecord(key);
            var location = records.FirstOrDefault();
            if (location != null)
                return _database.GetRecordsAtLocation(location);
            else
                return new List<TRecordType>();//empty list
        }

        public List<TRecordType> GetRecordsInRange(TKeyField lowerBounds, TKeyField upperBounds)
        {
            List<TRecordType> records = new List<TRecordType>();
            
            List<RowLocation> locations = _defaultIndex.FindRecordsBetween(lowerBounds, upperBounds);
            foreach(var location in locations)
            {
                records.Add(_database.GetRecordsAtLocation(location).First());
            }
            
            return records;
        }
    }
}
