using System.Diagnostics;
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
        MemoryStream ms = new();
        EncodeDecodedObject(bDecodedObject, ms);

        return ms.ToArray();
    }

    private static void EncodeDecodedObject(BDecodedObject bDecodedObject, MemoryStream ms)
    {
        if (bDecodedObject.Type == BDecodedObjectType.Integer)
        {
            GetBEncodedInteger(bDecodedObject, ms);
        }
        else if (bDecodedObject.Type == BDecodedObjectType.String)
        {
            GetBEncodedString(bDecodedObject, ms);
        }
        else if (bDecodedObject.Type == BDecodedObjectType.List)
        {
            GetBEncodedList(bDecodedObject, ms);
        }
        else if (bDecodedObject.Type == BDecodedObjectType.Dictionary)
        {
            GetBEncodedDictionary(bDecodedObject, ms);
        }
    }

    private static void GetBEncodedDictionary(BDecodedObject bDecodedObject, MemoryStream ms)
    {
        bDecodedObject.TryParseDictionary(out var dictValue);
        ms.WriteByte(_dictionaryStart);
        
        List<string> sortedDictKeys = dictValue.Keys.OrderBy(k => BitConverter.ToString(Encoding.UTF8.GetBytes(k))).ToList();

        foreach (string key in sortedDictKeys)
        {
            GetBEncodedString(new BDecodedObject(BDecodedObjectType.String, Encoding.UTF8.GetBytes(key)), ms);
            EncodeDecodedObject(dictValue[key], ms);
        }
        ms.WriteByte(_end);
    }

    private static void GetBEncodedList(BDecodedObject bDecodedObject, MemoryStream ms)
    {
        bDecodedObject.TryParseList(out var listValue);
        ms.WriteByte(_listStart);
        foreach (var item in listValue)
        {
            EncodeDecodedObject(item, ms);
        }
        ms.WriteByte(_end);
    }

    private static void GetBEncodedString(BDecodedObject bDecodedObject, MemoryStream ms)
    {
        bDecodedObject.TryParseStringBytes(out byte[] byteString);
        ms.Write(Encoding.UTF8.GetBytes(byteString.Length.ToString()));
        ms.WriteByte(Convert.ToByte(':'));
        ms.Write(byteString);
    }

    private static void GetBEncodedInteger(BDecodedObject bDecodedObject, MemoryStream ms)
    {
        bDecodedObject.TryParseIntegerLong(out long intValue);
        ms.WriteByte(_integerStart);
        ms.Write(Encoding.UTF8.GetBytes(intValue.ToString()));
        ms.WriteByte(_end);
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
        long len = ParseInt(enumerator);
        enumerator.MoveNext();

        byte[] byteString = new byte[len];
        
        for (int i = 0; i < len; i++) 
        {
            byteString[i] = enumerator.Current;
            enumerator.MoveNext();
        }

        return new BDecodedObject(BDecodedObjectType.String, byteString);
    }

    private static long ParseInt(IEnumerator<byte> enumerator)
    {
        if (!char.IsDigit(Convert.ToChar(enumerator.Current)))
        {
            Debugger.Break();
        }
        List<byte> numberBytes = new() { enumerator.Current };

        while (enumerator.MoveNext())
        {
            if (!char.IsDigit(Convert.ToChar(enumerator.Current)))
            {
                break;
            }
            numberBytes.Add(enumerator.Current);
        }

        string numString = Encoding.UTF8.GetString(numberBytes.ToArray());
        return long.Parse(numString);
    }

    private static BDecodedObject DecodeDictionary(IEnumerator<byte> enumerator)
    {
        Dictionary<string, BDecodedObject> decodedDict = new();

        enumerator.MoveNext();
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
        long longIntValue = ParseInt(enumerator);
        
        if (enumerator.Current != _end)
        {
            return new BDecodedObject(BDecodedObjectType.Invalid, 0);
        }
        enumerator.MoveNext();
        
        if (longIntValue > int.MaxValue)
        {
            return new(BDecodedObjectType.Integer, longIntValue);
        }
        int intValue = (int)longIntValue;
        return new(BDecodedObjectType.Integer, intValue);
    }
}