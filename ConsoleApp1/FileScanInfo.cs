public  class FileScanInfo
{
    public string file_path { get; }
    public string md5 { get;}
    public string sha1 { get;}
    public string sha256 { get;}
    public long file_size { get; }
    public DateTime last_seen { get; set;  }
    public int scanned  { get; set;  }


    public FileScanInfo(string file_path, string md5, string sha1, string sha256, long file_size, DateTime last_seen, int scanned)
    {
        this.file_path = file_path;
        this.md5 = md5;
        this.sha1 = sha1;
        this.sha256 = sha256;
        this.file_size = file_size;
        this.last_seen = last_seen;
        this.scanned = scanned;
    }
}