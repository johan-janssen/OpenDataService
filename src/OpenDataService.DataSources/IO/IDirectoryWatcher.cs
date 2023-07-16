using System.IO;
using System.Collections.Generic;

namespace OpenDataService.DataSources.IO;

public delegate Stream OpenStreamFunc(FileAccess fileAccess);
public delegate void FileEventHandler(IDirectoryWatcher obj, FileInfo fileInfo);

public class FileInfo
{
    public FileInfo(Uri uri, Uri? oldUri, OpenStreamFunc openStreamFunc)
    {
        Uri = uri;
        OldUri = oldUri;
        Open = openStreamFunc;
    }

    public Uri Uri { get; }
    public Uri? OldUri { get; }
    public OpenStreamFunc Open { get; }
}
public interface IDirectoryIterator : IEnumerable<FileInfo>
{

}
public interface IDirectoryWatcher : IDirectoryIterator
{
    event FileEventHandler? Changed;
    event FileEventHandler? Created;
    event FileEventHandler? Deleted;
    event FileEventHandler? Renamed;

    Uri Directory { get; }
}