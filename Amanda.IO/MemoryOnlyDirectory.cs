using Amanda.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amanda.IO.MemoryOnly
{
    public class MemoryOnlyDirectory : IAmandaDirectory
    {
        private Dictionary<string, IAmandaFile> _files;
        private Dictionary<string, IAmandaDirectory> _folders;
        private string _path = String.Empty;
        public string Path
        {
            get
            {
                return _path;
            }
        }
        public MemoryOnlyDirectory(string path, bool exists = true)
        {
            _path = path;
            _files = new Dictionary<string, IAmandaFile>();
            _folders = new Dictionary<string, IAmandaDirectory>();
            Exists = exists;
        }
        public bool Exists
        {
            get;
            private set;
        }

        public void MergeFileIntoExisting(string source, string destination)
        {
            IAmandaFile sourceFile = _files[source];
            IAmandaFile destFile = Touch(destination);
            if (_files.ContainsKey(destination))
                destFile = _files[destination];
            else
                _files[destination] = destFile;

            using (var output = destFile.OpenWrite())
            {
                using(var input = sourceFile.OpenRead())
                {
                    output.Position = output.Length;
                    input.CopyTo(output);
                }
            }
            _files.Remove(source);
        }

        public void Create()
        {
            throw new NotImplementedException();
        }

        public IAmandaFile CreateNewFile(string relativeFileName)
        {
            var file = new MemoryOnlyFile(relativeFileName, true);
            _files.Add(relativeFileName, file);
            return file;
        }

        public IAmandaDirectory CreateOrUseSubdirectory(string relativePath)
        {
            if(!_folders.Any(f => f.Key == relativePath)) { 
                string fullPath = System.IO.Path.Combine(_path, relativePath);
                _folders[relativePath] = new MemoryOnlyDirectory(fullPath);
            }
            return _folders[relativePath];
        }

        public IAmandaFile GetFile(string relativeFileName)
        {
            if(_files.ContainsKey(relativeFileName))
            {
                return _files[relativeFileName];
            }
            else
            {
                return new MemoryOnlyFile(relativeFileName, false);
            }
        }

        public IAmandaFile GetNewestFile(string filter = "")
        {
            if(filter != String.Empty && filter.Contains("."))
            {
                filter = filter.Split(".".ToCharArray())[1];
            }

            if (_files != null && _files.Count > 0)
            {
                var fileCandidate = _files.OrderByDescending(f => f.Key).FirstOrDefault(f => f.Key.EndsWith(filter));
                if (fileCandidate.Key != null)
                    return fileCandidate.Value;
                else
                    return null;
            }
            else
                return null;
        }

        public IAmandaFile Touch(string relativeFileName)
        {
            if (!_files.ContainsKey(relativeFileName))
            {
                var file = new MemoryOnlyFile(relativeFileName, true);
                _files.Add(relativeFileName, file);
                return file;
            }
            else
            {
                return _files[relativeFileName];
            }
        }

        public bool IsEmpty()
        {
            return true;
        }

        public bool FileExists(string relativeFileName)
        {
            return _files.Any(f => f.Key == relativeFileName);
        }

        public void DeleteFile(string relativeFileName)
        {
            _files.Remove(relativeFileName);
        }
    }
}
