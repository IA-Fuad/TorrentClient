namespace TorrentClient;

public class TorrentMetaDataInfo
{
    public int PieceLength { get; private set; }
    public string Pieces { get; private set; }
    public bool? Private { get; private set; }
    public List<FileInfo> Files { get; private set; }

    public TorrentMetaDataInfo(BEncoding.BDecodedObject metaDataInfo)
    {
        if (!metaDataInfo.TryParseDictionary(out var infoDict))
        {
            throw new ArgumentException("info value is not a dictionary");
        }

        if (!infoDict.TryGetValue("piece length", out var pieceLenValue)
            || !pieceLenValue.TryParseInteger(out int pieceLen))
        {
            throw new InvalidDataException("info piece length does not have a valid value");
        }
        if (!infoDict.TryGetValue("pieces", out var piecesValue)
            || !piecesValue.TryParseString(out string pieces))
        {
            throw new InvalidDataException("info pieces does not have valid a value");
        }
        
        if (infoDict.TryGetValue("private", out var privateValue))
        {
            privateValue.TryParseInteger(out int isPrivate);
            Private = isPrivate == 1;
        }
        PieceLength = pieceLen;
        Pieces = pieces;

        ParseFileInfo(infoDict);
    }

    private void ParseFileInfo(Dictionary<string, BEncoding.BDecodedObject> infoDict)
    {
        if (infoDict.TryGetValue("files", out var filesValue))
        {
            if (!filesValue.TryParseList(out var fileDictList))
            {
                throw new InvalidDataException("info files does not contain a valid dictionary list");
            }
            
            foreach (var fileDict in fileDictList)
            {
                
            }
        }
    }
}