using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amanda.IO.Journal
{
    public class Ledger<TRecordType>
    {
        protected FileSystemAccess<TRecordType> _fileSystemAccess;
        protected Amanda.IO.IAmandaDirectory addJournalFolder;
        private const string DEFAULT_DATA_FOLDER = ".add_journal";
        private bool _async = false;

        public Ledger(bool runAsync = false)
        {
            _async = runAsync;
        }

        public void CreateOrUseJournal(Amanda.IO.IAmandaDirectory rootFolder)
        {
            if (rootFolder == null || !rootFolder.Exists)
                throw new IOException("The specified database parent directory is null or does not exist.");
            addJournalFolder = rootFolder.CreateOrUseSubdirectory(DEFAULT_DATA_FOLDER);
            _fileSystemAccess = new FileSystemAccess<TRecordType>();
            _fileSystemAccess.CreateOrUseFileAccess(addJournalFolder);
        }

        public void Enque(List<TRecordType> t)
        {
            if (!_async)
                WriteToFileSystem(t);
            else
            {
                Task task = new Task(() => WriteToFileSystem(t));
                task.Start();
            }
        }

        public void WriteToFileSystem(List<TRecordType> t)
        {
            using (LockFile lf = new LockFile("append_to_newest_journal", addJournalFolder))
            {
                _fileSystemAccess.AppendToNewestFile(t);
            }
        }
    }
}