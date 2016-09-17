using Amanda.IO;
using Amanda.IO.MemoryOnly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Amanda.Data.Structured
{
    public class AmandaDb<TRecordType>
    {
        //folder structure on disk is as follows:
        //  .amanda - root folder
        //      .data - data files
        //      .keyFieldIndex - index based on key fields
        //      .timeBasedIndex - date based index
        private const string DEFAULT_DATA_FOLDER = ".data";

        protected FileSystemAccess<TRecordType> _fileSystemAccess;
        protected FileSystemAccess<TRecordType> _persistentAccess;

        protected Amanda.IO.IAmandaDirectory _workingFolder;
        protected Amanda.IO.IAmandaDirectory _persistFolder;

        public bool IsInitialized
        {
            get;
            protected set;
        }
        public AmandaDb()
        {
            this.IsInitialized = false;
        }

        /// <summary>
        /// This is different than other CreateOrUse* methods, because
        /// it takes the actual root folder, and not the parent.  This
        /// is to give the caller total control over the folder structure.
        /// </summary>
        /// <param name="rootFolder"></param>
        public void CreateOrUseAmandaDb(Amanda.IO.IAmandaDirectory rootFolder)
        {
            if(rootFolder ==null || !rootFolder.Exists)
                throw new IOException("The specified database parent directory is null or does not exist.");
            _workingFolder = new MemoryOnlyDirectory(DEFAULT_DATA_FOLDER);
            _fileSystemAccess = new FileSystemAccess<TRecordType>();
            _fileSystemAccess.CreateOrUseFileAccess(_workingFolder);
        }

        /// <summary>
        /// Pre load the cache.
        /// </summary>
        public void Synchronize(Amanda.IO.IAmandaDirectory rootFolder)
        {
            _persistFolder = rootFolder.CreateOrUseSubdirectory(DEFAULT_DATA_FOLDER);
            _persistentAccess = new FileSystemAccess<TRecordType>();
            _persistentAccess.CreateOrUseFileAccess(_workingFolder);            
        }

        public Dictionary<TRecordType, RowLocation> AddRecords(List<TRecordType> stocks)
        {
            using (LockFile lf = new LockFile("append_to_newest", _workingFolder))
            {
                return _fileSystemAccess.AppendToNewestFile(stocks);
            }
        }

        public List<TRecordType> GetRecordsAtLocation(RowLocation location)
        {
            return _fileSystemAccess.ReadAtLocation(location);
        }
    }
}
