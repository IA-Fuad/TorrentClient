namespace TorrentClient;

public class TorrentMetaDataInfo
{
    public int PieceLength { get; private set; }
    public string Pieces { get; private set; }
    public bool? Private { get; private set; }
    public string FileName { get; private set; }
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
        if (!infoDict.TryGetValue("name", out var fileNameValue)
            || !fileNameValue.TryParseString(out string fileName))
        {
            throw new InvalidDataException("info file name does not have a valid value");
        }
        
        if (infoDict.TryGetValue("private", out var privateValue))
        {
            privateValue.TryParseInteger(out int isPrivate);
            Private = isPrivate == 1;
        }
        PieceLength = pieceLen;
        Pieces = pieces;
        FileName = fileName;

        Files = new();
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
                if (!fileDict.TryParseDictionary(out var fileInfoDict))
                {
                    throw new InvalidDataException("file info is not a valid dictionary");
                }
                Files.Add(CreateFileInfo(fileInfoDict, true));
            }
        }
        Files.Add(CreateFileInfo(infoDict));
    }

    private FileInfo CreateFileInfo(Dictionary<string, BEncoding.BDecodedObject> fileInfoDict, bool isMultipleFiles = false)
    {
        string? md5sum = null;
        List<string> path;

        if (!fileInfoDict.TryGetValue("length", out var fileLenValue)
            || !fileLenValue.TryParseInteger(out int fileLen))
        {
            throw new InvalidDataException("file length does not have a valid value"); 
        }
        if (fileInfoDict.TryGetValue("md5sum", out var md5sumValue))
        {
            md5sumValue.TryParseString(out md5sum);
        }

        if (isMultipleFiles)
        {
            path = new();
            if (!fileInfoDict.TryGetValue("path", out var pathValue)
                || !pathValue.TryParseList(out var pathList))
            {
                throw new InvalidDataException("file path does not contain a valid value");
            }
            foreach (var p in pathList)
            {
                if (p.TryParseString(out string pathName))
                {
                    path.Add(pathName);
                }
                else 
                {
                    throw new InvalidDataException("file path does not have a valid string value");
                }
            }
            return new(fileLen, path, md5sum);
        }
        return new(fileLen, md5sum);
    }
}