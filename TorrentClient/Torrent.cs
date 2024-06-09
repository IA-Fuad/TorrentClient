using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TorrentClient;

public class Torrent
{
    private readonly TorrentMetaData _torrentMetaData;
    private readonly Tracker _tracker;
    private readonly TrackerRequestParameters _trackerRequestParameters;

    private readonly TcpListener _server;

    public Torrent(TorrentMetaData torrentMetaData, Tracker tracker, TrackerRequestParameters trackerRequestParameters)
    {
        _torrentMetaData = torrentMetaData;
        _tracker = tracker;
        _trackerRequestParameters = trackerRequestParameters;

        IPAddress localAddress = IPAddress.Parse("127.0.0.1");
        _server = new(localAddress, trackerRequestParameters.Port);
    }
    
    public async Task StartDownload()
    {
        _trackerRequestParameters.Left = _torrentMetaData.Info.Files.First().Length;

        Task.Run(StartListeningToPeers);

        List<Peer> peers = await _tracker.GetPeers(_trackerRequestParameters);

        peers[0].InitiateHandShake(_trackerRequestParameters.InfoHashBytes, _trackerRequestParameters.PeerIdBytes);
    }

    private void StartListeningToPeers()
    {
        _server.Start();
        byte[] bytes = new byte[256];

        while (true)
        {
            using TcpClient client = _server.AcceptTcpClient();

            NetworkStream stream = client.GetStream();

            int i;
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                var data = Encoding.UTF8.GetString(bytes, 0, i);
                Console.WriteLine(data);
            }
        }
    }
}