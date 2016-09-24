using System;
using System.Collections.Generic;
using Amanda.IO;

namespace Amanda.Data.Structured
{
    public interface IAmandaIndex<TKeyField>
    {
        void AddReference(TKeyField key, RowLocation record, string indexName = null);
        void CreateOrUseIndex(IAmandaDirectory parentRootFolder, string fileSystemFriendlyIndexName = ".sfsi");
        List<RowLocation> FindRecord(TKeyField key);
        List<RowLocation> FindRecordsBetween(TKeyField startOfRange, TKeyField endOfRange);
    }
}