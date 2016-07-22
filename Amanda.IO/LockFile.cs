using Amanda.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amanda.IO
{
    public class LockFile:IDisposable
    {
        private static object _metaLock = new object();
        private static Guid UniqueId { get; set; }
        private string _lockName = String.Empty;
        private IAmandaDirectory _directory = null;

        public LockFile(string lockName, IAmandaDirectory directory)
        {
            UniqueId = Guid.NewGuid();
            _lockName = lockName;
            _directory = directory;
            LockFile.AcquireLock(_lockName, _directory);
        }

        public static void UnLock(string lockName, IAmandaDirectory directory)
        {
            directory.DeleteFile(lockName);
        }

        public static void AcquireLock(string lockName, IAmandaDirectory directory)
        {
            string lockKey = "Existing Lock";
            int tries = 0;

            Stopwatch swatch = new Stopwatch();
            swatch.Start();
            bool exists = directory.FileExists(lockName);
            swatch.Stop();
            while (exists && tries < 100)
            {
                var task = Task.Delay(20);
                task.Wait();
                tries++;
                Trace.WriteLine("Waiting For Lock: " + lockKey + "(" + lockName + ")");
                exists = File.Exists(lockName);
            }
            if (tries >= 100 && exists)
            {
                //Trace.WriteLine("Lock Never Released: " + lockKey + "(" + lockName + ")");
                throw new InvalidOperationException("Tried " + tries + " times to acquire lock at " + lockName + " but wasn't available.");
            }
            lock (_metaLock)
            {
                var ifile = directory.CreateNewFile(lockName);
                using (var fs = ifile.OpenWrite())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(UniqueId.ToString());

                    ASCIIEncoding ascii = new ASCIIEncoding();
                    var bytes = ascii.GetBytes(sb.ToString());
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                }
                //Trace.WriteLine("Obtained Lock For: " + this.GetHashCode());
            }
        }

        public void Dispose()
        {
            if (!String.IsNullOrWhiteSpace(_lockName) && _directory != null)
            {
                LockFile.UnLock(_lockName, _directory);
            }
        }
    }
}
