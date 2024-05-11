// See https://aka.ms/new-console-template for more information
using TorrentClient;

string torrentFilePath = Console.ReadLine();

Torrent torrent = new(torrentFilePath);

Console.ReadKey();