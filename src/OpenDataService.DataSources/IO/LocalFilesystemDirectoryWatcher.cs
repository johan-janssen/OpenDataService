using System.Collections;
using System.IO;

namespace OpenDataService.DataSources.IO;

public class LocalFilesystemDirectoryWatcher : IDirectoryWatcher
{
    public event FileEventHandler? Changed;
    public event FileEventHandler? Created;
    public event FileEventHandler? Deleted;
    public event FileEventHandler? Renamed;
    private FileSystemWatcher fileSystemWatcher;
    private readonly string directory;

    public LocalFilesystemDirectoryWatcher(string directory)
    {
        directory = Path.GetFullPath(directory);
        Directory = new Uri(directory);
        fileSystemWatcher = new FileSystemWatcher(directory)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
        };

        fileSystemWatcher.Changed += (source, e) => Changed?.Invoke(this, ToFileInfo(e.FullPath));
        fileSystemWatcher.Created += (source, e) => Created?.Invoke(this, ToFileInfo(e.FullPath));
        fileSystemWatcher.Deleted += (source, e) => Deleted?.Invoke(this, ToFileInfo(e.FullPath));
        fileSystemWatcher.Renamed += (source, e) => Renamed?.Invoke(this, ToFileInfo(e.FullPath, e.OldFullPath));

        fileSystemWatcher.EnableRaisingEvents = true;
        this.directory = directory;
    }

    public Uri Directory { get; }

    Stream WaitForFileAvailable(string path, FileMode fileMode, FileAccess fileAccess)
    {
        const int maxTries = 10;
        int tries = 0;
        while (true)
        {
            try
            {
                FileStream fs = new FileStream(path, fileMode, fileAccess, FileShare.None);
                return fs;
            }
            catch (IOException)
            {
                tries++;
            }
            if (tries > maxTries)
            {
                throw new IOException();
            }
            Thread.Sleep(50);
        }
    }

    public IEnumerator<FileInfo> GetEnumerator()
    {
        return System.IO.Directory.GetFiles(directory).Select(f => ToFileInfo(f)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    FileInfo ToFileInfo(string filePath, string? oldPath=null)
    {
        OpenStreamFunc openStreamFunc = (FileAccess fileAccess) => WaitForFileAvailable(filePath, FileMode.Open, fileAccess);
        return new FileInfo(new Uri(filePath), oldPath == null ? null : new Uri(oldPath), openStreamFunc);
    }
}