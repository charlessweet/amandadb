using Amanda.IO;
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
        private const string DEFAULT_DATA_FOLDER = ".data";
        protected FileSystemAccess<TRecordType> _fileSystemAccess;
        protected Amanda.IO.IAmandaDirectory _workingFolder;
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
            _workingFolder = rootFolder.CreateOrUseSubdirectory(DEFAULT_DATA_FOLDER);

            _fileSystemAccess = new FileSystemAccess<TRecordType>();
            _fileSystemAccess.CreateOrUseFileAccess(_workingFolder);
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
