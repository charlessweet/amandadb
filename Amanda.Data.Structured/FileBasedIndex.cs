using Amanda.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Linq;
namespace Amanda.Data.Structured
{
    /// <summary>
    /// Creates and traverses indexes using hashtables.
    /// </summary>
    /// <remarks>
    /// This indexing system creates an index in 2 phases.  First, 
    /// creates the hashtable in memory.  Second, add index using the
    /// 'AddReference' method.  Finally, commit or rollback all index
    /// entries made since last Commit (or load).
    /// </remarks>
    public class FileBasedIndex<TKeyField> : IAmandaIndex<TKeyField>
    {
        protected const string DEFAULT_INDEX_NAME = ".ffsi";
        protected IAmandaDirectory _workingFolder;
        protected string CurrentIndexName { get; set; }

        /// <summary>
        /// Creates a new index, and sets the current index (for all subsequent
        /// operations using this object) to the newly created index.
        /// </summary>
        /// <remarks>
        /// If the index does not exist, one is created.  If it does exist, then
        /// the existing index is used.  Be careful if you're using more than one
        /// index to not accidentally rewrite to the wrong index.
        /// </remarks>
        /// <returns>
        /// The name of the newly created index.  You can use this in additional
        /// operations to be more cautious.  By default, the new index is used
        /// anyway. If you supply the index name, then an exception will be thrown
        /// in all subsequent operations if the index you're using doesn't match
        /// the known index.  This is a precaution for your convenience.
        /// </returns>
        /// <param name="fileSystemFriendlyIndexName">
        /// This is used as a prefix to distinguish the index from others in the file system.  All rules
        /// pertaining to your operating system naming conventions should be respected when considering
        /// an index name.
        /// </param>
        public void CreateOrUseIndex(Amanda.IO.IAmandaDirectory parentRootFolder, string fileSystemFriendlyIndexName = DEFAULT_INDEX_NAME)
        {
            if(parentRootFolder == null || !parentRootFolder.Exists)
                throw new IOException("The specified index parent directory is null or does not exist.");

            _workingFolder = parentRootFolder.CreateOrUseSubdirectory(fileSystemFriendlyIndexName);
        }
        
        public string GetFileName(TKeyField key)
        {
            string origFileName = key.ToString();
            var newName = String.Join("_", origFileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            return newName;//we'll need to sanitize these and maintain order
        }

        public List<RowLocation> FindRecord(TKeyField key)
        {
            string fileName = GetFileName(key);
            return ReadRecordsInFile(fileName);
        }

        private List<RowLocation> ReadRecordsInFile(string fileName)
        {
            var rowLocations = new List<RowLocation>();
            if (_workingFolder.FileExists(fileName))
            {
                //index folder initialization
                FileSystemAccess<RowLocation> chain = new FileSystemAccess<RowLocation>();
                chain.CreateOrUseFileAccess(_workingFolder);
                rowLocations.AddRange(chain.GetAllRecordsInFile(fileName));
            }
            return rowLocations;
        }

        /// <summary>
        /// Add the item to the index.  The default way this works is adds to a hash table,then we serialize this hash table
        /// to disk.  We may get more fancy in the future, but this works for now.
        /// </summary>
        public void AddReference(TKeyField key, RowLocation record, string indexName = null)
        {
            if (!String.IsNullOrWhiteSpace(indexName) && this.CurrentIndexName != indexName)
                throw new InvalidOperationException("The index specified for update " + indexName + " is not the current index " + this.CurrentIndexName + ".");

            IAmandaFile file = null;
            string fileName = GetFileName(key);
            List<RowLocation> rows = new List<RowLocation>();
            rows.Add(record);
            FileSystemAccess<RowLocation> chain = new FileSystemAccess<RowLocation>();
            if (!_workingFolder.FileExists(fileName))
                file = _workingFolder.Touch(fileName);
            else
                file = _workingFolder.GetFile(fileName);
            chain.AppendRecordsToFile(rows, file);
        }

        public List<RowLocation> FindRecordsBetween(TKeyField lowerBounds, TKeyField upperBounds)
        {
            IList<string> fileNamesFound = new List<string>();
            string lowRange = GetFileName(lowerBounds);
            string hiRange = GetFileName(upperBounds);
            fileNamesFound = _workingFolder.GetMatchingFiles((ai)=>
            {
                if(ai.GetRelativeFileName().CompareTo(lowRange) >= 0 && 
                ai.GetRelativeFileName().CompareTo(hiRange) <= 0)
                {
                    return true;
                }
                return false;
            }).Select(f => f.GetRelativeFileName())
                .ToList();

            List<RowLocation> rows = new List<RowLocation>();
            foreach (var fileName in fileNamesFound)
            {
                rows.AddRange(ReadRecordsInFile(fileName));
            }
            return rows;
        }
    }
}