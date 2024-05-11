namespace TorrentClient;

public class Torrent
{
    public Torrent(string torrentFilePath)
    {
        byte[] torrentMetaDataBytes = File.ReadAllBytes(torrentFilePath);
        TorrentMetaData torrentMetaData = new(torrentMetaDataBytes);
    }
}