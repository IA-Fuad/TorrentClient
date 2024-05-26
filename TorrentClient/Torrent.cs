namespace TorrentClient;

public class Torrent
{
    private readonly TorrentMetaData _torrentMetaData;
    private readonly Tracker _tracker;
    private readonly TrackerRequestParameters _trackerRequestParameters;

    public Torrent(TorrentMetaData torrentMetaData, Tracker tracker, TrackerRequestParameters trackerRequestParameters)
    {
        _torrentMetaData = torrentMetaData;
        _tracker = tracker;
        _trackerRequestParameters = trackerRequestParameters;
    }
    
    public async Task StartDownload()
    {
        _trackerRequestParameters.Left = _torrentMetaData.Info.Files.First().Length;
        List<Peer> peers = await _tracker.GetPeers(_trackerRequestParameters);
    }
}