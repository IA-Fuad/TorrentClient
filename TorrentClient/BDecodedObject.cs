namespace BEncoding;

public enum BDecodedObjectType 
{
    Invalid = 0,
    String = 1,
    Integer = 2,
    List = 3,
    Dictionary = 4
}

public class BDecodedObject
{
    public BDecodedObjectType Type { get; private set; }
    public object DecodedObject { get; private set; }

    public BDecodedObject(BDecodedObjectType type, object decodedObject)
    {
        Type = type;
        DecodedObject = decodedObject;
    }

    public bool TryParseString(out string byteString)
    {
        if (Type == BDecodedObjectType.String)
        {
            byteString = DecodedObject as string;
            return true;
        }
        
        byteString = null;
        return false;
    }

    public bool TryParseInteger(out int intValue)
    {
        intValue = 0;
        try 
        {
            if (Type == BDecodedObjectType.Integer)
            {
                intValue = (int)DecodedObject;
                return true;
            } 
            return false;
        }
        catch 
        {
            return false;
        }
    }

    public bool TryParseList(out List<BDecodedObject> list)
    {
        list = null;
        try 
        {
            if (Type == BDecodedObjectType.List)
            {
                list = DecodedObject as List<BDecodedObject>;
                return true;
            }
            return false;
        }
        catch 
        {
            return false;
        }
    }

    public bool TryParseDictionary(out Dictionary<string, BDecodedObject> dict)
    {
        dict = null;
        try 
        {
            if (Type == BDecodedObjectType.Dictionary)
            {
                dict = DecodedObject as Dictionary<string, BDecodedObject>;
                return true;
            }
            return false;
        }
        catch 
        {
            return false;
        }
    }
}