using System.IO;

namespace Amanda.IO
{
    public interface IAmandaFile
    {
        bool Exists { get; }
        string GetNameWithoutExtension();
        string GetRelativeFileName();
        bool IsLargerThan(double fileSizeInGb);
        Stream OpenRead();
        Stream OpenWrite();
        void Truncate();
        long Size { get; set; }
    }
}