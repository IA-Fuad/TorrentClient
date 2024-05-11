namespace TorrentClient;

public class FileInfo
{
    public string Name { get; set; }
    public int? Length { get; set; }
    public List<string> Path { get; set; }
}