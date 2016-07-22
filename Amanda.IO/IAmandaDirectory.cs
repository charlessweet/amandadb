namespace Amanda.IO
{
    public interface IAmandaDirectory
    {
        bool Exists { get; }
        string Path { get; }
        bool FileExists(string relativeFileName);
        void MergeFileIntoExisting(string source, string destination);
        void Create();
        IAmandaFile CreateNewFile(string relativeFileName);
        IAmandaDirectory CreateOrUseSubdirectory(string relativePath);
        IAmandaFile GetFile(string relativeFileName);
        void DeleteFile(string relativeFilePath);

        /// <summary>
        /// Get's the newest file according to the current implementation.
        /// </summary>
        /// <returns>The 'newest' file.</returns>
        IAmandaFile GetNewestFile(string filter = "*");
        bool IsEmpty();
        IAmandaFile Touch(string relativeFileName);
    }
}