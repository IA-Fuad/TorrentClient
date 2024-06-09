using MiscUtil.Conversion;

namespace TorrentClient
{
    internal enum MessageType
    {
        Choke = 0,
        Unchoke = 1,
        Interested = 2,
        NotInterested = 3,
        Have = 4,
        Bitfield = 5,
        Request = 6,
        Piece = 7,
        Cancel = 8,
        Port = 9
    }

    internal static class MessageTypeExtension
    {
        public static byte[] GetMessageBytes(this MessageType messageType, byte[]? payload = null)
        {
            int payloadLen = (payload is null ?  0 : payload.Length);
            int messageLen = messageType switch
            {
                MessageType.Have => 5,
                MessageType.Bitfield => 1 + payloadLen,
                MessageType.Request => 13,
                MessageType.Piece => 9 + payloadLen,
                MessageType.Cancel => 13,
                MessageType.Port => 3,
                _ => 1,
            };

            byte[] message = new byte[messageLen + 4];
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(messageLen), 0, message, 0, 4);
            message[5] = Convert.ToByte((int)messageType);
            if (payload != null )
            {
                Buffer.BlockCopy(payload, 0, message, 5, payloadLen);
            }

            return message;
        }
    }
}
