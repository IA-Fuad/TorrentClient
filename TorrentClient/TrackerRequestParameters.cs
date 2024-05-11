namespace TorrentClient;

public enum TrackerEvent
{
    Started,
    Stopped,
    Completed
}

public class TrackerRequestParameters
{
    public string Info_Hash { get; set; }
    public string Peer_Id { get; set; }
    public int Port { get; set; }
    public int Uploaded { get; set; }
    public int Downloaded { get; set; }
    public int Left { get; set; }
    public int? Compact { get; set; }
    public TrackerEvent Event { get; set; } 
    public string? Ip { get; set; }
    public int? Numwant { get; set; }
    public string? Key { get; set; }
    public string? Trackerid { get; set; }

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