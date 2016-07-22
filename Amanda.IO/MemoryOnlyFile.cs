using Amanda.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Amanda.IO.MemoryOnly
{
    public class MemoryOnlyFile : IAmandaFile
    {
        private string _relativeFileName;
        private MemoryOnlyStream _fileMemory;

        public MemoryOnlyFile(string relativeFileName, bool exists = false)
        {
            _relativeFileName = relativeFileName;
            _fileMemory = new MemoryOnlyStream();
            this.Exists = exists;
        }

        public bool Exists
        {
            get;
            protected set;
        }

        public string GetNameWithoutExtension()
        {
            return Path.GetFileNameWithoutExtension(_relativeFileName);
        }

        public string GetRelativeFileName()
        {
            return _relativeFileName;
        }

        public bool IsLargerThan(double fileSizeInGb)
        {
            return _fileMemory.Length > (fileSizeInGb * 1073741824);
        }

        public Stream OpenRead()
        {
            return _fileMemory;
        }

        public Stream OpenWrite()
        {
            return _fileMemory;
        }

        public void Truncate()
        {
            _fileMemory.SetLength(0);
        }
    }
}
