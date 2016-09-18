using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Amanda.IO.Journal
{
    public class Ledger<TRecordType>
    {
        protected FileSystemAccess<JournalEntry<TRecordType>> _fileSystemAccess;
        protected Amanda.IO.IAmandaDirectory _workingFolder;
        private const string DEFAULT_DATA_FOLDER = ".journal";
        private bool _async = false;

        public Ledger(bool runAsync = false)
        {
            _async = runAsync;
        }

        public void CreateOrUseAmandaJournal(Amanda.IO.IAmandaDirectory rootFolder)
        {
            if (rootFolder == null || !rootFolder.Exists)
                throw new IOException("The specified database parent directory is null or does not exist.");
            _workingFolder = rootFolder.CreateOrUseSubdirectory(DEFAULT_DATA_FOLDER);
            _fileSystemAccess = new FileSystemAccess<JournalEntry<TRecordType>>();
            _fileSystemAccess.CreateOrUseFileAccess(_workingFolder);
        }

        public void Push(List<JournalEntry<TRecordType>> t)
        {
            Task task = new Task(() => WriteToFileSystem(t));
            if (!_async)
                task.RunSynchronously();
            else
                task.Start();
        }

        public void WriteToFileSystem(List<JournalEntry<TRecordType>> t)
        {
            using (LockFile lf = new LockFile("append_to_newest", _workingFolder))
            {
                _fileSystemAccess.AppendToNewestFile(t);
            }
        }
    }
}