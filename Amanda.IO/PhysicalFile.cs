using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amanda.IO
{
    public class PhysicalFile : IAmandaFile
    {
        protected const long BYTES_PER_GIGABYTE = 1073741824;
        protected const int MAX_FILE_ACCESS_TRIES = 200;
        string FilePath { get; set; }
        public bool Exists { get { return File.Exists(FilePath); } }

        public long Size
        {
            get
            {
                var fileInfo = new FileInfo(FilePath);
                return fileInfo.Length;
            }
        }

        public PhysicalFile(string filePath)
        {
            FilePath = filePath;
        }

        public bool IsLargerThan(double fileSizeInGb)
        {
            int loop = 0;
            do
            {
                loop++;
                try
                {
                    var fileInfo = new FileInfo(FilePath);
                    return (fileInfo.Length > (long)fileSizeInGb * BYTES_PER_GIGABYTE);
                }
                catch (Exception ex)
                {
                    Task.Delay(10).Wait();
                    if (loop == MAX_FILE_ACCESS_TRIES)
                        throw new MaximumFileAccessAttemptsExceededException("Failed to open " + FilePath + " for writing after " + loop + " attempts.", ex);
                }
            } while (true); //either we'll reach MAXxxx or we'll open file
        }

        public string GetNameWithoutExtension()
        {
            return Path.GetFileNameWithoutExtension(FilePath);
        }

        public string GetRelativeFileName()
        {
            return Path.GetFileName(FilePath);
        }

        public int GetNextWriteLocation()
        {
            return File.ReadLines(FilePath).Count() + 1;
        }

        public Stream OpenWrite()
        {
            int loop = 0;
            do
            {
                loop++;
                try
                {
                    return System.IO.File.OpenWrite(FilePath);
                }
                catch (Exception ex)
                {
                    Task.Delay(10).Wait();
                    if (loop == MAX_FILE_ACCESS_TRIES)
                        throw new MaximumFileAccessAttemptsExceededException("Failed to open " + FilePath + " for writing after " + loop + " attempts.", ex);
                }
            } while (true); //either we'll reach MAXxxx or we'll open file
        }

        public Stream OpenRead()
        {
            int loop = 0;
            do
            {
                loop++;
                try
                {
                    return System.IO.File.OpenRead(FilePath);
                }
                catch (Exception ex)
                {
                    Task.Delay(10).Wait();
                    if (loop == MAX_FILE_ACCESS_TRIES)
                        throw new MaximumFileAccessAttemptsExceededException("Failed to open  " + FilePath + "  for reading after " + loop + " attempts.", ex);
                }
            } while (true); //either we'll reach MAXxxx or we'll open file
        }

        public void Truncate()
        {
            File.Delete(FilePath);
        }
    }
}
