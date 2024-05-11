namespace TorrentClient;

public class Peer 
{
    public string PeerId { get; set; }
    public string Ip { get; set; }
    public int Port { get; set; }

    public Peer(string peerId, string ip, int port)
    {
        PeerId = peerId;
        Ip = ip;
        Port = port;
    }


}