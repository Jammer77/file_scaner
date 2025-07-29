using System.Collections.Concurrent;
using System.Diagnostics;


internal class Program
{
    private static async Task Main(string[] args)
    {
       
        var folderForScan = args.FirstOrDefault();

        if (string.IsNullOrEmpty(folderForScan))
        {
            PrintUsage();
            return;
        }

        Trace.Listeners.Add(new TextWriterTraceListener("errors.log"));
        Trace.AutoFlush = true;

        using var context = new FileScanDbContext();

        try
        {
            context.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Database Error: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }

        var loadedScanedFilesCollection = context.hashes.GroupBy(h => h.file_path)
                                                        .ToDictionary(g => g.Key,
                                                                      g => g.First());


        if (!Directory.Exists(folderForScan))
        {
            Trace.TraceError($"Folder {folderForScan} not exist");
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        string[] fileNameCollection;
        try
        {
            fileNameCollection
            = Directory.GetFiles(folderForScan, "*", SearchOption.AllDirectories);
            // If our utility has to work with folders that contain non-readable or protected
            // files, we need to rewrite this using recursion + try-catch. 
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Error during reading file info: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            return;
        }


        var result = new ConcurrentDictionary<string, FileScanInfo>();

        await Parallel.ForEachAsync(fileNameCollection, async (fileName, cancellationToken) =>
        {
            bool alreadyScanned = loadedScanedFilesCollection.TryGetValue(fileName, out FileScanInfo? dbScanInfo);
            FileScanInfo? fileScanInfo = null;

         
                // Here we could check for changes since the file was last added to the database,
                // such as differences in size or modification time.
                // It's not difficult to implement, but the requirements do not include these features,
                // and this is just a test project.
           

            if (!alreadyScanned)
            {
                fileScanInfo = await Task.Run(() => GetFileScanInfo(fileName), cancellationToken);
            }
            else
            {
                fileScanInfo = dbScanInfo;
                fileScanInfo.scanned++;
                fileScanInfo.last_seen = DateTime.UtcNow;
            }

            result.TryAdd(fileScanInfo.sha256, fileScanInfo);
        });

        Console.WriteLine($"{result.Count} files scanned - total time: {stopwatch}");


        foreach (var fileScanInfo in result.Values)
        {
            var existing = await context.hashes.FindAsync(fileScanInfo.sha256);

            if (null == existing)
                context.hashes.Add(fileScanInfo);
            else
                context.hashes.Update(fileScanInfo);
        }
        try
        {
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Database Error: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            return;
        }

        FileScanInfo GetFileScanInfo(string fileName)
        {
            using var fileStream = File.OpenRead(fileName);
            var fileInfo = new FileInfo(fileStream.Name);

            // Tried parallelism here, but there are many obstacles:
            // First — algorithms have different execution times.
            // Second — FileStream is not concurrency-safe.
            // I'm not sure it's a good idea to switch to MemoryStream,
            // because, for example, a customer with 8GB RAM
            //  might scan a Blu-ray video collection.

            string md5 = fileStream.CalculateMD5();
            string sha1 = fileStream.CalculateSHA1();
            string sha256 = fileStream.CalculateSHA256();
            long fileSize = fileInfo.Length;
            DateTime lastSeen = fileInfo.LastAccessTime;

            return new FileScanInfo(fileName, md5, sha1, sha256, fileSize, lastSeen, 1);

        }
    }

    private static void PrintUsage()
    {

        var welcome = "Unsuported system. Usage: ScanApp <foldername>";
#if WINDOWS
        welcome = "Usage: ScanApp.exe <foldername>";
#elif OSX
        welcome = "Usage: ScanApp <foldername>";
#endif
        Console.WriteLine(welcome);
    }
}
