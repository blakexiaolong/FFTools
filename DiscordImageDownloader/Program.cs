using HtmlAgilityPack;
using System.IO;
using System.Net;

namespace DiscordImageDownloader
{
    class Program
    {
        public const string IN_PATH = @"C:\Users\Andrew\Downloads\discord-image-downloader";
        public const string OUT_PATH = @"C:\Users\Andrew\Downloads\discord-image-downloader\output";

        static void Main()
        {
            Directory.CreateDirectory(IN_PATH);
            Directory.CreateDirectory(OUT_PATH);
            using (WebClient client = new WebClient())
            {
                foreach (string fileName in Directory.EnumerateFiles(IN_PATH))
                {
                    string fileFolder = Path.Combine(OUT_PATH, Path.GetFileNameWithoutExtension(fileName));
                    Directory.CreateDirectory(fileFolder);

                    HtmlDocument html = new HtmlDocument();
                    html.LoadHtml(File.ReadAllText(fileName));
                    foreach (var tag in html.DocumentNode.SelectNodes("//*[@class=\"chatlog__attachment\"]//a//img"))
                    {
                        foreach (var attribute in tag.Attributes.AttributesWithName("src"))
                        {
                            string[] splits = attribute.Value.Split('/');
                            string filePath = Path.Combine(fileFolder, $"{splits[^2]}-{splits[^1]}");
                            if (File.Exists(filePath)) continue;

                            client.DownloadFile(attribute.Value, filePath);
                        }
                    }
                }
            }
        }
    }
}