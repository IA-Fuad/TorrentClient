using System.Text;
using BEncoding;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace TorrentClientTest;

public class BEncodingTest
{
    [Theory]
    [InlineData("i3e", 3)]
    [InlineData("i-3e", -3)]
    [InlineData("i0e", 0)]
    public void GetDecodedObject_Int_ReturnsInt(string encodedString, int expectedInt)
    {
        // Arrange
        var encodedBytes = GetBytes(encodedString);

        // Act
        bool isInt = BEncoding.BEncoding.DecodeBEncodedBytes(encodedBytes).TryParseInteger(out int intValue);

        // Assert
        Assert.True(isInt);
        Assert.Equal(expectedInt, intValue);
    }

    [Theory]
    [InlineData("4:spam", "spam")]
    [InlineData("0:", "")]
    public void GetDecodedObject_String_ReturnsString(string encodedString, string expectedString)
    {
        // Arrange
        var encodedBytes = GetBytes(encodedString);

        // Act
        bool isString  = BEncoding.BEncoding.DecodeBEncodedBytes(encodedBytes).TryParseString(out string str);

        // Asset
        Assert.True(isString);
        Assert.Equal(expectedString, str);
    }

    [Fact]
    public void GetDecodedObject_Dictionary_ReturnsDictionaryStringValues()
    {
        // Arrange 
        var encodedBytes = GetBEncodedDictionaryString();

        // Act
        bool isDictionary = BEncoding.BEncoding.DecodeBEncodedBytes(encodedBytes).TryParseDictionary(out var dictionary);

        // Assert
        Assert.True(isDictionary);
        Assert.Equal(2, dictionary.Count);
        Assert.Collection(dictionary, 
            e1 =>
            {
                Assert.Equal("cow", e1.Key);
                e1.Value.TryParseString(out string str);
                Assert.Equal("moo", str);
            }, 
            e2 => 
            {
                Assert.Equal("spam", e2.Key);
                e2.Value.TryParseString(out string str);
                Assert.Equal("eggs", str);
            });
    }    

    [Fact]
    public void GetDecodedObject_Dictionary_ReturnsDictionaryListValues()
    {
        // Arrange
        var encodedBytes = GetBEncodedDictionaryList();

        // Act
        bool isDict = BEncoding.BEncoding.DecodeBEncodedBytes(encodedBytes).TryParseDictionary(out var dictionary);

        // Assert
        Assert.True(isDict);
        Assert.Collection(dictionary, 
            e => 
            {
                Assert.Equal("cow", e.Key);
                Assert.Equal(BDecodedObjectType.List, e.Value.Type);
                
                e.Value.TryParseList(out var list);
                Assert.Collection(list, 
                    item1 => 
                    {
                        item1.TryParseString(out string str);
                        Assert.Equal("spam", str);
                    },
                    item2 => 
                    {
                        item2.TryParseString(out string str);
                        Assert.Equal("eggs", str);
                    });
            });
    }

    [Fact]
    public void GetDecodedObject_List_ReturnsList()
    {
        // Arrange
        var encodedBytes = GetBEncodedList();

        // Act
        bool isList = BEncoding.BEncoding.DecodeBEncodedBytes(encodedBytes).TryParseList(out List<BDecodedObject> list);

        // Assert
        Assert.True(isList);
        Assert.Collection(list, 
            e1 => 
            {
                bool isString = e1.TryParseString(out string str);
                Assert.True(isString);
                Assert.Equal("spam", str);
            },
            e2 => 
            {
                bool isString = e2.TryParseString(out string str);
                Assert.True(isString);
                Assert.Equal("eggs", str);
            });
    }

    [Fact]
    public void GetBEncodedBytes_List_ReturnsBEncodedList()
    {
        var encodedBytes = GetBEncodedList();
        var decodedObject = BEncoding.BEncoding.DecodeBEncodedBytes(encodedBytes);

        var reEncodedBytes = BEncoding.BEncoding.GetBEncodedBytes(decodedObject);

        Assert.Equal(encodedBytes, reEncodedBytes);
    }

    [Fact]
    public void GetBEncodedBytes_Dictionary_ReturnsDictionarList()
    {
        var encodedBytes = GetBEncodedDictionaryList();
        var decodedObject = BEncoding.BEncoding.DecodeBEncodedBytes(encodedBytes);

        var reEncodedBytes = BEncoding.BEncoding.GetBEncodedBytes(decodedObject);

        Assert.Equal(encodedBytes, reEncodedBytes);
    }

    [Fact]
    public void GetBEncodedBytes_Dictionary_ReturnsDictionaryStrings()
    {
        var encodedBytes = GetBEncodedDictionaryString();
        var decodedObject = BEncoding.BEncoding.DecodeBEncodedBytes(encodedBytes);

        var reEncodedBytes = BEncoding.BEncoding.GetBEncodedBytes(decodedObject);

        Assert.Equal(encodedBytes, reEncodedBytes);
    }


    //[Fact]
    //public void GetBEncodedBytes_TorrentInfo_HashOk()
    //{
    //    string torrentFilePath = "/home/ifuad/Downloads/ubuntu-23.10.1-desktop-amd64.iso.torrent";
    //    byte[] torrentMetaDataBytes = File.ReadAllBytes(torrentFilePath);

    //    for (int i = 0; i < torrentMetaDataBytes.Length; i++)
    //    {
    //        if (torrentMetaDataBytes[i] == (byte)'i' && torrentMetaDataBytes[i+1] == (byte)'n' 
    //            && torrentMetaDataBytes[i+2] == (byte)'f' && torrentMetaDataBytes[i+3] == (byte)'o')
    //        {
    //            i += 3;
    //        }
    //    }
    //}

    private byte[] GetBEncodedList()
    {
        return GetBytes("l4:spam4:eggse");
    }

    private byte[] GetBEncodedDictionaryList()
    {
        return GetBytes("d3:cowl4:spam4:eggsee");
    }

    private byte[] GetBEncodedDictionaryString()
    {
        return GetBytes("d3:cow3:moo4:spam4:eggse");
    }

    private byte[] GetBytes(string encodedString)
    {
        return Encoding.UTF8.GetBytes(encodedString);
    }
}