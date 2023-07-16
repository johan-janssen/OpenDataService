using System.Collections;
using System.Collections.Generic;
using System.IO;
using OpenDataService.DataSources.Glob;

namespace OpenDataService.DataSources.IO;

public delegate IDataSource CreateDataSourceFunc(Stream stream);

class LazyLoadedFileDataSource : IDataSource
{
    private IDataSource? backingDataSource;
    OpenStreamFunc openStreamFunc;
    CreateDataSourceFunc createDataSourceFunc;

    public LazyLoadedFileDataSource(CreateDataSourceFunc createDataSourceFunc, OpenStreamFunc openStreamFunc)
    {
        this.openStreamFunc = openStreamFunc;
        this.createDataSourceFunc = createDataSourceFunc;
    }

    public IEntitySet GetEntitySet(string name)
    {
        CreateDataSourceIfNeeded();
        return backingDataSource!.GetEntitySet(name);
    }

    public IEnumerator<IEntitySet> GetEnumerator()
    {
        CreateDataSourceIfNeeded();
        return backingDataSource!.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        CreateDataSourceIfNeeded();
        return backingDataSource!.GetEnumerator();
    }

    private void CreateDataSourceIfNeeded()
    {
        if (backingDataSource == null)
        {
            using (var stream = openStreamFunc(FileAccess.Read))
            {
                backingDataSource = createDataSourceFunc(stream);
            }
        }
    }
}

public class DataSourceFactory
{
    public DataSourceFactory(string glob, CreateDataSourceFunc createDataSourceFunc)
    {
        Glob = glob;
        CreateDataSourceFunc = createDataSourceFunc;
    }

    public string Glob { get; }
    public CreateDataSourceFunc CreateDataSourceFunc { get; }
}

public class DirectoryDataSourceProvider : IDataSourceProvider
{
    private readonly IDirectoryWatcher watcher;
    private readonly IEnumerable<DataSourceFactory> factories;
    private readonly Func<Uri, string> uriToDataSourceNameFunc;

    public DirectoryDataSourceProvider(IDirectoryWatcher watcher, IEnumerable<DataSourceFactory> factories, Func<Uri, string>? uriToDataSourceNameFunc=null)
    {
        this.watcher = watcher;
        this.factories = factories;
        if (uriToDataSourceNameFunc == null)
        {
            this.uriToDataSourceNameFunc = GetDataSourceName;
        }
        else
        {
            this.uriToDataSourceNameFunc = uriToDataSourceNameFunc;
        }

        foreach (var fileInfo in watcher)
        {
            TryAdd(fileInfo);
        }
        
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
        watcher.Changed += OnChanged;
        watcher.Renamed += OnRenamed;
    }

    public IDictionary<string, IDataSource> DataSources { get; } = new Dictionary<string, IDataSource>();

    public string GetDataSourceName(Uri uri)
    {
        var filename = Path.GetFileName(uri.LocalPath);
        return Path.GetFileNameWithoutExtension(filename);
    }

    private void OnCreated(IDirectoryWatcher obj, FileInfo fileInfo)
    {
        TryAdd(fileInfo);
    }

    private void TryAdd(FileInfo fileInfo)
    {
        var factory = GetFactory(fileInfo);
        if (factory != null)
        {
            var name = uriToDataSourceNameFunc(fileInfo.Uri);
            DataSources.Add(name, new LazyLoadedFileDataSource(factory.CreateDataSourceFunc, fileInfo.Open));
        }
    }

    private void OnDeleted(IDirectoryWatcher obj, FileInfo fileInfo)
    {
        Delete(fileInfo.Uri);
    }

    private void OnChanged(IDirectoryWatcher obj, FileInfo fileInfo)
    {
        var factory = GetFactory(fileInfo);
        if (factory != null)
        {
            var name = uriToDataSourceNameFunc(fileInfo.Uri);
            DataSources[name] = new LazyLoadedFileDataSource(factory.CreateDataSourceFunc, fileInfo.Open);
        }
    }

    private void OnRenamed(IDirectoryWatcher obj, FileInfo fileInfo)
    {
        Delete(fileInfo.OldUri);
        TryAdd(fileInfo);
    }

    void Delete(Uri? uri)
    {
        if (uri == null)
        {
            return;
        }
        var name = uriToDataSourceNameFunc(uri);
        if (DataSources.ContainsKey(name))
        {
            DataSources.Remove(name);
        }
    }

    DataSourceFactory? GetFactory(FileInfo fileInfo)
    {
        var relativePath = watcher.Directory.MakeRelativeUri(fileInfo.Uri).ToString();
        var matchingFactories = factories.Where(factory => {
            var isMatch = new Glob.Glob(factory.Glob).IsMatch(relativePath);
            return isMatch;
        }).ToArray();
        
        if (matchingFactories.Length == 0)
        {
            return null;
        }
        if (matchingFactories.Length > 1)
        {
            throw new Exception("Multiple glob matches for file");
        }
        return matchingFactories[0];
    }
}