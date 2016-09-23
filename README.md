# amandadb
C# local database dreaming of big things.  Seriously, though, this database has comprehensive test coverage, and currently will serialize classes to disk using reflection with default indexing.  It was created to log stock transactions and do some data mining, but I've since discovered value investing, and am no longer interested in machine learning stock ticker values.  Hence, I have this database and nothing to do with it - so I thought I would make it available for open source.

Update (9/17/2016):
Moved all data access to in-memory only, thus reducing the online write and read times significantly.  From ~7ms read <1ms read, and from ~0.03ms to <0.005ms write.  This creates more work.  Now I need to work out a journaling and synchronization mechanism to persist the data and harden against things like power outages, but I've already got the necessary interfaces to interact with the file system to make this happen.  Also, I'll have to figure out what to load into memory on startup (maybe just the indexes?).

The times are still with pretty small records.  The 8k records are taking a while to load (15.24ms) because they're JSON and moving to the correct positions in the files are taking a while.  Standardizing serialized record sizes will allow me to pre-calculate the position of the data in the file, and move to the position in the stream without deserializing records.

Update (9/22/2016)
Changed record storage format to binary (got rid of JSON storage).  My goal was to have uniform record sizes so I can identify locations by math instead of seeking.  This has allowed me to reduce the access times even with 8k records down to 0ms (smaller than my timers can count).  Now, 8k records are loading in 0ms, basically all writes and reads are 0ms.

About AmandaDB:
<ol>
<li>On my machine (Lenovo G50 Core i7 2.4GHz, 16GB RAM, 64-bit, Windows 10), performance is 0ms and 0ms write time and read time respectively, per 256 byte record.  Down to 0ms writes and reads on all records.</li>
<li>Works with the underlying file system.  This doesn't control disk access, like some DBMS's.  Storage maps to files.</li>
<li>I'd love to have it more configurable.</li>
<li>Works with .NET Framework - anywhere this is installed, this will work.</li>
</ol>
