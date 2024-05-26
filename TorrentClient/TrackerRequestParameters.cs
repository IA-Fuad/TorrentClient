using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace TorrentClient;

public enum TrackerEvent
{
    Started,
    Stopped,
    Completed
}

public class TrackerRequestParameters
{
    public string Info_Hash { get; private set; }
    public string Peer_Id { get; private set; }
    public int Port { get; private set; }
    public long Uploaded { get; set; }
    public long Downloaded { get; set; }
    public long Left { get; set; }
    public int? Compact { get; set; }
    public TrackerEvent Event { get; set; } 
    public string? Ip { get; private set; }
    public int? Numwant { get; set; }
    public string? Key { get; private set; }
    public string? Trackerid { get; private set; }

    public TrackerRequestParameters(byte[] info)
    {
        var hash = SHA1.HashData(info);
        Info_Hash = HttpUtility.UrlEncode(hash);
        List<byte> peerId = Guid.NewGuid().ToByteArray().ToList();
        peerId.AddRange(Guid.NewGuid().ToByteArray().ToList());
        peerId = peerId.SkipLast(peerId.Count - 20).ToList();

        Peer_Id = HttpUtility.UrlEncode(peerId.ToArray());
        Port = 6881;
        Event = TrackerEvent.Started;
    }

    public override string ToString()
    {
        List<string> parameters = new()
        {
            nameof(Info_Hash).ToLower() + "=" + Info_Hash,
            nameof(Peer_Id).ToLower() + "=" + Peer_Id,
            nameof(Port).ToLower() + "=" + Port,
            nameof(Uploaded).ToLower() + "=" + Uploaded.ToString(),
            nameof(Downloaded).ToLower() + "=" + Downloaded.ToString(),
            nameof(Left).ToLower() + "=" + Left,
            Compact == null ? string.Empty : nameof(Compact).ToLower() + "=" + Compact,
            nameof(Event).ToLower() + "=" + Event.ToString().ToLower(),
            Ip == null ? string.Empty : nameof(Ip).ToLower() + "=" + Ip,
            Numwant == null ? string.Empty : nameof(Numwant).ToLower() + "=" + Numwant,
            Key == null ? string.Empty : nameof(Key).ToLower() + "=" + Key,
            Trackerid == null ? string.Empty : nameof(Trackerid).ToLower() + "=" + Trackerid
        };

        return string.Join("&", parameters.Where(p => p != string.Empty));
    }
}