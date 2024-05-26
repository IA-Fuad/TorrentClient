namespace TorrentClient;

public class TorrentMetaData
{
    public TorrentMetaDataInfo Info { get; private set; }
    public byte[] BEncodedInfoBytes { get; private set; }
    public string Announce { get; private set; }
    public long? CreationDate { get; private set; }
    public string? Comment { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? Encoding { get; private set; }

    public TorrentMetaData(byte[] torrentMetaData)
    {
        ParseTorrentMetaData(torrentMetaData);
    }

    private void ParseTorrentMetaData(byte[] torrentMetaData)
    {
        var decodedMetaData = BEncoding.BEncoding.DecodeBEncodedBytes(torrentMetaData);

        if (decodedMetaData.Type != BEncoding.BDecodedObjectType.Dictionary) 
        {
            throw new ArgumentException("torrent meta data is not a dictionary.");
        }

        decodedMetaData.TryParseDictionary(out var metaDataDict);

        if (!metaDataDict.TryGetValue("announce", out var announce)
            || !announce.TryParseString(out string announceUrl))
        {
            throw new ArgumentException("torrent meta data does not contain valid announce value");
        }
        if (!metaDataDict.TryGetValue("info", out var info))
        {
            throw new ArgumentException("torrent meta data does not contain valid info value");
        }

        Announce = announceUrl;
        Info = new(info);
        BEncodedInfoBytes = BEncoding.BEncoding.GetBEncodedBytes(info);
        Console.WriteLine(BEncoding.BEncoding.DecodeBEncodedBytes(BEncodedInfoBytes));
    }
}