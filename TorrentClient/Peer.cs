using MiscUtil.Conversion;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace TorrentClient;

public class Peer 
{
    public bool AmChoking { get; private set; }
    public bool AmInterested { get; private set; }
    public bool IsPeerChoking { get; private set; }
    public bool IsPeerInterested { get; private set; }

    public event EventHandler HandShaked;
    public event EventHandler InvalidPeer;
    public event EventHandler<int> HavePiece;

    private readonly string pstr = "BitTorrent protocol";

    private TcpClient _tcpClient;
    private NetworkStream _networkStream;

    private readonly string _peerId;
    private readonly string _ip;
    private readonly int _port;

    private bool _validPeer = true;
    private int _pieceCount;
    private bool _handShaked;

    public Peer(string peerId, string ip, int port)
    {
        _peerId = peerId;
        _ip = ip;
        _port = port;

        InvalidPeer += InvalidPeerEventHandler;
    }

    public async Task InitiateHandShake(byte[] infoHash, byte[] myPeerId, int pieceCount)
    {
        _pieceCount = pieceCount;
        _tcpClient = new();

        List<byte> message = new()
        {
            (byte)pstr.Length
        };
        message.AddRange(Encoding.UTF8.GetBytes(pstr));
        message.AddRange(Enumerable.Repeat((byte)0, 8));
        message.AddRange(infoHash);
        message.AddRange(myPeerId);

        await _tcpClient.ConnectAsync(_ip, _port);

        Console.WriteLine("Initiated handshake");

        _networkStream = _tcpClient.GetStream();

        ReadData();
        await _networkStream.WriteAsync(message.ToArray(), 0, message.Count);
    }

    private async Task ReadData()
    {
        byte[] dataToRecieve = new byte[4];
        int bytesRead;
        int messageLen;

        while (_validPeer)
        {
            bytesRead = await _networkStream.ReadAsync(dataToRecieve, 0, dataToRecieve.Length);
            if (bytesRead < 4) continue;

            messageLen = EndianBitConverter.Big.ToInt32(dataToRecieve, 0); 

            if (!_handShaked)
            {
                await DecodeHandShakeMessage(messageLen);                   
            }
            else
            {
                await ReadMessage(messageLen);
            }
        }
    }

    private async Task DecodeHandShakeMessage(int messageLen)
    {
        if (messageLen != 67)
        {
            InvalidPeer?.Invoke(this, EventArgs.Empty);
            return;
        }
    }

    private async Task ReadMessage(int messageLen)
    {
        int bytesRead;
        byte[] dataToRecieve = new byte[256];
        List<byte> message = new();

        while (messageLen > 0)
        {
            bytesRead = await _networkStream.ReadAsync(dataToRecieve, 0, Math.Min(messageLen, dataToRecieve.Length));
            messageLen -= bytesRead;

            message.AddRange(dataToRecieve.Take(bytesRead));
        }

        if (message.Count > 0)
        {
            MessageType messageType = (MessageType)message[0];
            switch (messageType)
            {
                
            }
        }
    }

    private void InvalidPeerEventHandler(object sender, EventArgs e)
    {
        _validPeer = false;
    }

    private async Task SendMessage(byte[] message)
    {
        await _networkStream.WriteAsync(message);
    }

    private void DecodeChokeMessage()
    {
        IsPeerChoking = true;
    }

    public async Task SendChokeMessage()
    {
        await SendMessage(MessageType.Choke.GetMessageBytes());
        AmChoking = true;
    }

    private void DecodeUnchokeMessage()
    {
        IsPeerChoking = false;
    }

    public async Task SendUnchokeMessage()
    {
        await SendMessage(MessageType.Unchoke.GetMessageBytes());
        AmChoking = false;
    }

    private void DecodeInterestedMessage()
    {
        IsPeerInterested = true;
    }

    public async Task SendInterestedMessage()
    {
        await SendMessage(MessageType.Interested.GetMessageBytes());
        AmInterested = true;
    }

    private void DecodeNotInterestedMessage()
    {
        IsPeerInterested = false;
    }

    public async Task SendNotInterestedMessage()
    {
        await SendMessage(MessageType.NotInterested.GetMessageBytes());
        AmInterested = false;
    }

    private void DecodeHaveMessage(byte[] message)
    {
        int pieceIndex = BigEndianBitConverter.Big.ToInt32(message, 0);
        HavePiece?.Invoke(this, pieceIndex);
    }

    public async Task SendHavePieceMessage(int pieceIndex)
    {
        await SendMessage(MessageType.Have.GetMessageBytes(BigEndianBitConverter.Big.GetBytes(pieceIndex)));
    }

    private void DecodeBitfieldMessage(byte[] message)
    {
        if (message.Length * 8 < _pieceCount)
        {
            InvalidPeer?.Invoke(this, EventArgs.Empty);
            return;
        }

        int currentPiece = 0;
        for (int i = 0; i < message.Length; i++)
        {
            int bitIndex = 0;
            for (int k = 0; k < 8; k++)
            {
                if (((1 << bitIndex) & message[i]) != 0)
                {
                    if (currentPiece > _pieceCount)
                    {
                        InvalidPeer?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                    HavePiece?.Invoke(this, currentPiece);
                }
                currentPiece++;
            }
        }
    }


}