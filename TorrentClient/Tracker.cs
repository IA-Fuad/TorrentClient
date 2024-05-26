using System.Net;
using System.Text;

namespace TorrentClient;

public class Tracker
{
    private readonly HttpClient _httpClient;
    private readonly string _failureReason = "failure reason";
    private readonly string _warningMessage = "warning message";
    private readonly string _interval = "interval";
    private readonly string _minInverval = "min interval";
    private readonly string _trackerId = "tracker id";
    private readonly string _complete = "complete";
    private readonly string _incomplete = "incomplete";
    private readonly string _peers = "peers";
    private readonly string _peerId = "peer id";
    private readonly string _ip = "ip";
    private readonly string _port = "port";

    public Tracker(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Tracker(string announceUrl)
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(announceUrl)
        };
    }

    public async Task<List<Peer>> GetPeers(TrackerRequestParameters requestParameters)
    {
        List<Peer> peers = new();
        
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync("?" + requestParameters.ToString());
        using Stream trackerResponseStream = await responseMessage.Content.ReadAsStreamAsync();
        byte[] trackerResponse = new byte[trackerResponseStream.Length];
        trackerResponseStream.Read(trackerResponse, 0, trackerResponse.Length);

        // File.WriteAllBytes(@"F:\Projects\TorrentClient\InfoHash\trackerResponse1.txt", Encoding.UTF8.GetBytes(trackerResponse));

        var decodedResponse = BEncoding.BEncoding.DecodeBEncodedBytes(trackerResponse); 

        if (decodedResponse.TryParseDictionary(out Dictionary<string, BEncoding.BDecodedObject> dict))
        {
            if (dict.ContainsKey(_failureReason))
            {
                return peers;
            }
            ParsePeers(dict, peers);
        }

        return peers;
    }

    private void ParsePeers(Dictionary<string, BEncoding.BDecodedObject> dict, List<Peer> peers)
    {
        if (dict.TryGetValue(_peers, out var peersValue))
        {
            if (peersValue.TryParseList(out var peerDictList))
            {
                foreach(var peerDict in peerDictList)
                {
                    if (peerDict.TryParseDictionary(out var peer))
                    {
                        peer[_peerId].TryParseString(out string peerId);
                        peer[_ip].TryParseString(out string ip);
                        peer[_port].TryParseInteger(out int port);
                        peers.Add(new Peer(
                            peerId,
                            ip,
                            port 
                        ));
                    }
                }
            }
            else if (peersValue.TryParseString(out string peerListString))
            {
                // To do
            }
        }
    }
}