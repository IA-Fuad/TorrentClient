using TorrentClient;

string torrentFilePath = "F:\\Projects\\TorrentClient\\ubuntu-24.04-desktop-amd64.iso.torrent";

byte[] torrentMetaDataBytes = File.ReadAllBytes(torrentFilePath);
TorrentMetaData torrentMetaData = new(torrentMetaDataBytes);
Tracker tracker = new(torrentMetaData.Announce);

TrackerRequestParameters trackerRequestParameters = new(torrentMetaData.BEncodedInfoBytes);

Torrent torrent = new(torrentMetaData, tracker, trackerRequestParameters);
await torrent.StartDownload();
