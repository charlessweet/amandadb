# amandadb
C# local database dreaming of big things.  Seriously, though, this database has comprehensive test coverage, and currently will serialize classes to disk using reflection with default indexing.  It was created to log stock transactions and do some data mining, but I've since discovered value investing, and am no longer interested in machine learning stock ticker values.  Hence, I have this database and nothing to do with it - so I thought I would make it available for open source.

Update (9/17/2016):
Moved all data access to in-memory only, thus reducing the online write and read times significantly.  From ~7ms read <1ms read, and from ~0.03ms to <0.005ms write.  This creates more work.  Now I need to work out a journaling and synchronization mechanism to persist the data and harden against things like power outages, but I've already got the necessary interfaces to interact with the file system to make this happen.  Also, I'll have to figure out what to load into memory on startup (maybe just the indexes?).

About AmandaDB:
<ol>
<li>On my machine (Lenovo G50 Core i7 2.4GHz, 16GB RAM, 64-bit, Windows 10), performance is 0.005ms and 0.753ms write time and read time respectively, per 256 byte record.  I'm really interested in getting up to 8k reading in < 5ms.  Right now, I'm at something asonine like 30ms for reads, so don't be confused - it'll be a while.</li>
<li>Works with the underlying file system.  This doesn't control disk access, like some DBMS's.  Storage maps to files.</li>
<li>I'd love to have it more configurable.</li>
</ol>
