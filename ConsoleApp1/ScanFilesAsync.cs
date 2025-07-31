internal partial class Program
{

    public static async IAsyncEnumerable<string> ScanFilesAsync(string rootFolder)
    {
        var folders = new ConcurrentQueue<string>();
        folders.Enqueue(rootFolder);

        int bufferSize = Environment.ProcessorCount * 10;
        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(bufferSize)
        {
            SingleReader = false,
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        var workers = new List<Task>();

        int workerCount = Math.Max(1, Environment.ProcessorCount / 2);

        for (int i = 0; i < workerCount; i++)
        {
            workers.Add(Task.Run(async () =>
            {
                while (folders.TryDequeue(out string? currentFolder))
                {
                    try
                    {
                        foreach (string file in Directory.EnumerateFiles(currentFolder))
                        {
                            var fileInfo = new FileInfo(file);
                            bool isHidden = (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                            if (!isHidden)
                                await channel.Writer.WriteAsync(file);
                        }

                        foreach (string dir in Directory.EnumerateDirectories(currentFolder))
                        {
                            var fileInfo = new DirectoryInfo(dir);
                            bool isHidden = (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                            if (!isHidden)
                                folders.Enqueue(dir);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"Error in folder '{currentFolder}': {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }));
        }

        _ = Task.WhenAll(workers).ContinueWith(_ => channel.Writer.Complete());

        await foreach (var file in channel.Reader.ReadAllAsync())
        {
            yield return file;
        }
    }
}