# amandadb
C# local database dreaming of big things.  Seriously, though, this database has comprehensive test coverage, and currently will serialize classes to disk using reflection with default indexing.  It was created to log stock transactions and do some data mining, but I've since discovered value investing, and am no longer interested in machine learning stock ticker values.  Hence, I have this database and nothing to do with it - so I thought I would make it available for open source.

About AmandaDB:
1. On my machine (Lenovo G50 Core i7 2.4GHz, 16GB RAM, 64-bit, Windows 10), write time is 0.0394ms and 7.0822ms write time and read time respectively, per 256 byte record.  I'm really interested in getting up to 8k reading in < 5ms.  Right now, I'm at something asonine like 30ms for reads, so don't be confused - it'll be a while.
2. Works with the underlying file system.  This doesn't control disk access, like some DBMS's.  Storage maps to files.
