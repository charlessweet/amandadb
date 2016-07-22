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
    public class AmandaIndex<TKeyField>
    {
        protected const string DEFAULT_INDEX_NAME = ".sfsi";
        protected IAmandaDirectory _workingFolder;
        FileSystemAccess<SortedDictionary<TKeyField, List<RowLocation>>> _indexFileSystem;
        protected string CurrentIndexName { get; set; }
        protected SortedDictionary<TKeyField, List<RowLocation>> CurrentIndex { get; set; }

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

            //index folder initialization
            _indexFileSystem = new FileSystemAccess<SortedDictionary<TKeyField, List<RowLocation>>>();
            _indexFileSystem.CreateOrUseFileAccess(_workingFolder);

            var rec = (SortedDictionary<TKeyField, List<RowLocation>>)_indexFileSystem.GetFirstRecord();
            if(rec != null)
            {
                CurrentIndex = rec;
            }
            else
            {
                CurrentIndex = new SortedDictionary<TKeyField, List<RowLocation>>();
            }
        }

        public List<RowLocation> FindRecord(TKeyField key)
        {
            var rowLocations = new List<RowLocation>();
            if (CurrentIndex.ContainsKey(key))
            {
                rowLocations = CurrentIndex[key];
            }
            return rowLocations;
        }

        public List<RowLocation> FindRecordsBetween(TKeyField lowerBounds, TKeyField upperBounds)
        {
            List<TKeyField> keysFound = new List<TKeyField>();
            var comparer = Comparer<TKeyField>.Default;
            keysFound = CurrentIndex.Keys.Where(k => comparer.Compare(k, lowerBounds) >= 0 &&
                comparer.Compare(k, upperBounds) <= 0).ToList();

            List<RowLocation> rows = new List<RowLocation>();
            foreach(var key in keysFound)
            {
                rows.AddRange(CurrentIndex[key]);
            }
            return rows;
        }

        /// <summary>
        /// Add the item to the index.  The default way this works is adds to a hash table,then we serialize this hash table
        /// to disk.  We may get more fancy in the future, but this works for now.
        /// </summary>
        public void AddReference(TKeyField key, RowLocation record, string indexName = null)
        {
            if (!String.IsNullOrWhiteSpace(indexName) && this.CurrentIndexName != indexName)
                throw new InvalidOperationException("The index specified for update " + indexName + " is not the current index " + this.CurrentIndexName + ".");
            if (!CurrentIndex.ContainsKey(key))
            {
                List<RowLocation> rows = new List<RowLocation>();
                rows.Add(record);
                CurrentIndex.Add(key, rows);
            }
            else
            {
                var rows = CurrentIndex[key];
                rows.Add(record);
            }
        }
    
        public void Commit(string indexName = null)
        {
            if (!String.IsNullOrWhiteSpace(indexName) && this.CurrentIndexName != indexName)
                throw new InvalidOperationException("The index specified for update " + indexName + " is not the current index " + this.CurrentIndexName + ".");
            //for the current index, find a file
            _indexFileSystem.TruncateAndOverwriteCurrentFile(this.CurrentIndex);
        }

        public void Rollback(string indexName = null)
        {
            if (!String.IsNullOrWhiteSpace(indexName) && this.CurrentIndexName != indexName)
                throw new InvalidOperationException("The index specified for update " + indexName + " is not the current index " + this.CurrentIndexName + ".");
        }
    }
}