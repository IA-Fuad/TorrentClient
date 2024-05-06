using System.Text;

namespace BEncoding;

public static class BEncoding
{
    private static readonly byte _integerStart = Convert.ToByte('i');
    private static readonly byte _listStart = Convert.ToByte('l');
    private static readonly byte _dictionaryStart = Convert.ToByte('d');
    private static readonly byte _end = Convert.ToByte('e');

    public static BDecodedObject DecodeBEncodedBytes(byte[] encodedBytes)
    {
        var enumerator = encodedBytes.AsEnumerable().GetEnumerator();
        enumerator.MoveNext();
        
        return GetDecodedObject(enumerator);
    }

    private static BDecodedObject GetDecodedObject(IEnumerator<byte> enumerator)
    {
        if (enumerator.Current == _integerStart)
        {
            return DecodeInteger(enumerator);
        }
        if (enumerator.Current == _listStart)
        {
            return DecodeList(enumerator);
        }
        if (enumerator.Current == _dictionaryStart)
        {
            return DecodeDictionary(enumerator);
        }
        return DecodeByteString(enumerator);
    }

    private static BDecodedObject DecodeByteString(IEnumerator<byte> enumerator)
    {
        int len = ParseInt(enumerator);
        enumerator.MoveNext();

        byte[] byteString = new byte[len];
        
        for (int i = 0; i < len; i++) 
        {
            byteString[i] = enumerator.Current;
            enumerator.MoveNext();
        }

        return new BDecodedObject(BDecodedObjectType.String, Encoding.UTF8.GetString(byteString));
    }

    private static int ParseInt(IEnumerator<byte> enumerator)
    {
        int symbol = 1;
        char firstChar = Convert.ToChar(enumerator.Current);
        if (firstChar == '-')
        {
            symbol = -1;
            enumerator.MoveNext();
        }
        int num = Convert.ToChar(enumerator.Current) - '0';

        while (enumerator.MoveNext())
        {
            char currentChar = Convert.ToChar(enumerator.Current);
            if (!char.IsDigit(currentChar))
            {
                break;
            }
            num = (num * 10) + (currentChar - '0');
        }

        return num * symbol;
    }

    private static BDecodedObject DecodeDictionary(IEnumerator<byte> enumerator)
    {
        Dictionary<string, BDecodedObject> decodedDict = new();
        
        bool isValid = enumerator.MoveNext();
        while (enumerator.Current != _end)
        {
            DecodeByteString(enumerator).TryParseString(out string key);

            var value = GetDecodedObject(enumerator);
            
            decodedDict.Add(key, value);
        }
        enumerator.MoveNext();

        return new BDecodedObject(BDecodedObjectType.Dictionary, decodedDict);
    }

    private static BDecodedObject DecodeList(IEnumerator<byte> enumerator)
    {
        List<BDecodedObject> decodedList = new();

        enumerator.MoveNext();
        while (enumerator.Current != _end)
        {
            decodedList.Add(GetDecodedObject(enumerator));
        }
        enumerator.MoveNext();

        return new BDecodedObject(BDecodedObjectType.List, decodedList);
    }

    private static BDecodedObject DecodeInteger(IEnumerator<byte> enumerator)
    {
        enumerator.MoveNext();
        int intValue = ParseInt(enumerator);
        
        if (enumerator.Current != _end)
        {
            return new BDecodedObject(BDecodedObjectType.Invalid, 0);
        }
        enumerator.MoveNext();
        
        return new BDecodedObject(BDecodedObjectType.Integer, intValue);
    }
}