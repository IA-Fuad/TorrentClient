using System.Collections.Immutable;
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

    public static byte[] GetBEncodedBytes(BDecodedObject bDecodedObject)
    {
        if (bDecodedObject.Type == BDecodedObjectType.Integer)
        {
            return GetBEncodedInteger(bDecodedObject);
        }
        if (bDecodedObject.Type == BDecodedObjectType.String)
        {
            return GetBEncodedString(bDecodedObject);
        }
        if (bDecodedObject.Type == BDecodedObjectType.List)
        {
            return GetBEncodedList(bDecodedObject);
        }
        return GetBEncodedDictionary(bDecodedObject);
    }

    private static byte[] GetBEncodedDictionary(BDecodedObject bDecodedObject)
    {
        bDecodedObject.TryParseDictionary(out var dictValue);
        List<byte[]> dictItems = new()
        {
            Encoding.UTF8.GetBytes("d")
        };
        
        List<string> sortedDictKeys = dictValue.Keys.ToList();
        sortedDictKeys.Sort((k1, k2) => 
        {
            var k1Bytes = Encoding.UTF8.GetBytes(k1);
            var k2Bytes = Encoding.UTF8.GetBytes(k2);
            bool swaped = false;

            if (k1Bytes.Length > k2Bytes.Length)
            {
                (k1Bytes, k2Bytes) = (k2Bytes, k1Bytes);
                swaped = true;
            }
            for (int i = 0; i < k1Bytes.Length; i++)
            {
                if (k1Bytes[i] < k2Bytes[i])
                {
                    return swaped ? 1 : -1;
                }
                if (k1Bytes[1] > k2Bytes[i])
                {
                    return swaped ? -1 : 1;
                }
            }
            return swaped ? 1 : -1;
        });

        foreach (string key in sortedDictKeys)
        {
            byte[] bEncodedKey = GetBEncodedString(new BDecodedObject(BDecodedObjectType.String, key));
            byte[] bEncodedValue = GetBEncodedBytes(dictValue[key]);
            dictItems.Add(bEncodedKey);
            dictItems.Add(bEncodedValue);
        }
        dictItems.Add(Encoding.UTF8.GetBytes("e"));

        return dictItems.SelectMany(di => di).ToArray();
    }

    private static byte[] GetBEncodedList(BDecodedObject bDecodedObject)
    {
        bDecodedObject.TryParseList(out var listValue);
        List<byte[]> listItems = new()
        {
            Encoding.UTF8.GetBytes("l")
        };
        foreach (var item in listValue)
        {
            listItems.Add(GetBEncodedBytes(item));
        }
        listItems.Add(Encoding.UTF8.GetBytes("e"));

        return listItems.SelectMany(li => li).ToArray();
    }

    private static byte[] GetBEncodedString(BDecodedObject bDecodedObject)
    {
        bDecodedObject.TryParseString(out string stringValue);
        string bEncodedString = stringValue.Length.ToString() + ":" + stringValue;
        return Encoding.UTF8.GetBytes(bEncodedString);
    }

    private static byte[] GetBEncodedInteger(BDecodedObject bDecodedObject)
    {
        bDecodedObject.TryParseInteger(out int intValue);
        string bEncodedInt = "i" + intValue.ToString() + "e";
        return Encoding.UTF8.GetBytes(bEncodedInt);
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