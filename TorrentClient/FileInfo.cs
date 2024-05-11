namespace TorrentClient;

public class FileInfo
{
    public int Length { get; private set; }
    public string? Md5Sum { get; private set; }
    public List<string>? Path { get; private set; }

    public FileInfo(int length, string? md5sum = null)
    {
        Length = length;
        Md5Sum = md5sum;
    }

    public FileInfo(int length, List<string> path, string? md5sum = null)
    {
        Length = length;
        Path = path;
        Md5Sum = md5sum;
    }
}