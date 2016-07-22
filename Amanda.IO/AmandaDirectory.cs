using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amanda.IO
{
    public class AmandaDirectory : IAmandaDirectory
    {
        public const int MAX_FILE_ACCESS_ATTEMPTS = 200;
        protected DirectoryInfo _directoryInfo; 
        public AmandaDirectory(string directory)
        {
            try
            {
                _directoryInfo = new DirectoryInfo(directory);
            }catch(ArgumentException ex)
            {
                throw new ArgumentException("The specified directory was null or whitespace.", ex);
            }
        }

        public bool Exists
        {
            get
            {
                _directoryInfo.Refresh();
                return _directoryInfo.Exists;
            }
        }

        public string Path
        {
            get
            {
                return _directoryInfo.FullName;
            }
        }

        public void Create()
        {
            _directoryInfo.Create();
        }

        public bool IsEmpty()
        {
            return (_directoryInfo.Parent.GetDirectories().Length == 1 && _directoryInfo.Parent.GetFiles().Length == 0);
        }

        public IAmandaDirectory CreateOrUseSubdirectory(string relativePath)
        {
            string subPath = System.IO.Path.Combine(this.Path, relativePath);
            Amanda.IO.AmandaDirectory subDirectory = new AmandaDirectory(subPath);
            if (!subDirectory.Exists)
            {
                subDirectory.Create();
                if (!subDirectory.Exists)
                    throw new IOException("Could not initialize new Amanda repository.");
            }
            return subDirectory;
        }

        private string CreateFullyQualifiedFileNameFromRelative(string relativeFileName)
        {
            return System.IO.Path.Combine(_directoryInfo.FullName, relativeFileName);
        }

        public IAmandaFile GetNewestFile(string filter = "*")
        {
            var files = _directoryInfo.GetFiles(filter);
            if (files.Count() == 0)
                return null;
            return new AmandaFile(files.OrderByDescending(f => f.Name).Last().FullName);
        }

        public IAmandaFile CreateNewFile(string relativeFileName)
        {
            string newName = CreateFullyQualifiedFileNameFromRelative(relativeFileName);
            return new AmandaFile(newName);
        }

        public void MergeFileIntoExisting(string source, string destination)
        {
            var sourceFile = GetFile(source);
            var destFile = GetFile(destination);
            using (var input = sourceFile.OpenRead())
            {
                using (var output = destFile.OpenWrite())
                {
                    output.Position = output.Length;
                    input.CopyTo(output);
                }
            }
            DeleteFile(source);
        }

        public IAmandaFile GetFile(string relativeFileName)
        {
            string newName = CreateFullyQualifiedFileNameFromRelative(relativeFileName);
            return new AmandaFile(newName);
        }

        public IAmandaFile Touch(string relativeFileName)
        {
            string fileName = CreateFullyQualifiedFileNameFromRelative(relativeFileName);
            if (!File.Exists(fileName))
            {
                int loop = 0;
                do
                {
                    loop++;
                    try
                    {
                        System.IO.File.WriteAllText(fileName, string.Empty);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Task.Delay(10).Wait();
                        if (loop == MAX_FILE_ACCESS_ATTEMPTS)
                            throw new MaximumFileAccessAttemptsExceededException("Failed to touch " + fileName + " after " + loop + " tries.", ex);
                    }
                } while (true);
            }
            return GetFile(relativeFileName);
        }

        public bool FileExists(string relativeFileName)
        {
            string fileName = System.IO.Path.Combine(Path, relativeFileName);
            return File.Exists(fileName);
        }

        public void DeleteFile(string relativeFileName)
        {
            string fileName = System.IO.Path.Combine(Path, relativeFileName);
            int loop = 0;
            do
            {
                loop++;
                try
                {
                    File.Delete(fileName);
                    return;
                }
                catch (FileNotFoundException)
                {
                    //don't care - we were deleting it anyway
                }
                catch (IOException ex)
                {
                    Task.Delay(10).Wait();
                    if (loop == MAX_FILE_ACCESS_ATTEMPTS)
                        throw new MaximumFileAccessAttemptsExceededException("Failed to delete " + fileName + " after " + loop + " tries.", ex);
                }
            } while (true);
        }
    }
}
