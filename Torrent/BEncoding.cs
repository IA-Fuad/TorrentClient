using System.Text;

namespace Torrent;
public static class BEncoding
{
    private static readonly byte DictionaryStart = Encoding.UTF8.GetBytes("d")[0];
    private static readonly byte DictionaryEnd = Encoding.UTF8.GetBytes("e")[0];
    private static readonly byte ListStart = Encoding.UTF8.GetBytes("l")[0];
    private static readonly byte ListEnd = Encoding.UTF8.GetBytes("e")[0];
    private static readonly byte NumberStart = Encoding.UTF8.GetBytes("i")[0];
    private static readonly byte NumberEnd = Encoding.UTF8.GetBytes("e")[0];
    private static readonly byte ByteArrayDivider = Encoding.UTF8.GetBytes(":")[0];

    private static object Decode(byte[] bytes)
    {
        IEnumerator<byte> enumerator = ((IEnumerable<byte>)bytes).GetEnumerator();
        enumerator.MoveNext();

        return DecodeNextObject(enumerator);
    }

    private static object DecodeNextObject(IEnumerator<byte> enumerator)
    {
        if (enumerator.Current == DictionaryStart) 
        {
            return DecodeDictionary(enumerator);
        }
        if (enumerator.Current == ListStart) 
        {
            return DecodeList(enumerator);
        }
        if (enumerator.Current == NumberStart) 
        {
            return DecodeNumber(enumerator);
        }
        return DecodeByteArray(enumerator);
    }

    private static object DecodeFile(string path) 
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Unable to find file: " + path);
        }

        byte[] bytes = File.ReadAllBytes(path);

        return Decode(bytes);
    }

    private static byte[] DecodeByteArray(IEnumerator<byte> enumerator)
    {
        List<byte> byteLength = new();
        do 
        {
            if (enumerator.Current == ByteArrayDivider)
            {
                break;
            }

            byteLength.Add(enumerator.Current);
        } while (enumerator.MoveNext());

        int len = int.Parse(Encoding.UTF8.GetString(byteLength.ToArray()));

        byte[] bytes = new byte[len];
        for (int i = 0; i < len; i++) 
        {
            enumerator.MoveNext();
            bytes[i] = enumerator.Current;
        }

        return bytes;
    }

    private static long DecodeNumber(IEnumerator<byte> enumerator)
    {
        List<byte> bytes = new();

        while (enumerator.MoveNext()) 
        {
            if (enumerator.Current == NumberEnd)
            {
                break;
            }

            bytes.Add(enumerator.Current);
        }

        return long.Parse(Encoding.UTF8.GetString(bytes.ToArray()));
    }

    private static List<object> DecodeList(IEnumerator<byte> enumerator)
    {
        List<object> list = new();

        while (enumerator.MoveNext()) 
        {
            if (enumerator.Current == ListEnd)
            {
                break;
            }

            list.Add(DecodeNextObject(enumerator));
        }

        return list;
    }

    private static Dictionary<string, object> DecodeDictionary(IEnumerator<byte> enumerator)
    {
        Dictionary<string, object> dict = new();
        List<string> keys = new();

        while (enumerator.MoveNext())
        {
            if (enumerator.Current == DictionaryEnd)
            {
                break;
            }

            string key = Encoding.UTF8.GetString(DecodeByteArray(enumerator));
            keys.Add(key);
            enumerator.MoveNext();

            dict.Add(key, DecodeNextObject(enumerator));
        }

        var sortedKeys = keys.OrderBy(k => BitConverter.ToString(Encoding.UTF8.GetBytes(k)));
        if (!keys.SequenceEqual(sortedKeys)) 
        {
            throw new Exception("Error loading Dictionary. Keys are not sorted.");
        }
        
        return dict;
    }
}
